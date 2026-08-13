using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.CreateTask
{
    public class CreateTaskCommandHandler(ITaskRepository tasks)
        : IRequestHandler<CreateTaskCommand, TaskItem>
    {
        public async Task<TaskItem> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                DueDate = request.DueDate
            };

            await tasks.CreateAsync(task, cancellationToken);

            return task;
        }
    }
}
