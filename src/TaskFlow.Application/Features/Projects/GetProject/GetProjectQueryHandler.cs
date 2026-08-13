using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.GetProject
{
    public class GetProjectQueryHandler(IProjectRepository projects)
        : IRequestHandler<GetProjectQuery, Project>
    {
        public async Task<Project> Handle(GetProjectQuery request, CancellationToken cancellationToken) =>
            await projects.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project not found.");
    }
}
