using MediatR;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Features.Members.RemoveMember
{
    public record RemoveMemberCommand(Guid WorkspaceId, Guid UserId) : IAdminWorkspaceScoped, IRequest<Unit>;
}
