using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Members.AddMember
{
    public class AddMemberCommandHandler(IWorkspaceRepository workspaces, IUserRepository users)
        : IRequestHandler<AddMemberCommand, WorkspaceMember>
    {
        public async Task<WorkspaceMember> Handle(AddMemberCommand request, CancellationToken cancellationToken)
        {
            var user = await users.GetByEmailAsync(request.Email, cancellationToken)
                ?? throw new NotFoundException($"No user with the email '{request.Email}' was found.");

            if (await workspaces.GetMembershipAsync(request.WorkspaceId, user.Id, cancellationToken) is not null)
            {
                throw new ConflictException("The user is already a member of this workspace.");
            }

            var member = new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                WorkspaceId = request.WorkspaceId,
                UserId = user.Id,
                Role = WorkspaceRole.Member
            };

            await workspaces.AddMemberAsync(member, cancellationToken);

            return member;
        }
    }
}
