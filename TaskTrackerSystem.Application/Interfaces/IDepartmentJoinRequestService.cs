using TaskTrackerSystem.Application.DTOs;

namespace TaskTrackerSystem.Application.Interfaces;

public interface IDepartmentJoinRequestService
{
    Task RequestJoinAsync(string userId, int departmentId);

    Task<DepartmentJoinRequestDto?> GetLatestForUserAsync(string userId);

    Task<List<DepartmentJoinRequestDto>> GetAllPendingAsync();

    Task<List<DepartmentJoinRequestDto>> GetPendingForDepartmentsAsync(List<int> departmentIds);

    Task<DepartmentJoinRequestDto?> ApproveAsync(int requestId);

    Task<DepartmentJoinRequestDto?> RejectAsync(int requestId);
}