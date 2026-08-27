using DomainTaskStatus = TaskTrackerSystem.Domain.Enums.TaskStatus;
using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Application.DTOs;

public class TaskDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DomainTaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsArchived { get; set; }
}

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(1);
}

public class UpdateTaskDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DomainTaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public DateTime DueDate { get; set; }
}

public class TaskReminderDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public string UserId { get; set; } = string.Empty;
}