namespace TaskFlow.Application.Common.Interfaces
{
    public interface ITaskScoped
    {
        Guid TaskId { get; }
    }
}
