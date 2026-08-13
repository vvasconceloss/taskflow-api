using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.CreateTask
{
    public record CreateTaskCommand(Guid ProjectId, string Title, string? Description, TaskPriority Priority, DateTime? DueDate)
        : IProjectScoped, IRequest<TaskItem>;
}
