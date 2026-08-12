namespace TaskFlow.Domain.Entities
{
    public class WorkspaceMember
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid WorkspaceId { get; set; }
        public WorkspaceRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
