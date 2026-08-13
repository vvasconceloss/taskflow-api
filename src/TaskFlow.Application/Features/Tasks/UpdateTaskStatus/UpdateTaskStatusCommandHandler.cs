using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.Application.Features.Tasks.UpdateTaskStatus
{
    public class UpdateTaskStatusCommandHandler(ITaskRepository tasks)
        : IRequestHandler<UpdateTaskStatusCommand, TaskItem>
    {
        public async Task<TaskItem> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
        {
            var task = await tasks.GetByIdAsync(request.TaskId, cancellationToken)
                ?? throw new NotFoundException("Task not found.");

            if (request.Status == TaskStatus.Done && task.Status != TaskStatus.Done)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            else if (request.Status != TaskStatus.Done)
            {
                task.CompletedAt = null;
            }

            task.Status = request.Status;
            await tasks.UpdateAsync(task, cancellationToken);

            return task;
        }
    }
}
