using TaskTrackerSystem.Domain.Entities;

namespace TaskTrackerSystem.Application.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetByUserAsync(string userId);

    Task<List<TaskItem>> GetAllAsync();

    Task<TaskItem?> GetByIdAsync(int id, string userId);

    Task<TaskItem?> GetInProgressTaskAsync(string userId);

    Task<List<TaskItem>> GetTasksDueForReminderAsync(DateTime fromDate, DateTime toDate);

    Task MarkReminderSentAsync(int taskId);

    Task AddAsync(TaskItem task);

    void Remove(TaskItem task);

    Task SaveChangesAsync();
}