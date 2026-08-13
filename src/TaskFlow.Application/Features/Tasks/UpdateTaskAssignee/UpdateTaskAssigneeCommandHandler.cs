using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.UpdateTaskAssignee
{
    public class UpdateTaskAssigneeCommandHandler(ITaskRepository tasks, IWorkspaceRepository workspaces)
        : IRequestHandler<UpdateTaskAssigneeCommand, TaskItem>
    {
        public async Task<TaskItem> Handle(UpdateTaskAssigneeCommand request, CancellationToken cancellationToken)
        {
            var task = await tasks.GetByIdAsync(request.TaskId, cancellationToken)
                ?? throw new NotFoundException("Task not found.");

            if (request.AssigneeUserId is Guid assigneeId)
            {
                var workspaceId = await tasks.GetWorkspaceIdByTaskIdAsync(task.Id, cancellationToken)
                    ?? throw new NotFoundException("Task not found.");

                if (!await workspaces.IsMemberAsync(workspaceId, assigneeId, cancellationToken))
                {
                    throw new ForbiddenException("The assignee must be a member of the workspace.");
                }
            }

            task.AssigneeUserId = request.AssigneeUserId;
            await tasks.UpdateAsync(task, cancellationToken);

            return task;
        }
    }
}
