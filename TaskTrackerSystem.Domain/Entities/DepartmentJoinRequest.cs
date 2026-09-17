using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Domain.Entities;

public class DepartmentJoinRequest
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public JoinRequestStatus Status { get; set; } = JoinRequestStatus.Pending;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }
}