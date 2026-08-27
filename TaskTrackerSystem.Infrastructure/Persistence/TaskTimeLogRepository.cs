using Microsoft.EntityFrameworkCore;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Domain.Entities;

namespace TaskTrackerSystem.Infrastructure.Persistence;

public class TaskTimeLogRepository : ITaskTimeLogRepository
{
    private readonly AppDbContext _db;

    public TaskTimeLogRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<TaskTimeLog?> GetOpenSessionAsync(int taskId)
        => _db.TaskTimeLogs
            .FirstOrDefaultAsync(x => x.TaskItemId == taskId && x.EndedAt == null);

    public Task<TaskTimeLog?> GetOpenSessionForUserAsync(string userId)
    => _db.TaskTimeLogs
        .Where(x => x.UserId == userId && x.EndedAt == null)
        .OrderByDescending(x => x.StartedAt)
        .FirstOrDefaultAsync();

    public Task<List<TaskTimeLog>> GetLogsForTaskAsync(int taskId)
    => _db.TaskTimeLogs
        .Where(x => x.TaskItemId == taskId)
        .ToListAsync();

    public async Task AddAsync(TaskTimeLog log)
        => await _db.TaskTimeLogs.AddAsync(log);

    public Task<List<TaskTimeLog>> GetLogsAsync(string userId, DateTime fromDate, DateTime toDate)
        => _db.TaskTimeLogs
            .Where(x => x.UserId == userId && x.StartedAt >= fromDate && x.StartedAt < toDate)
            .ToListAsync();
}