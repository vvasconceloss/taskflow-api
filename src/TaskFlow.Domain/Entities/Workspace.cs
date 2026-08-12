namespace TaskFlow.Domain.Entities
{
    public enum WorkspaceRole { Admin, Member }

    public class Workspace
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<WorkspaceMember> Members { get; set; } = [];
        public ICollection<Project> Projects { get; set; } = [];
    }
}
