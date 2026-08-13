using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Members.RemoveMember
{
    public class RemoveMemberCommandHandler(IWorkspaceRepository workspaces)
        : IRequestHandler<RemoveMemberCommand, Unit>
    {
        public async Task<Unit> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
        {
            var member = await workspaces.GetMembershipAsync(request.WorkspaceId, request.UserId, cancellationToken)
                ?? throw new NotFoundException("Membership not found.");

            if (member.Role == WorkspaceRole.Admin)
            {
                var adminCount = await workspaces.CountAdminsAsync(request.WorkspaceId, cancellationToken);
                if (adminCount <= 1)
                {
                    throw new ConflictException("The last Admin of a workspace cannot be removed.");
                }
            }

            await workspaces.RemoveMemberAsync(member, cancellationToken);

            return Unit.Value;
        }
    }
}
