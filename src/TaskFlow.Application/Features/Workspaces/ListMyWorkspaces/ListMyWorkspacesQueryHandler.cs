using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Workspaces.ListMyWorkspaces
{
    public class ListMyWorkspacesQueryHandler(IWorkspaceRepository workspaces, ICurrentUserService currentUser)
        : IRequestHandler<ListMyWorkspacesQuery, List<Workspace>>
    {
        public Task<List<Workspace>> Handle(ListMyWorkspacesQuery request, CancellationToken cancellationToken) =>
            workspaces.GetWorkspacesForUserAsync(currentUser.UserId, cancellationToken);
    }
}
