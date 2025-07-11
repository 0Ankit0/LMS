
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
        group.MapGet("/", async (IAssessmentRepository repo) => await repo.GetAssessmentsAsync());
        group.MapPost("/paginated", async (PaginationRequest req, IAssessmentRepository repo) => await repo.GetAssessmentsPaginatedAsync(req));
        group.MapGet("/course/{courseId}", async (int courseId, IAssessmentRepository repo) => await repo.GetAssessmentsByCourseAsync(courseId));
        group.MapGet("/{id}", async (int id, IAssessmentRepository repo) => await repo.GetAssessmentAsync(id));
        group.MapPost("/", async (CreateAssessmentRequest req, IAssessmentRepository repo) => await repo.CreateAssessmentAsync(req));
        group.MapPut("/{id}", async (int id, CreateAssessmentRequest req, IAssessmentRepository repo) => await repo.UpdateAssessmentAsync(id, req));
        group.MapDelete("/{id}", async (int id, IAssessmentRepository repo) => await repo.DeleteAssessmentAsync(id));
        group.MapPost("/question", async (CreateQuestionRequest req, IAssessmentRepository repo) => await repo.CreateQuestionAsync(req));
        group.MapGet("/questions", async (IAssessmentRepository repo) => await repo.GetAllQuestionsAsync());
        group.MapPost("/questions/paginated", async (PaginationRequest req, IAssessmentRepository repo) => await repo.GetAllQuestionsPaginatedAsync(req));
    }
}
