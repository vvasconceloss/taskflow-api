using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Features.Projects.DeleteProject
{
    public class DeleteProjectCommandHandler(IProjectRepository projects)
        : IRequestHandler<DeleteProjectCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await projects.GetByIdAsync(request.ProjectId, cancellationToken)
                ?? throw new NotFoundException("Project not found.");

            if (await projects.HasTasksAsync(request.ProjectId, cancellationToken))
            {
                throw new ConflictException("Cannot delete a project that still has tasks. Archive or move the tasks first.");
            }

            await projects.RemoveAsync(project, cancellationToken);

            return Unit.Value;
        }
    }
}
