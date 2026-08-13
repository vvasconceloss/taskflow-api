using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.GetTask
{
    public class GetTaskQueryHandler(ITaskRepository tasks)
        : IRequestHandler<GetTaskQuery, TaskItem>
    {
        public async Task<TaskItem> Handle(GetTaskQuery request, CancellationToken cancellationToken) =>
            await tasks.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new NotFoundException("Task not found.");
    }
}
