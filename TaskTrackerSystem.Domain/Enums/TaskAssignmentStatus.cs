namespace TaskTrackerSystem.Domain.Enums;

public enum TaskAssignmentStatus
{
    PendingClaim = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    DirectAssigned = 4
}