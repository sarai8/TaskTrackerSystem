using Microsoft.EntityFrameworkCore;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Domain.Entities;
using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Infrastructure.Persistence;

public class DepartmentJoinRequestRepository : IDepartmentJoinRequestRepository
{
    private readonly AppDbContext _db;

    public DepartmentJoinRequestRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<DepartmentJoinRequest?> GetPendingByUserIdAsync(string userId)
        => _db.DepartmentJoinRequests
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Status == JoinRequestStatus.Pending);

    public Task<DepartmentJoinRequest?> GetLatestByUserIdAsync(string userId)
        => _db.DepartmentJoinRequests
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.RequestedAt)
            .FirstOrDefaultAsync();

    public Task<DepartmentJoinRequest?> GetByIdAsync(int id)
        => _db.DepartmentJoinRequests.FirstOrDefaultAsync(x => x.Id == id);

    public Task<List<DepartmentJoinRequest>> GetAllPendingAsync()
        => _db.DepartmentJoinRequests
            .Where(x => x.Status == JoinRequestStatus.Pending)
            .OrderBy(x => x.RequestedAt)
            .ToListAsync();

    public Task<List<DepartmentJoinRequest>> GetPendingByDepartmentIdsAsync(List<int> departmentIds)
        => _db.DepartmentJoinRequests
            .Where(x => x.Status == JoinRequestStatus.Pending && departmentIds.Contains(x.DepartmentId))
            .OrderBy(x => x.RequestedAt)
            .ToListAsync();

    public async Task AddAsync(DepartmentJoinRequest request)
        => await _db.DepartmentJoinRequests.AddAsync(request);

    public Task SaveChangesAsync()
        => _db.SaveChangesAsync();
}