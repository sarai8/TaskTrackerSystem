using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Domain.Entities;

public class TaskAssignmentPost
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime DueDate { get; set; }

    public string AssignedByUserId { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    public string? AssignedToUserId { get; set; }

    public TaskAssignmentStatus Status { get; set; } = TaskAssignmentStatus.PendingClaim;

    public int? CreatedTaskId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}