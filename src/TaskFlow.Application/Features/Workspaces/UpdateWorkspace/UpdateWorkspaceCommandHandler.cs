using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Workspaces.UpdateWorkspace
{
    public class UpdateWorkspaceCommandHandler(IWorkspaceRepository workspaces)
        : IRequestHandler<UpdateWorkspaceCommand, Workspace>
    {
        public async Task<Workspace> Handle(UpdateWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var workspace = await workspaces.GetByIdAsync(request.WorkspaceId, cancellationToken)
                ?? throw new NotFoundException("Workspace not found.");

            workspace.Name = request.Name;
            await workspaces.UpdateAsync(workspace, cancellationToken);

            return workspace;
        }
    }
}
