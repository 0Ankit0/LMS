
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class ModuleEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/modules");
        group.MapGet("/", async (IModuleRepository repo) => await repo.GetModulesAsync());
        group.MapPost("/paginated", async (PaginationRequest req, IModuleRepository repo) => await repo.GetModulesPaginatedAsync(req));
        group.MapGet("/{id}", async (int id, IModuleRepository repo) => await repo.GetModuleByIdAsync(id));
        group.MapGet("/course/{courseId}", async (int courseId, IModuleRepository repo) => await repo.GetModulesByCourseIdAsync(courseId));
        group.MapPost("/", async (CreateModuleRequest req, IModuleRepository repo) => await repo.CreateModuleAsync(req));
        group.MapPut("/{id}", async (int id, CreateModuleRequest req, IModuleRepository repo) => await repo.UpdateModuleAsync(id, req));
        group.MapDelete("/{id}", async (int id, IModuleRepository repo) => await repo.DeleteModuleAsync(id));
        group.MapPut("/{moduleId}/order", async (int moduleId, int newOrder, IModuleRepository repo) => await repo.UpdateModuleOrderAsync(moduleId, newOrder));
    }
}
