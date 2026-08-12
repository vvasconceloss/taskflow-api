namespace TaskFlow.Domain.Entities
{
    public enum TaskPriority { Low, Medium, High }
    public enum TaskStatus { Todo, InProgress, Done }

    public class TaskItem
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? AssigneeUserId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
