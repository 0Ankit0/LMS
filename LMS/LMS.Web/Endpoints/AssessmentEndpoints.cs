using LMS.Data.Entities;
using LMS.Web.Data;
using LMS.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMS.Web.Endpoints
{
    public class AssessmentEndpoints : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/assessments").WithTags("Assessments");

            // Assessment submission
            group.MapPost("/{assessmentId:int}/submit", SubmitAssessment)
                .RequireAuthorization()
                .WithName("SubmitAssessment")
                .WithSummary("Submit assessment answers");

            // Manual grading endpoints
            group.MapGet("/pending-grading", GetPendingGradingAssessments)
                .RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"))
                .WithName("GetPendingGradingAssessments")
                .WithSummary("Get assessments pending manual grading");

            group.MapPost("/attempts/{attemptId:int}/grade", GradeAssessmentManually)
                .RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"))
                .WithName("GradeAssessmentManually")
                .WithSummary("Manually grade an assessment attempt");

            // Assessment results
            group.MapGet("/attempts/{attemptId:int}/results", GetAssessmentResults)
                .RequireAuthorization()
                .WithName("GetAssessmentResults")
                .WithSummary("Get assessment attempt results");
        }

        private static async Task<IResult> SubmitAssessment(
            int assessmentId,
            AssessmentSubmissionRequest request,
            ApplicationDbContext context,
            ClaimsPrincipal user)
        {
            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            // Get assessment and validate
            var assessment = await context.Assessments
                .Include(a => a.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.Id == assessmentId);

            if (assessment == null)
                return Results.NotFound("Assessment not found");

            // Get user's enrollment
            var enrollment = await context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId &&
                    (e.CourseId == assessment.CourseId || assessment.CourseId == null));

            if (enrollment == null)
                return Results.BadRequest("User not enrolled in this course");

            // Check if user has attempts remaining
            var attemptCount = await context.AssessmentAttempts
                .CountAsync(aa => aa.EnrollmentId == enrollment.Id && aa.AssessmentId == assessmentId);

            if (attemptCount >= assessment.MaxAttempts)
                return Results.BadRequest("Maximum attempts exceeded");

            // Create assessment attempt
            var attempt = new AssessmentAttempt
            {
                EnrollmentId = enrollment.Id,
                AssessmentId = assessmentId,
                AttemptNumber = attemptCount + 1,
                StartedAt = DateTime.UtcNow
            };

            context.AssessmentAttempts.Add(attempt);
            await context.SaveChangesAsync();

            // Process responses and auto-grade where possible
            var totalPoints = 0.0;
            var earnedPoints = 0.0;
            var hasManualGrading = false;

            foreach (var response in request.Responses)
            {
                var question = assessment.Questions.FirstOrDefault(q => q.Id == response.QuestionId);
                if (question == null) continue;

                totalPoints += question.Points;

                var questionResponse = new QuestionResponse
                {
                    AttemptId = attempt.Id,
                    QuestionId = response.QuestionId,
                    SelectedOptionId = response.SelectedOptionId,
                    TextResponse = response.TextResponse,
                    AnsweredAt = DateTime.UtcNow
                };

                // Auto-grade if possible
                var gradeResult = AutoGradeResponse(question, questionResponse);
                questionResponse.IsCorrect = gradeResult.IsCorrect;
                questionResponse.PointsEarned = gradeResult.PointsEarned;

                if (gradeResult.RequiresManualGrading)
                    hasManualGrading = true;
                else
                    earnedPoints += gradeResult.PointsEarned;

                context.QuestionResponses.Add(questionResponse);
            }

            // Update attempt with results
            attempt.CompletedAt = DateTime.UtcNow;
            attempt.TimeTaken = attempt.CompletedAt - attempt.StartedAt;

            if (hasManualGrading)
            {
                attempt.Status = AssessmentAttemptStatus.PendingManualGrading;
                // Don't set final score yet
            }
            else
            {
                attempt.Score = earnedPoints;
                attempt.Percentage = totalPoints > 0 ? (earnedPoints / totalPoints) * 100 : 0;
                attempt.IsPassed = attempt.Percentage >= assessment.PassingScore;
                attempt.Status = AssessmentAttemptStatus.Completed;
            }

            await context.SaveChangesAsync();

            return Results.Ok(new
            {
                AttemptId = attempt.Id,
                RequiresManualGrading = hasManualGrading,
                Score = attempt.Score,
                Percentage = attempt.Percentage,
                IsPassed = attempt.IsPassed
            });
        }

        private static AutoGradeResult AutoGradeResponse(Question question, QuestionResponse response)
        {
            switch (question.Type)
            {
                case QuestionType.MultipleChoice:
                case QuestionType.TrueFalse:
                    var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                    var isCorrect = correctOption != null && response.SelectedOptionId == correctOption.Id;
                    return new AutoGradeResult
                    {
                        IsCorrect = isCorrect,
                        PointsEarned = isCorrect ? question.Points : 0,
                        RequiresManualGrading = false
                    };

                case QuestionType.ShortAnswer:
                case QuestionType.Essay:
                    return new AutoGradeResult
                    {
                        IsCorrect = false, // Will be set during manual grading
                        PointsEarned = 0,
                        RequiresManualGrading = true
                    };

                case QuestionType.Matching:
                case QuestionType.FillInTheBlank:
                case QuestionType.Ordering:
                    // These would need more complex auto-grading logic
                    return new AutoGradeResult
                    {
                        IsCorrect = false,
                        PointsEarned = 0,
                        RequiresManualGrading = true
                    };

                default:
                    return new AutoGradeResult
                    {
                        IsCorrect = false,
                        PointsEarned = 0,
                        RequiresManualGrading = true
                    };
            }
        }

        private static async Task<IResult> GetPendingGradingAssessments(
            ApplicationDbContext context,
            int page = 1,
            int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;

            var pendingAttempts = await context.AssessmentAttempts
                .Include(aa => aa.Assessment)
                .Include(aa => aa.Enrollment)
                .ThenInclude(e => e.User)
                .Include(aa => aa.Enrollment)
                .ThenInclude(e => e.Course)
                .Where(aa => aa.Status == AssessmentAttemptStatus.PendingManualGrading)
                .OrderBy(aa => aa.CompletedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(aa => new
                {
                    AttemptId = aa.Id,
                    AssessmentTitle = aa.Assessment.Title,
                    StudentName = $"{aa.Enrollment.User.FirstName} {aa.Enrollment.User.LastName}",
                    StudentEmail = aa.Enrollment.User.Email,
                    CourseName = aa.Enrollment.Course.Title,
                    SubmittedAt = aa.CompletedAt,
                    AttemptNumber = aa.AttemptNumber
                })
                .ToListAsync();

            var totalCount = await context.AssessmentAttempts
                .CountAsync(aa => aa.Status == AssessmentAttemptStatus.PendingManualGrading);

            return Results.Ok(new
            {
                Data = pendingAttempts,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        private static async Task<IResult> GradeAssessmentManually(
            int attemptId,
            ManualGradingRequest request,
            ApplicationDbContext context)
        {
            var attempt = await context.AssessmentAttempts
                .Include(aa => aa.Assessment)
                .Include(aa => aa.Responses)
                .ThenInclude(r => r.Question)
                .FirstOrDefaultAsync(aa => aa.Id == attemptId);

            if (attempt == null)
                return Results.NotFound("Assessment attempt not found");

            if (attempt.Status != AssessmentAttemptStatus.PendingManualGrading)
                return Results.BadRequest("Assessment is not pending manual grading");

            var totalPoints = 0.0;
            var earnedPoints = 0.0;

            // Update manual grading scores
            foreach (var grading in request.QuestionGrades)
            {
                var response = attempt.Responses.FirstOrDefault(r => r.Id == grading.ResponseId);
                if (response == null) continue;

                response.IsCorrect = grading.IsCorrect;
                response.PointsEarned = grading.PointsEarned;

                totalPoints += response.Question.Points;
                earnedPoints += response.PointsEarned;
            }

            // Calculate final score
            attempt.Score = earnedPoints;
            attempt.Percentage = totalPoints > 0 ? (earnedPoints / totalPoints) * 100 : 0;
            attempt.IsPassed = attempt.Percentage >= attempt.Assessment.PassingScore;
            attempt.Status = AssessmentAttemptStatus.Completed;

            await context.SaveChangesAsync();

            return Results.Ok(new
            {
                Score = attempt.Score,
                Percentage = attempt.Percentage,
                IsPassed = attempt.IsPassed
            });
        }

        private static async Task<IResult> GetAssessmentResults(
            int attemptId,
            ApplicationDbContext context,
            ClaimsPrincipal user)
        {
            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isInstructor = user.IsInRole("Admin") || user.IsInRole("Instructor");

            var attempt = await context.AssessmentAttempts
                .Include(aa => aa.Assessment)
                .Include(aa => aa.Enrollment)
                .ThenInclude(e => e.User)
                .Include(aa => aa.Responses)
                .ThenInclude(r => r.Question)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(aa => aa.Id == attemptId);

            if (attempt == null)
                return Results.NotFound("Assessment attempt not found");

            // Check access rights
            if (!isInstructor && attempt.Enrollment.UserId != userId)
                return Results.Forbid();

            var result = new
            {
                AttemptId = attempt.Id,
                AssessmentTitle = attempt.Assessment.Title,
                StudentName = $"{attempt.Enrollment.User.FirstName} {attempt.Enrollment.User.LastName}",
                Score = attempt.Score,
                Percentage = attempt.Percentage,
                IsPassed = attempt.IsPassed,
                Status = attempt.Status.ToString(),
                StartedAt = attempt.StartedAt,
                CompletedAt = attempt.CompletedAt,
                TimeTaken = attempt.TimeTaken,
                Responses = attempt.Responses.Select(r => new
                {
                    QuestionId = r.QuestionId,
                    QuestionText = r.Question.Text,
                    QuestionType = r.Question.Type.ToString(),
                    SelectedOptionId = r.SelectedOptionId,
                    TextResponse = r.TextResponse,
                    IsCorrect = r.IsCorrect,
                    PointsEarned = r.PointsEarned,
                    MaxPoints = r.Question.Points,
                    Options = r.Question.Options.Select(o => new
                    {
                        OptionId = o.Id,
                        Text = o.Text,
                        IsCorrect = isInstructor ? o.IsCorrect : (bool?)null // Only show correct answers to instructors
                    })
                })
            };

            return Results.Ok(result);
        }
    }

    public class AssessmentSubmissionRequest
    {
        public List<QuestionResponseRequest> Responses { get; set; } = new();
    }

    public class QuestionResponseRequest
    {
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; }
        public string? TextResponse { get; set; }
    }

    public class ManualGradingRequest
    {
        public List<QuestionGradeRequest> QuestionGrades { get; set; } = new();
    }

    public class QuestionGradeRequest
    {
        public int ResponseId { get; set; }
        public bool IsCorrect { get; set; }
        public double PointsEarned { get; set; }
    }

    public class AutoGradeResult
    {
        public bool IsCorrect { get; set; }
        public double PointsEarned { get; set; }
        public bool RequiresManualGrading { get; set; }
    }
}
