namespace TaskFlow.Api.Contracts
{
    public record ProjectResponse(Guid Id, string Name, string? Description, Guid WorkspaceId, bool IsArchived, DateTime CreatedAt);

    public record CreateProjectRequest(string Name, string? Description);

    public record UpdateProjectRequest(string Name, string? Description);
}
