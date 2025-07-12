
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
        group.MapGet("/", async (IModuleRepository repo) => await repo.GetModulesAsync())
            .WithName("GetModules").WithSummary("Get all modules");
        group.MapPost("/paginated", async (PaginationRequest req, IModuleRepository repo) => await repo.GetModulesPaginatedAsync(req))
            .WithName("GetModulesPaginated").WithSummary("Get modules with pagination");
        group.MapGet("/{id}", async (int id, IModuleRepository repo) => await repo.GetModuleByIdAsync(id))
            .WithName("GetModuleById").WithSummary("Get module by ID");
        group.MapGet("/course/{courseId}", async (int courseId, IModuleRepository repo) => await repo.GetModulesByCourseIdAsync(courseId))
            .WithName("GetModulesByCourseId").WithSummary("Get modules by course ID");
        group.MapPost("/", async (CreateModuleRequest req, IModuleRepository repo) => await repo.CreateModuleAsync(req))
            .WithName("CreateModule").WithSummary("Create a new module");
        group.MapPut("/{id}", async (int id, CreateModuleRequest req, IModuleRepository repo) => await repo.UpdateModuleAsync(id, req))
            .WithName("UpdateModule").WithSummary("Update a module");
        group.MapDelete("/{id}", async (int id, IModuleRepository repo) => await repo.DeleteModuleAsync(id))
            .WithName("DeleteModule").WithSummary("Delete a module by ID");
        group.MapPut("/{moduleId}/order", async (int moduleId, int newOrder, IModuleRepository repo) => await repo.UpdateModuleOrderAsync(moduleId, newOrder))
            .WithName("UpdateModuleOrder").WithSummary("Update the order of a module");
    }
}
