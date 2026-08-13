using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.ArchiveProject
{
    public class ArchiveProjectCommandHandler(IProjectRepository projects)
        : IRequestHandler<ArchiveProjectCommand, Project>
    {
        public async Task<Project> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await projects.GetByIdAsync(request.ProjectId, cancellationToken)
                ?? throw new NotFoundException("Project not found.");

            project.IsArchived = true;
            await projects.UpdateAsync(project, cancellationToken);

            return project;
        }
    }
}
