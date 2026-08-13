using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.UpdateProject
{
    public class UpdateProjectCommandHandler(IProjectRepository projects)
        : IRequestHandler<UpdateProjectCommand, Project>
    {
        public async Task<Project> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await projects.GetByIdAsync(request.ProjectId, cancellationToken)
                ?? throw new NotFoundException("Project not found.");

            project.Name = request.Name;
            project.Description = request.Description;
            await projects.UpdateAsync(project, cancellationToken);

            return project;
        }
    }
}
