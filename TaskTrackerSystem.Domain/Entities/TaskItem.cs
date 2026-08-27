using DomainTaskStatus = TaskTrackerSystem.Domain.Enums.TaskStatus;
using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Domain.Entities;

public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DomainTaskStatus Status { get; set; } = DomainTaskStatus.Pending;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public string UserId { get; set; } = string.Empty;

    public bool ReminderSent { get; set; } = false;

    public bool IsArchived { get; set; } = false;
}