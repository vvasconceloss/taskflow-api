using MediatR;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Extensions;
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
            }).WithSummary("Lists the workspace's non-archived projects (paginated)")
              .Produces<PagedResult<ProjectResponse>>(StatusCodes.Status200OK)
              .WithApiErrorResponses();

            workspaceGroup.MapPost("/", async (Guid workspaceId, CreateProjectRequest request, ISender sender) =>
            {
                var project = await sender.Send(new CreateProjectCommand(workspaceId, request.Name, request.Description));
                return Results.Created(string.Empty, ToResponse(project));
            }).WithSummary("Creates a project in the workspace")
              .Produces<ProjectResponse>(StatusCodes.Status201Created)
              .WithApiErrorResponses();

            var projectGroup = app.MapGroup("/projects").RequireAuthorization();

            projectGroup.MapGet("/{projectId:guid}", async (Guid projectId, ISender sender) =>
            {
                var project = await sender.Send(new GetProjectQuery(projectId));
                return Results.Ok(ToResponse(project));
            }).WithSummary("Returns a project by id")
              .Produces<ProjectResponse>(StatusCodes.Status200OK)
              .WithApiErrorResponses();

            projectGroup.MapPatch("/{projectId:guid}", async (Guid projectId, UpdateProjectRequest request, ISender sender) =>
            {
                var project = await sender.Send(new UpdateProjectCommand(projectId, request.Name, request.Description));
                return Results.Ok(ToResponse(project));
            }).WithSummary("Updates a project's name and description")
              .Produces<ProjectResponse>(StatusCodes.Status200OK)
              .WithApiErrorResponses();

            projectGroup.MapPost("/{projectId:guid}/archive", async (Guid projectId, ISender sender) =>
            {
                var project = await sender.Send(new ArchiveProjectCommand(projectId));
                return Results.Ok(ToResponse(project));
            }).WithSummary("Archives a project; it disappears from the default listing")
              .Produces<ProjectResponse>(StatusCodes.Status200OK)
              .WithApiErrorResponses();

            projectGroup.MapDelete("/{projectId:guid}", async (Guid projectId, ISender sender) =>
            {
                await sender.Send(new DeleteProjectCommand(projectId));
                return Results.NoContent();
            }).WithSummary("Deletes a project; blocked while it still has tasks")
              .Produces(StatusCodes.Status204NoContent)
              .WithApiErrorResponses();

            return app;
        }

        private static ProjectResponse ToResponse(Domain.Entities.Project project) =>
            new(project.Id, project.Name, project.Description, project.WorkspaceId, project.IsArchived, project.CreatedAt);
    }
}
