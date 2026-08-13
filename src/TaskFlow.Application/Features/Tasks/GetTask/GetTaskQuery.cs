using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.GetTask
{
    public record GetTaskQuery(Guid TaskId) : ITaskScoped, IRequest<TaskItem>;
}
