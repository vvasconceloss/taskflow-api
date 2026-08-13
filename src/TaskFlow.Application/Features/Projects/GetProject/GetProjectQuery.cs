using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.GetProject
{
    public record GetProjectQuery(Guid ProjectId) : IProjectScoped, IRequest<Project>;
}
