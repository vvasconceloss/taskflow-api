namespace TaskFlow.Application.Common.Interfaces
{
    public interface IWorkspaceScoped
    {
        Guid WorkspaceId { get; }
    }

    public interface IAdminWorkspaceScoped : IWorkspaceScoped
    {
    }
}
