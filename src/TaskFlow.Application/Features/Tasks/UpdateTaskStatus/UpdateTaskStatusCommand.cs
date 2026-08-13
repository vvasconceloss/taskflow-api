using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.Application.Features.Tasks.UpdateTaskStatus
{
    public record UpdateTaskStatusCommand(Guid TaskId, TaskStatus Status) : ITaskScoped, IRequest<TaskItem>;
}
