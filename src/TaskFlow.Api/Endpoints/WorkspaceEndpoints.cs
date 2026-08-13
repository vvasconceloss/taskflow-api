using MediatR;
using TaskFlow.Api.Contracts;
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

            group.MapGet("/", async (ISender sender) =>
            {
                var workspaces = await sender.Send(new ListMyWorkspacesQuery());
                return Results.Ok(workspaces.Select(w => new WorkspaceResponse(w.Id, w.Name, w.CreatedAt)));
            });

            group.MapPost("/", async (CreateWorkspaceCommand command, ISender sender) =>
            {
                var workspace = await sender.Send(command);
                return Results.Created(string.Empty, new WorkspaceResponse(workspace.Id, workspace.Name, workspace.CreatedAt));
            });

            group.MapGet("/{workspaceId:guid}", async (Guid workspaceId, ISender sender) =>
            {
                var workspace = await sender.Send(new GetWorkspaceQuery(workspaceId));
                return Results.Ok(new WorkspaceResponse(workspace.Id, workspace.Name, workspace.CreatedAt));
            });

            group.MapPatch("/{workspaceId:guid}", async (Guid workspaceId, UpdateWorkspaceRequest request, ISender sender) =>
            {
                var workspace = await sender.Send(new UpdateWorkspaceCommand(workspaceId, request.Name));
                return Results.Ok(new WorkspaceResponse(workspace.Id, workspace.Name, workspace.CreatedAt));
            });

            group.MapDelete("/{workspaceId:guid}", async (Guid workspaceId, ISender sender) =>
            {
                await sender.Send(new DeleteWorkspaceCommand(workspaceId));
                return Results.NoContent();
            });

            group.MapPost("/{workspaceId:guid}/members", async (Guid workspaceId, AddMemberRequest request, ISender sender) =>
            {
                var member = await sender.Send(new AddMemberCommand(workspaceId, request.Email));
                return Results.Created(string.Empty, new WorkspaceMemberResponse(member.UserId, member.Role));
            });

            group.MapPatch("/{workspaceId:guid}/members/{userId:guid}", async (Guid workspaceId, Guid userId, UpdateMemberRoleRequest request, ISender sender) =>
            {
                var member = await sender.Send(new UpdateMemberRoleCommand(workspaceId, userId, request.Role));
                return Results.Ok(new WorkspaceMemberResponse(member.UserId, member.Role));
            });

            group.MapDelete("/{workspaceId:guid}/members/{userId:guid}", async (Guid workspaceId, Guid userId, ISender sender) =>
            {
                await sender.Send(new RemoveMemberCommand(workspaceId, userId));
                return Results.NoContent();
            });

            return app;
        }
    }
}
