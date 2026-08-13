using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Workspaces.GetWorkspace
{
    public class GetWorkspaceQueryHandler(IWorkspaceRepository workspaces)
        : IRequestHandler<GetWorkspaceQuery, Workspace>
    {
        public async Task<Workspace> Handle(GetWorkspaceQuery request, CancellationToken cancellationToken) =>
            await workspaces.GetByIdAsync(request.WorkspaceId, cancellationToken)
            ?? throw new NotFoundException("Workspace not found.");
    }
}
