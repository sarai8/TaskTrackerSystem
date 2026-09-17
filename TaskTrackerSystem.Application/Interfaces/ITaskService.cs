using TaskTrackerSystem.Application.DTOs;

namespace TaskTrackerSystem.Application.Interfaces;

public interface ITaskService
{
    Task<IReadOnlyList<TaskDto>> GetTasksAsync(string userId);
    Task<IReadOnlyList<TaskDto>> GetAllTasksAsync();

    Task<TaskDto?> GetTaskAsync(int id, string userId);

    Task<int> CreateAsync(CreateTaskDto dto, string userId);

    Task<bool> UpdateAsync(UpdateTaskDto dto, string userId);

    Task<bool> DeleteAsync(int id, string userId);

    Task<bool> CompleteAsync(int id, string userId);

    Task<List<TaskReminderDto>> GetTasksDueForReminderAsync(DateTime fromDate, DateTime toDate);

    Task MarkReminderSentAsync(int taskId);

    Task<List<DailyTimeDto>> GetDailyTimeSummaryAsync(string userId, DateTime fromDate, DateTime toDate);

    Task<ActiveTaskDto?> GetActiveTaskAsync(string userId);

    Task<bool> PauseAsync(int id, string userId);

    Task<bool> StartAsync(int id, string userId);

    Task<bool> ArchiveAsync(int id, string userId);

    Task<bool> UnarchiveAsync(int id, string userId);

    Task<bool> ResumeAsync(int id, string userId);
}