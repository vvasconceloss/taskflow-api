using TaskFlow.Domain.Entities;

namespace TaskFlow.Api.Contracts
{
    public record WorkspaceResponse(Guid Id, string Name, DateTime CreatedAt);

    public record UpdateWorkspaceRequest(string Name);

    public record WorkspaceMemberResponse(Guid UserId, WorkspaceRole Role);

    public record AddMemberRequest(string Email);

    public record UpdateMemberRoleRequest(WorkspaceRole Role);
}
