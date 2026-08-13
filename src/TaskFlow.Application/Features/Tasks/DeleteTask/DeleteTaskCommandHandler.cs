using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Features.Tasks.DeleteTask
{
    public class DeleteTaskCommandHandler(ITaskRepository tasks)
        : IRequestHandler<DeleteTaskCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await tasks.GetByIdAsync(request.TaskId, cancellationToken)
                ?? throw new NotFoundException("Task not found.");

            await tasks.RemoveAsync(task, cancellationToken);

            return Unit.Value;
        }
    }
}
