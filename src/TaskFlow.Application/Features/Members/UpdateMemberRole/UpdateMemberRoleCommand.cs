using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Members.UpdateMemberRole
{
    public record UpdateMemberRoleCommand(Guid WorkspaceId, Guid UserId, WorkspaceRole Role)
        : IAdminWorkspaceScoped, IRequest<WorkspaceMember>;
}
