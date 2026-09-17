using TaskTrackerSystem.Domain.Entities;

namespace TaskTrackerSystem.Application.Interfaces;

public interface IDepartmentJoinRequestRepository
{
    Task<DepartmentJoinRequest?> GetPendingByUserIdAsync(string userId);

    Task<DepartmentJoinRequest?> GetLatestByUserIdAsync(string userId);

    Task<DepartmentJoinRequest?> GetByIdAsync(int id);

    Task<List<DepartmentJoinRequest>> GetAllPendingAsync();

    Task<List<DepartmentJoinRequest>> GetPendingByDepartmentIdsAsync(List<int> departmentIds);

    Task AddAsync(DepartmentJoinRequest request);

    Task SaveChangesAsync();
}