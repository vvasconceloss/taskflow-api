using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.CreateProject
{
    public class CreateProjectCommandHandler(IProjectRepository projects)
        : IRequestHandler<CreateProjectCommand, Project>
    {
        public async Task<Project> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                WorkspaceId = request.WorkspaceId
            };

            await projects.CreateAsync(project, cancellationToken);

            return project;
        }
    }
}
