namespace TaskTrackerSystem.Domain.Entities;

public class TaskTimeLog
{
    public int Id { get; set; }

    public int TaskItemId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }
}