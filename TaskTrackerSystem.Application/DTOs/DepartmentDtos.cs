namespace TaskTrackerSystem.Application.DTOs;

public class DepartmentDto
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int EmployeeCount { get; set; }
}

public class DepartmentJoinRequestDto
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = "";

    public DateTime RequestedAt { get; set; }
    public TaskTrackerSystem.Domain.Enums.JoinRequestStatus Status { get; set; }
}