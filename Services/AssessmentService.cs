using LMS.Data;
using LMS.Models.Assessment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LMS.Services;

public class AssessmentService
{
    private readonly IDbContextFactory<AuthDbContext> _contextFactory;

    public AssessmentService(IDbContextFactory<AuthDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<AssessmentModel>> GetAssessmentsAsync()
    {
        await using var _context = _contextFactory.CreateDbContext();
        var assessments = await _context.Assessments
            .Include(a => a.Questions)
                .ThenInclude(q => q.Options)
            .Include(a => a.Course)
            .Include(a => a.Module)
            .Include(a => a.Lesson)
            .ToListAsync();

        return assessments.Select(MapToAssessmentModel);
    }

    public async Task<IEnumerable<AssessmentModel>> GetAssessmentsByCourseAsync(int courseId)
    {
        await using var _context = _contextFactory.CreateDbContext();
        var assessments = await _context.Assessments
            .Where(a => a.CourseId == courseId)
            .Include(a => a.Questions)
                .ThenInclude(q => q.Options)
            .Include(a => a.Course)
            .Include(a => a.Module)
            .Include(a => a.Lesson)
            .ToListAsync();

        return assessments.Select(MapToAssessmentModel);
    }

    public async Task<AssessmentModel?> GetAssessmentAsync(int id)
    {
        await using var _context = _contextFactory.CreateDbContext();
        var assessment = await _context.Assessments
            .Include(a => a.Questions)
                .ThenInclude(q => q.Options)
            .Include(a => a.Course)
            .Include(a => a.Module)
            .Include(a => a.Lesson)
            .FirstOrDefaultAsync(a => a.Id == id);

        return assessment != null ? MapToAssessmentModel(assessment) : null;
    }

    public async Task<AssessmentModel> CreateAssessmentAsync(CreateAssessmentRequest request)
    {
        await using var _context = _contextFactory.CreateDbContext();
        var assessment = new Assessment
        {
            Title = request.Title,
            Description = request.Description,
            Type = (AssessmentType)request.Type,
            CourseId = request.CourseId,
            MaxAttempts = request.MaxAttempts,
            TimeLimit = request.TimeLimit,
            PassingScore = request.PassingScore,
            AvailableFrom = request.AvailableFrom,
            AvailableUntil = request.AvailableUntil,
            IsRandomized = request.IsRandomized,
            ShowCorrectAnswers = request.ShowCorrectAnswers,
            ShowScoreImmediately = request.ShowScoreImmediately,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();

        return await GetAssessmentAsync(assessment.Id) ?? throw new InvalidOperationException("Assessment not found after creation");
    }

    public async Task<AssessmentModel> UpdateAssessmentAsync(int id, CreateAssessmentRequest request)
    {
        await using var _context = _contextFactory.CreateDbContext();
        var assessment = await _context.Assessments.FindAsync(id);
        if (assessment == null)
            throw new ArgumentException($"Assessment with ID {id} not found");

        assessment.Title = request.Title;
        assessment.Description = request.Description;
        assessment.Type = (AssessmentType)request.Type;
        assessment.CourseId = request.CourseId;
        assessment.MaxAttempts = request.MaxAttempts;
        assessment.TimeLimit = request.TimeLimit;
        assessment.PassingScore = request.PassingScore;
        assessment.AvailableFrom = request.AvailableFrom;
        assessment.AvailableUntil = request.AvailableUntil;
        assessment.IsRandomized = request.IsRandomized;
        assessment.ShowCorrectAnswers = request.ShowCorrectAnswers;
        assessment.ShowScoreImmediately = request.ShowScoreImmediately;
        assessment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetAssessmentAsync(id) ?? throw new InvalidOperationException("Assessment not found after update");
    }

    public async Task<bool> DeleteAssessmentAsync(int id)
    {
        await using var _context = _contextFactory.CreateDbContext();
        var assessment = await _context.Assessments.FindAsync(id);
        if (assessment == null)
            return false;

        _context.Assessments.Remove(assessment);
        await _context.SaveChangesAsync();
        return true;
    }

    private static AssessmentModel MapToAssessmentModel(Assessment assessment)
    {
        return new AssessmentModel
        {
            Id = assessment.Id,
            Title = assessment.Title,
            Description = assessment.Description,
            Type = assessment.Type.ToString(),
            CourseId = assessment.CourseId,
            MaxAttempts = assessment.MaxAttempts,
            TimeLimit = assessment.TimeLimit,
            PassingScore = assessment.PassingScore,
            AvailableFrom = assessment.AvailableFrom,
            AvailableUntil = assessment.AvailableUntil,
            IsRandomized = assessment.IsRandomized,
            ShowCorrectAnswers = assessment.ShowCorrectAnswers,
            ShowScoreImmediately = assessment.ShowScoreImmediately,
            Questions = assessment.Questions?.Select(q => new QuestionModel
            {
                Id = q.Id,
                Text = q.Text,
                Type = q.Type.ToString(),
                Points = q.Points,
                AssessmentId = q.AssessmentId,
                OrderIndex = q.OrderIndex,
                Explanation = q.Explanation,
                IsRequired = q.IsRequired,
                Options = q.Options?.Select(o => new QuestionOptionModel
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect,
                    OrderIndex = o.OrderIndex
                }).ToList() ?? new List<QuestionOptionModel>()
            }).ToList() ?? new List<QuestionModel>()
        };
    }
}
