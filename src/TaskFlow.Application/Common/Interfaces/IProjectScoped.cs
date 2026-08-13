namespace TaskFlow.Application.Common.Interfaces
{
    public interface IProjectScoped
    {
        Guid ProjectId { get; }
    }
}
