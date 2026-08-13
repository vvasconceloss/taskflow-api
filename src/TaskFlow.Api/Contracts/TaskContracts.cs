using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;
using TaskPriority = TaskFlow.Domain.Entities.TaskPriority;

namespace TaskFlow.Api.Contracts
{
    public record TaskResponse(
        Guid Id,
        string Title,
        string? Description,
        TaskStatus Status,
        TaskPriority Priority,
        Guid ProjectId,
        Guid? AssigneeUserId,
        DateTime? DueDate,
        DateTime CreatedAt,
        DateTime? CompletedAt);

    public record CreateTaskRequest(string Title, string? Description, TaskPriority? Priority, DateTime? DueDate);

    public record UpdateTaskRequest(string Title, string? Description, TaskPriority Priority, DateTime? DueDate);

    public record UpdateTaskStatusRequest(TaskStatus Status);

    public record UpdateTaskAssigneeRequest(Guid? AssigneeUserId);
}
