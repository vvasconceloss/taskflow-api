using MediatR;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Extensions;
using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Features.Members.AddMember;
using TaskFlow.Application.Features.Members.RemoveMember;
using TaskFlow.Application.Features.Members.UpdateMemberRole;
using TaskFlow.Application.Features.Workspaces.CreateWorkspace;
using TaskFlow.Application.Features.Workspaces.DeleteWorkspace;
using TaskFlow.Application.Features.Workspaces.GetWorkspace;
using TaskFlow.Application.Features.Workspaces.ListMyWorkspaces;
using TaskFlow.Application.Features.Workspaces.UpdateWorkspace;

namespace TaskFlow.Api.Endpoints
{
    public static class WorkspaceEndpoints
    {
        public static IEndpointRouteBuilder MapWorkspaceEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/workspaces").RequireAuthorization();

            group.MapGet("/", async (
                int page = 1,
                int pageSize = 20,
                string? sortBy = null,
                string? sortDir = null,
                ISender sender = default!) =>
            {
                var paged = await sender.Send(new ListMyWorkspacesQuery(page, pageSize, sortBy, sortDir == "desc"));
                return Results.Ok(new PagedResult<WorkspaceResponse>(
                    paged.Items.Select(w => new WorkspaceResponse(w.Id, w.Name, w.CreatedAt)).ToList(),
                    paged.Page,
                    paged.PageSize,
                    paged.TotalItems,
                    paged.TotalPages));
            }).WithSummary("Lists the authenticated user's workspaces")
              .Produces<PagedResult<WorkspaceResponse>>(StatusCodes.Status200OK)
              .WithApiErrorResponses();

            group.MapPost("/", async (CreateWorkspaceCommand command, ISender sender) =>
            {
                var workspace = await sender.Send(command);
                return Results.Created(string.Empty, new WorkspaceResponse(workspace.Id, workspace.Name, workspace.CreatedAt));
            }).WithSummary("Creates a workspace; the creator becomes Admin")
              .Produces<WorkspaceResponse>(StatusCodes.Status201Created)
              .WithApiErrorResponses();

            group.MapGet("/{workspaceId:guid}", async (Guid workspaceId, ISender sender) =>
            {
                var workspace = await sender.Send(new GetWorkspaceQuery(workspaceId));
                return Results.Ok(new WorkspaceResponse(workspace.Id, workspace.Name, workspace.CreatedAt));
            }).WithSummary("Returns a workspace by id")
              .Produces<WorkspaceResponse>(StatusCodes.Status200OK)
              .WithApiErrorResponses();

            group.MapPatch("/{workspaceId:guid}", async (Guid workspaceId, UpdateWorkspaceRequest request, ISender sender) =>
            {
                var workspace = await sender.Send(new UpdateWorkspaceCommand(workspaceId, request.Name));
                return Results.Ok(new WorkspaceResponse(workspace.Id, workspace.Name, workspace.CreatedAt));
            }).WithSummary("Renames a workspace (Admin only)")
              .Produces<WorkspaceResponse>(StatusCodes.Status200OK)
              .WithApiErrorResponses();

            group.MapDelete("/{workspaceId:guid}", async (Guid workspaceId, ISender sender) =>
            {
                await sender.Send(new DeleteWorkspaceCommand(workspaceId));
                return Results.NoContent();
            }).WithSummary("Deletes a workspace and everything inside it (Admin only)")
              .Produces(StatusCodes.Status204NoContent)
              .WithApiErrorResponses();

            group.MapPost("/{workspaceId:guid}/members", async (Guid workspaceId, AddMemberRequest request, ISender sender) =>
            {
                var member = await sender.Send(new AddMemberCommand(workspaceId, request.Email));
                return Results.Created(string.Empty, new WorkspaceMemberResponse(member.UserId, member.Role));
            }).WithSummary("Adds a member to the workspace by email (Admin only)")
              .Produces<WorkspaceMemberResponse>(StatusCodes.Status201Created)
              .WithApiErrorResponses();

            group.MapPatch("/{workspaceId:guid}/members/{userId:guid}", async (Guid workspaceId, Guid userId, UpdateMemberRoleRequest request, ISender sender) =>
            {
                var member = await sender.Send(new UpdateMemberRoleCommand(workspaceId, userId, request.Role));
                return Results.Ok(new WorkspaceMemberResponse(member.UserId, member.Role));
            }).WithSummary("Changes a member's role (Admin only; last Admin cannot be demoted)")
              .Produces<WorkspaceMemberResponse>(StatusCodes.Status200OK)
              .WithApiErrorResponses();

            group.MapDelete("/{workspaceId:guid}/members/{userId:guid}", async (Guid workspaceId, Guid userId, ISender sender) =>
            {
                await sender.Send(new RemoveMemberCommand(workspaceId, userId));
                return Results.NoContent();
            }).WithSummary("Removes a member (Admin only; last Admin cannot be removed)")
              .Produces(StatusCodes.Status204NoContent)
              .WithApiErrorResponses();

            return app;
        }
    }
}
