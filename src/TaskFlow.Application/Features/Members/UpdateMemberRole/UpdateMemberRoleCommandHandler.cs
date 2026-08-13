using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Members.UpdateMemberRole
{
    public class UpdateMemberRoleCommandHandler(IWorkspaceRepository workspaces)
        : IRequestHandler<UpdateMemberRoleCommand, WorkspaceMember>
    {
        public async Task<WorkspaceMember> Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
        {
            var member = await workspaces.GetMembershipAsync(request.WorkspaceId, request.UserId, cancellationToken)
                ?? throw new NotFoundException("Membership not found.");

            if (member.Role == WorkspaceRole.Admin && request.Role == WorkspaceRole.Member)
            {
                var adminCount = await workspaces.CountAdminsAsync(request.WorkspaceId, cancellationToken);
                if (adminCount <= 1)
                {
                    throw new ConflictException("The last Admin of a workspace cannot be demoted.");
                }
            }

            member.Role = request.Role;
            await workspaces.UpdateMemberAsync(member, cancellationToken);

            return member;
        }
    }
}
