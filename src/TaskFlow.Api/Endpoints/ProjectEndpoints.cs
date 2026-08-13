using MediatR;
using TaskFlow.Api.Contracts;
using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Features.Projects.ArchiveProject;
using TaskFlow.Application.Features.Projects.CreateProject;
using TaskFlow.Application.Features.Projects.DeleteProject;
using TaskFlow.Application.Features.Projects.GetProject;
using TaskFlow.Application.Features.Projects.ListProjects;
using TaskFlow.Application.Features.Projects.UpdateProject;

namespace TaskFlow.Api.Endpoints
{
    public static class ProjectEndpoints
    {
        public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
        {
            var workspaceGroup = app.MapGroup("/workspaces/{workspaceId:guid}/projects").RequireAuthorization();

            workspaceGroup.MapGet("/", async (
                Guid workspaceId,
                int page = 1,
                int pageSize = 20,
                string? sortBy = null,
                string? sortDir = null,
                ISender sender = default!) =>
            {
                var paged = await sender.Send(new ListProjectsQuery(workspaceId, page, pageSize, sortBy, sortDir == "desc"));
                return Results.Ok(new PagedResult<ProjectResponse>(
                    paged.Items.Select(ToResponse).ToList(),
                    paged.Page,
                    paged.PageSize,
                    paged.TotalItems,
                    paged.TotalPages));
            });

            workspaceGroup.MapPost("/", async (Guid workspaceId, CreateProjectRequest request, ISender sender) =>
            {
                var project = await sender.Send(new CreateProjectCommand(workspaceId, request.Name, request.Description));
                return Results.Created(string.Empty, ToResponse(project));
            });

            var projectGroup = app.MapGroup("/projects").RequireAuthorization();

            projectGroup.MapGet("/{projectId:guid}", async (Guid projectId, ISender sender) =>
            {
                var project = await sender.Send(new GetProjectQuery(projectId));
                return Results.Ok(ToResponse(project));
            });

            projectGroup.MapPatch("/{projectId:guid}", async (Guid projectId, UpdateProjectRequest request, ISender sender) =>
            {
                var project = await sender.Send(new UpdateProjectCommand(projectId, request.Name, request.Description));
                return Results.Ok(ToResponse(project));
            });

            projectGroup.MapPost("/{projectId:guid}/archive", async (Guid projectId, ISender sender) =>
            {
                var project = await sender.Send(new ArchiveProjectCommand(projectId));
                return Results.Ok(ToResponse(project));
            });

            projectGroup.MapDelete("/{projectId:guid}", async (Guid projectId, ISender sender) =>
            {
                await sender.Send(new DeleteProjectCommand(projectId));
                return Results.NoContent();
            });

            return app;
        }

        private static ProjectResponse ToResponse(Domain.Entities.Project project) =>
            new(project.Id, project.Name, project.Description, project.WorkspaceId, project.IsArchived, project.CreatedAt);
    }
}
