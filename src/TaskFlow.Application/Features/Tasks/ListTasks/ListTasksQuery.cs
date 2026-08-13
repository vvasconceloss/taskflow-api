using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.ListTasks
{
    public record ListTasksQuery(Guid ProjectId) : IProjectScoped, IRequest<List<TaskItem>>;
}
