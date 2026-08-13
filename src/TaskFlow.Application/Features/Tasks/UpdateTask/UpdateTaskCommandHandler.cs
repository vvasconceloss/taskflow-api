using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.UpdateTask
{
    public class UpdateTaskCommandHandler(ITaskRepository tasks)
        : IRequestHandler<UpdateTaskCommand, TaskItem>
    {
        public async Task<TaskItem> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await tasks.GetByIdAsync(request.TaskId, cancellationToken)
                ?? throw new NotFoundException("Task not found.");

            task.Title = request.Title;
            task.Description = request.Description;
            task.Priority = request.Priority;
            task.DueDate = request.DueDate;

            await tasks.UpdateAsync(task, cancellationToken);

            return task;
        }
    }
}
