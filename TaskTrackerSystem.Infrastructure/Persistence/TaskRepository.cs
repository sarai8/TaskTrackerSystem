using Microsoft.EntityFrameworkCore;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Domain.Entities;

namespace TaskTrackerSystem.Infrastructure.Persistence;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;

    public TaskRepository(AppDbContext db)
        => _db = db;

    public Task<List<TaskItem>> GetByUserAsync(string userId)
        => _db.Tasks
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Status)
            .ThenBy(x => x.DueDate)
            .ToListAsync();

    public Task<TaskItem?> GetByIdAsync(int id, string userId)
        => _db.Tasks
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);

    public Task<TaskItem?> GetInProgressTaskAsync(string userId)
    => _db.Tasks
        .FirstOrDefaultAsync(x =>
            x.UserId == userId &&
            x.Status == TaskTrackerSystem.Domain.Enums.TaskStatus.InProgress);

    public Task<List<TaskItem>> GetTasksDueForReminderAsync(DateTime fromDate, DateTime toDate)
    => _db.Tasks
        .Where(x =>
            !x.ReminderSent &&
            x.Status != TaskTrackerSystem.Domain.Enums.TaskStatus.Completed &&
            x.DueDate >= fromDate &&
            x.DueDate <= toDate)
        .ToListAsync();

    public async Task MarkReminderSentAsync(int taskId)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);

        if (task != null)
        {
            task.ReminderSent = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task AddAsync(TaskItem task)
        => await _db.Tasks.AddAsync(task);

    public void Remove(TaskItem task)
        => _db.Tasks.Remove(task);

    public Task SaveChangesAsync()
        => _db.SaveChangesAsync();
}