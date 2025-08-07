using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class AssessmentEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assessments");
        group.MapGet("/", async (IAssessmentRepository repo) => await repo.GetAssessmentsAsync())
            .WithName("GetAssessments").WithSummary("Get all assessments");
        group.MapPost("/paginated", async (PaginationRequest req, IAssessmentRepository repo) => await repo.GetAssessmentsPaginatedAsync(req))
            .WithName("GetAssessmentsPaginated").WithSummary("Get assessments with pagination");
        group.MapGet("/course/{courseId}", async (int courseId, IAssessmentRepository repo) => await repo.GetAssessmentsByCourseAsync(courseId))
            .WithName("GetAssessmentsByCourse").WithSummary("Get assessments by course ID");
        group.MapGet("/{id}", async (int id, IAssessmentRepository repo) => await repo.GetAssessmentByIdAsync(id))
            .WithName("GetAssessmentById").WithSummary("Get assessment by ID");
        group.MapPost("/", async (CreateAssessmentRequest req, IAssessmentRepository repo) => await repo.CreateAssessmentAsync(req))
            .WithName("CreateAssessment").WithSummary("Create a new assessment");
        group.MapPut("/{id}", async (int id, CreateAssessmentRequest req, IAssessmentRepository repo) => await repo.UpdateAssessmentAsync(id, req))
            .WithName("UpdateAssessment").WithSummary("Update an assessment");
        group.MapDelete("/{id}", async (int id, IAssessmentRepository repo) => await repo.DeleteAssessmentAsync(id))
            .WithName("DeleteAssessment").WithSummary("Delete an assessment by ID");
        group.MapPost("/question", async (CreateQuestionRequest req, IAssessmentRepository repo) => await repo.CreateQuestionAsync(req))
            .WithName("CreateQuestion").WithSummary("Create a new question for an assessment");
        group.MapGet("/questions", async (IAssessmentRepository repo) => await repo.GetAllQuestionsAsync())
            .WithName("GetAllQuestions").WithSummary("Get all questions");
        group.MapPost("/questions/paginated", async (PaginationRequest req, IAssessmentRepository repo) => await repo.GetAllQuestionsPaginatedAsync(req))
            .WithName("GetAllQuestionsPaginated").WithSummary("Get all questions with pagination");
    }
}
