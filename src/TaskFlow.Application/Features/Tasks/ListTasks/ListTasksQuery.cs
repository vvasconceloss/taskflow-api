using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;
using TaskPriority = TaskFlow.Domain.Entities.TaskPriority;

namespace TaskFlow.Application.Features.Tasks.ListTasks
{
    public record ListTasksQuery(
        Guid ProjectId,
        TaskStatus? Status,
        TaskPriority? Priority,
        Guid? AssigneeId,
        int Page,
        int PageSize,
        string? SortBy,
        bool SortDescending)
        : IProjectScoped, IRequest<PagedResult<TaskItem>>;
}
