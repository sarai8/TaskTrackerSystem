using TaskTrackerSystem.Domain.Entities;
using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Application.Interfaces;

public interface ITaskAssignmentRepository
{
    Task<TaskAssignmentPost?> GetByIdAsync(int id);

    Task<List<TaskAssignmentPost>> GetPoolForDepartmentAsync(int departmentId);

    Task<List<TaskAssignmentPost>> GetPendingApprovalByManagerAsync(string managerId);
    Task<List<TaskAssignmentPost>> GetAllPendingApprovalAsync();

    Task AddAsync(TaskAssignmentPost post);

    Task SaveChangesAsync();
}