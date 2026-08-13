using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Members.AddMember
{
    public record AddMemberCommand(Guid WorkspaceId, string Email) : IAdminWorkspaceScoped, IRequest<WorkspaceMember>;
}
