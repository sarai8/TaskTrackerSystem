using TaskTrackerSystem.Application.DTOs;

namespace TaskTrackerSystem.Application.Interfaces;

public interface ITaskAssignmentService
{
    Task AssignToDepartmentAsync(string managerId, int departmentId, CreateTaskAssignmentDto dto);

    Task AssignToUserAsync(string managerId, string userId, CreateTaskAssignmentDto dto);

    Task<List<TaskAssignmentPostDto>> GetPoolForDepartmentAsync(int departmentId);

    Task<TaskAssignmentPostDto?> ClaimAsync(int postId, string userId, int userDepartmentId, string departmentManagerId);

    Task<List<TaskAssignmentPostDto>> GetPendingApprovalByManagerAsync(string managerId);

    Task<List<TaskAssignmentPostDto>> GetAllPendingApprovalAsync();

    Task<TaskAssignmentPostDto?> ApproveAsync(int postId, string approverId, bool canApproveAny);

    Task<TaskAssignmentPostDto?> RejectAsync(int postId, string approverId, bool canApproveAny);
}