using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Workspaces.CreateWorkspace
{
    public class CreateWorkspaceCommandHandler(IWorkspaceRepository workspaces, ICurrentUserService currentUser)
        : IRequestHandler<CreateWorkspaceCommand, Workspace>
    {
        public async Task<Workspace> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                CreatedByUserId = currentUser.UserId
            };

            var owner = new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspace.Id,
                UserId = currentUser.UserId,
                Role = WorkspaceRole.Admin
            };

            await workspaces.CreateWithOwnerAsync(workspace, owner, cancellationToken);

            return workspace;
        }
    }
}
