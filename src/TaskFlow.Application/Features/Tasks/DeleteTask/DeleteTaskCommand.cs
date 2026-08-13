using MediatR;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Features.Tasks.DeleteTask
{
    public record DeleteTaskCommand(Guid TaskId) : ITaskScoped, IRequest<Unit>;
}
