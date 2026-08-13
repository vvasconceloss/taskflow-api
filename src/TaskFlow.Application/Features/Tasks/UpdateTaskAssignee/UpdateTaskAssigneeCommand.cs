using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.UpdateTaskAssignee
{
    public record UpdateTaskAssigneeCommand(Guid TaskId, Guid? AssigneeUserId) : ITaskScoped, IRequest<TaskItem>;
}
