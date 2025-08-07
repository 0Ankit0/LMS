using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LMS.Repositories;

public interface IAssessmentRepository
{
    Task<IEnumerable<AssessmentModel>> GetAssessmentsAsync();
    Task<PaginatedResult<AssessmentModel>> GetAssessmentsPaginatedAsync(PaginationRequest request);
    Task<IEnumerable<AssessmentModel>> GetAssessmentsByCourseAsync(int courseId);
    Task<IEnumerable<AssessmentModel>> GetAssessmentsByCourseIdAsync(int courseId);
    Task<AssessmentModel?> GetAssessmentByIdAsync(int id); // Keep this one as it's more standard naming
    Task<AssessmentModel> CreateAssessmentAsync(CreateAssessmentRequest request);
    Task<AssessmentModel> UpdateAssessmentAsync(int id, CreateAssessmentRequest request);
    Task<bool> DeleteAssessmentAsync(int id);
    Task<QuestionModel> CreateQuestionAsync(CreateQuestionRequest request);
    Task<QuestionModel?> GetQuestionByIdAsync(int id);
    Task<QuestionModel> UpdateQuestionAsync(int id, CreateQuestionRequest request);
    Task<bool> DeleteQuestionAsync(int id);
    Task<IEnumerable<QuestionModel>> GetAllQuestionsAsync();
    Task<PaginatedResult<QuestionModel>> GetAllQuestionsPaginatedAsync(PaginationRequest request);
}
public class AssessmentRepository : IAssessmentRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AssessmentRepository> _logger;

    public AssessmentRepository(ApplicationDbContext context, ILogger<AssessmentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<AssessmentModel>> GetAssessmentsAsync()
    {
        try
        {
            var assessments = await _context.Assessments
                .Include(a => a.Questions)
                    .ThenInclude(q => q.Options)
                .Include(a => a.Course)
                .Include(a => a.Module)
                .Include(a => a.Lesson)
                .ToListAsync();

            return assessments.Select(MapToAssessmentModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assessments");
            throw;
        }
    }

    public async Task<IEnumerable<AssessmentModel>> GetAssessmentsByCourseAsync(int courseId)
    {
        try
        {
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assessments by course: {CourseId}", courseId);
            throw;
        }
    }

    public async Task<IEnumerable<AssessmentModel>> GetAssessmentsByCourseIdAsync(int courseId)
    {
        // For compatibility, just call GetAssessmentsByCourseAsync
        return await GetAssessmentsByCourseAsync(courseId);
    }

    public async Task<AssessmentModel?> GetAssessmentByIdAsync(int id)
    {
        try
        {
            var assessment = await _context.Assessments
                .Include(a => a.Questions)
                    .ThenInclude(q => q.Options)
                .Include(a => a.Course)
                .Include(a => a.Module)
                .Include(a => a.Lesson)
                .FirstOrDefaultAsync(a => a.Id == id);

            return assessment != null ? MapToAssessmentModel(assessment) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assessment by id: {AssessmentId}", id);
            throw;
        }
    }

    public async Task<PaginatedResult<AssessmentModel>> GetAssessmentsPaginatedAsync(PaginationRequest request)
    {
        try
        {
            request.Validate();
            var query = _context.Assessments
                .Include(a => a.Questions)
                    .ThenInclude(q => q.Options)
                .Include(a => a.Course)
                .Include(a => a.Module)
                .Include(a => a.Lesson)
                .OrderBy(a => a.Title);

            var totalCount = await query.CountAsync();
            var skip = (request.PageNumber - 1) * request.PageSize;
            var assessments = await query.Skip(skip).Take(request.PageSize).ToListAsync();

            return new PaginatedResult<AssessmentModel>
            {
                Items = assessments.Select(MapToAssessmentModel).ToList(),
                TotalCount = totalCount,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated assessments");
            throw;
        }
    }

    public async Task<AssessmentModel> CreateAssessmentAsync(CreateAssessmentRequest request)
    {
        try
        {
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

            return await GetAssessmentByIdAsync(assessment.Id) ?? throw new InvalidOperationException("Assessment not found after creation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assessment");
            throw;
        }
    }

    public async Task<AssessmentModel> UpdateAssessmentAsync(int id, CreateAssessmentRequest request)
    {
        try
        {
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

            return await GetAssessmentByIdAsync(id) ?? throw new InvalidOperationException("Assessment not found after update");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating assessment: {AssessmentId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAssessmentAsync(int id)
    {
        try
        {
            var assessment = await _context.Assessments.FindAsync(id);
            if (assessment == null)
                return false;

            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assessment: {AssessmentId}", id);
            throw;
        }
    }

    public async Task<QuestionModel> CreateQuestionAsync(CreateQuestionRequest request)
    {
        try
        {
            var question = new Question
            {
                Text = request.Text,
                AssessmentId = request.AssessmentId,
                Type = (QuestionType)request.Type,
                Points = request.Points,
                OrderIndex = request.OrderIndex,
                Explanation = request.Explanation,
                IsRequired = request.IsRequired,
                CreatedAt = DateTime.UtcNow
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            // Add options if provided
            if (request.Options.Any())
            {
                var options = request.Options.Select(o => new QuestionOption
                {
                    Text = o.Text,
                    QuestionId = question.Id,
                    IsCorrect = o.IsCorrect,
                    OrderIndex = o.OrderIndex
                }).ToList();

                _context.QuestionOptions.AddRange(options);
                await _context.SaveChangesAsync();
            }

            // Return the created question as a model
            var createdQuestion = await _context.Questions
                .Include(q => q.Options)
                .FirstAsync(q => q.Id == question.Id);

            return MapToQuestionModel(createdQuestion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating question");
            throw;
        }
    }

    public async Task<QuestionModel?> GetQuestionByIdAsync(int id)
    {
        try
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .Include(q => q.Assessment)
                .FirstOrDefaultAsync(q => q.Id == id);

            return question != null ? MapToQuestionModel(question) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting question by id: {QuestionId}", id);
            throw;
        }
    }

    public async Task<QuestionModel> UpdateQuestionAsync(int id, CreateQuestionRequest request)
    {
        try
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);
            
            if (question == null)
                throw new ArgumentException($"Question with ID {id} not found");

            question.Text = request.Text;
            question.AssessmentId = request.AssessmentId;
            question.Type = (QuestionType)request.Type;
            question.Points = request.Points;
            question.OrderIndex = request.OrderIndex;
            question.Explanation = request.Explanation;
            question.IsRequired = request.IsRequired;

            // Update options if provided
            if (request.Options.Any())
            {
                // Remove existing options
                _context.QuestionOptions.RemoveRange(question.Options);

                // Add new options
                var options = request.Options.Select(o => new QuestionOption
                {
                    Text = o.Text,
                    QuestionId = question.Id,
                    IsCorrect = o.IsCorrect,
                    OrderIndex = o.OrderIndex
                }).ToList();

                _context.QuestionOptions.AddRange(options);
            }

            await _context.SaveChangesAsync();

            return await GetQuestionByIdAsync(id) ?? throw new InvalidOperationException("Question not found after update");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question: {QuestionId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteQuestionAsync(int id)
    {
        try
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
                return false;

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting question: {QuestionId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<QuestionModel>> GetAllQuestionsAsync()
    {
        try
        {
            var questions = await _context.Questions
                .Include(q => q.Options)
                .ToListAsync();
            return questions.Select(MapToQuestionModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all questions");
            throw;
        }
    }

    public async Task<PaginatedResult<QuestionModel>> GetAllQuestionsPaginatedAsync(PaginationRequest request)
    {
        try
        {
            request.Validate();
            var query = _context.Questions
                .Include(q => q.Options)
                .Include(q => q.Assessment)
                .OrderBy(q => q.Assessment.Title)
                .ThenBy(q => q.OrderIndex);

            var totalCount = await query.CountAsync();
            var skip = (request.PageNumber - 1) * request.PageSize;
            var questions = await query.Skip(skip).Take(request.PageSize).ToListAsync();

            return new PaginatedResult<QuestionModel>
            {
                Items = questions.Select(MapToQuestionModel).ToList(),
                TotalCount = totalCount,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated questions");
            throw;
        }
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
                Type = q.Type,
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

    private static QuestionModel MapToQuestionModel(Question question)
    {
        return new QuestionModel
        {
            Id = question.Id,
            Text = question.Text,
            AssessmentId = question.AssessmentId,
            Type = question.Type,
            Points = question.Points,
            OrderIndex = question.OrderIndex,
            Explanation = question.Explanation,
            IsRequired = question.IsRequired,
            CreatedAt = question.CreatedAt,
            Options = question.Options?.Select(o => new QuestionOptionModel
            {
                Id = o.Id,
                Text = o.Text,
                IsCorrect = o.IsCorrect,
                OrderIndex = o.OrderIndex
            }).ToList() ?? new List<QuestionOptionModel>()
        };
    }
}
