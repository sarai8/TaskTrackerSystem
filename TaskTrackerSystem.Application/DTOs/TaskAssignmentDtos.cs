using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Application.DTOs;

public class TaskAssignmentPostDto
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string? Description { get; set; }

    public TaskPriority Priority { get; set; }

    public DateTime DueDate { get; set; }

    public string AssignedByUserId { get; set; } = "";

    public int? DepartmentId { get; set; }

    public string? AssignedToUserId { get; set; }

    public TaskAssignmentStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateTaskAssignmentDto
{
    public string Title { get; set; } = "";

    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime DueDate { get; set; }
}