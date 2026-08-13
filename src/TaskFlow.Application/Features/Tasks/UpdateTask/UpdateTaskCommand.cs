using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.UpdateTask
{
    public record UpdateTaskCommand(Guid TaskId, string Title, string? Description, TaskPriority Priority, DateTime? DueDate)
        : ITaskScoped, IRequest<TaskItem>;
}
