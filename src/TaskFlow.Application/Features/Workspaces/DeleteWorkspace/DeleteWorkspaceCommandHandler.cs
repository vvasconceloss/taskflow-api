using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Features.Workspaces.DeleteWorkspace
{
    public class DeleteWorkspaceCommandHandler(IWorkspaceRepository workspaces)
        : IRequestHandler<DeleteWorkspaceCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var workspace = await workspaces.GetByIdAsync(request.WorkspaceId, cancellationToken)
                ?? throw new NotFoundException("Workspace not found.");

            await workspaces.RemoveAsync(workspace, cancellationToken);

            return Unit.Value;
        }
    }
}
