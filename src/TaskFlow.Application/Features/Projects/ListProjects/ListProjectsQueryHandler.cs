using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.ListProjects
{
    public class ListProjectsQueryHandler(IProjectRepository projects)
        : IRequestHandler<ListProjectsQuery, List<Project>>
    {
        public Task<List<Project>> Handle(ListProjectsQuery request, CancellationToken cancellationToken) =>
            projects.GetProjectsByWorkspaceAsync(request.WorkspaceId, cancellationToken);
    }
}
