using TaskTrackerSystem.Domain.Entities;

namespace TaskTrackerSystem.Application.Interfaces;

public interface ITaskTimeLogRepository
{
    Task<TaskTimeLog?> GetOpenSessionAsync(int taskId);

    Task<TaskTimeLog?> GetOpenSessionForUserAsync(string userId);

    Task<List<TaskTimeLog>> GetLogsForTaskAsync(int taskId);

    Task AddAsync(TaskTimeLog log);

    Task<List<TaskTimeLog>> GetLogsAsync(string userId, DateTime fromDate, DateTime toDate);
}