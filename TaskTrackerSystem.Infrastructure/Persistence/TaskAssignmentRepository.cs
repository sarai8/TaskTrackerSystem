using Microsoft.EntityFrameworkCore;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Domain.Entities;
using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Infrastructure.Persistence;

public class TaskAssignmentRepository : ITaskAssignmentRepository
{
    private readonly AppDbContext _db;

    public TaskAssignmentRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<TaskAssignmentPost?> GetByIdAsync(int id)
        => _db.TaskAssignmentPosts.FirstOrDefaultAsync(x => x.Id == id);

    public Task<List<TaskAssignmentPost>> GetPoolForDepartmentAsync(int departmentId)
        => _db.TaskAssignmentPosts
            .Where(x => x.DepartmentId == departmentId && x.Status == TaskAssignmentStatus.PendingClaim)
            .OrderBy(x => x.DueDate)
            .ToListAsync();

    public Task<List<TaskAssignmentPost>> GetPendingApprovalByManagerAsync(string managerId)
        => _db.TaskAssignmentPosts
            .Where(x => x.AssignedByUserId == managerId && x.Status == TaskAssignmentStatus.PendingApproval)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

    public Task<List<TaskAssignmentPost>> GetAllPendingApprovalAsync()
    => _db.TaskAssignmentPosts
        .Where(x => x.Status == TaskAssignmentStatus.PendingApproval)
        .OrderBy(x => x.CreatedAt)
        .ToListAsync();

    public async Task AddAsync(TaskAssignmentPost post)
        => await _db.TaskAssignmentPosts.AddAsync(post);

    public Task SaveChangesAsync()
        => _db.SaveChangesAsync();
}