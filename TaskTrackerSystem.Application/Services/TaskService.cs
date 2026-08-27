using AutoMapper;
using TaskTrackerSystem.Application.DTOs;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Domain.Entities;
using DomainTaskStatus = TaskTrackerSystem.Domain.Enums.TaskStatus;
using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    private readonly ITaskTimeLogRepository _timeLogRepo;
    private readonly IMapper _mapper;

    public TaskService(
        ITaskRepository repo,
        ITaskTimeLogRepository timeLogRepo,
        IMapper mapper)
    {
        _repo = repo;
        _timeLogRepo = timeLogRepo;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TaskDto>> GetTasksAsync(string userId)
        => _mapper.Map<List<TaskDto>>(
            await _repo.GetByUserAsync(userId));

    public async Task<TaskDto?> GetTaskAsync(int id, string userId)
    {
        var x = await _repo.GetByIdAsync(id, userId);

        return x == null
            ? null
            : _mapper.Map<TaskDto>(x);
    }

    public async Task<int> CreateAsync(
        CreateTaskDto dto,
        string userId)
    {
        var x = _mapper.Map<TaskItem>(dto);

        x.UserId = userId;
        x.Status = DomainTaskStatus.Pending;
        x.CreatedAt = DateTime.UtcNow;

        await _repo.AddAsync(x);
        await _repo.SaveChangesAsync();

        return x.Id;
    }

    public async Task<bool> UpdateAsync(
        UpdateTaskDto dto,
        string userId)
    {
        var x = await _repo.GetByIdAsync(dto.Id, userId);

        if (x == null)
            return false;

        var dueDateChanged = x.DueDate != dto.DueDate;
        var oldStatus = x.Status;

        _mapper.Map(dto, x);

        x.UpdatedAt = DateTime.UtcNow;

        if (dueDateChanged)
            x.ReminderSent = false;

        await HandleStatusTransitionAsync(x, oldStatus, userId);

        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        var x = await _repo.GetByIdAsync(id, userId);

        if (x == null)
            return false;

        _repo.Remove(x);

        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CompleteAsync(
        int id,
        string userId)
    {
        var x = await _repo.GetByIdAsync(id, userId);

        if (x == null)
            return false;

        var oldStatus = x.Status;

        x.Status = DomainTaskStatus.Completed;
        x.UpdatedAt = DateTime.UtcNow;

        await HandleStatusTransitionAsync(x, oldStatus, userId);

        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<List<TaskReminderDto>> GetTasksDueForReminderAsync(
        DateTime fromDate,
        DateTime toDate)
    {
        var tasks = await _repo.GetTasksDueForReminderAsync(fromDate, toDate);

        return tasks.Select(x => new TaskReminderDto
        {
            Id = x.Id,
            Title = x.Title,
            DueDate = x.DueDate,
            UserId = x.UserId
        }).ToList();
    }

    public Task MarkReminderSentAsync(int taskId)
        => _repo.MarkReminderSentAsync(taskId);

    public async Task<List<DailyTimeDto>> GetDailyTimeSummaryAsync(
        string userId,
        DateTime fromDate,
        DateTime toDate)
    {
        var logs = await _timeLogRepo.GetLogsAsync(userId, fromDate, toDate);

        return logs
            .Select(l => new
            {
                Date = l.StartedAt.Date,
                Minutes = ((l.EndedAt ?? DateTime.Now) - l.StartedAt).TotalMinutes
            })
            .GroupBy(x => x.Date)
            .Select(g => new DailyTimeDto
            {
                Date = g.Key,
                TotalMinutes = g.Sum(x => x.Minutes)
            })
            .OrderBy(x => x.Date)
            .ToList();
    }

    public async Task<ActiveTaskDto?> GetActiveTaskAsync(string userId)
    {
        var task = await _repo.GetInProgressTaskAsync(userId);

        if (task == null)
            return null;

        var logs = await _timeLogRepo.GetLogsForTaskAsync(task.Id);

        var totalMinutes = logs.Sum(l =>
            ((l.EndedAt ?? DateTime.Now) - l.StartedAt).TotalMinutes);

        var isRunning = logs.Any(l => l.EndedAt == null);

        return new ActiveTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            DueDate = task.DueDate,
            IsRunning = isRunning,
            ElapsedMinutes = (int)Math.Round(totalMinutes)
        };
    }

    public async Task<bool> StartAsync(int id, string userId)
    {
        var x = await _repo.GetByIdAsync(id, userId);

        if (x == null)
            return false;

        var oldStatus = x.Status;

        x.Status = DomainTaskStatus.InProgress;
        x.UpdatedAt = DateTime.UtcNow;

        await HandleStatusTransitionAsync(x, oldStatus, userId);

        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ArchiveAsync(int id, string userId)
    {
        var x = await _repo.GetByIdAsync(id, userId);

        if (x == null)
            return false;

        x.IsArchived = true;
        x.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UnarchiveAsync(int id, string userId)
    {
        var x = await _repo.GetByIdAsync(id, userId);

        if (x == null)
            return false;

        x.IsArchived = false;
        x.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PauseAsync(int id, string userId)
    {
        var x = await _repo.GetByIdAsync(id, userId);

        if (x == null)
            return false;

        var openSession = await _timeLogRepo.GetOpenSessionAsync(id);

        if (openSession != null)
            openSession.EndedAt = DateTime.Now;

        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ResumeAsync(int id, string userId)
    {
        var x = await _repo.GetByIdAsync(id, userId);

        if (x == null)
            return false;

        var openSession = await _timeLogRepo.GetOpenSessionAsync(id);

        if (openSession == null)
        {
            await _timeLogRepo.AddAsync(new TaskTimeLog
            {
                TaskItemId = x.Id,
                UserId = userId,
                StartedAt = DateTime.Now
            });
        }

        await _repo.SaveChangesAsync();

        return true;
    }

    private async Task HandleStatusTransitionAsync(
        TaskItem task,
        DomainTaskStatus oldStatus,
        string userId)
    {
        var newStatus = task.Status;

        if (oldStatus == newStatus)
            return;

        if (newStatus == DomainTaskStatus.InProgress)
        {
            var openSession = await _timeLogRepo.GetOpenSessionAsync(task.Id);

            if (openSession == null)
            {
                await _timeLogRepo.AddAsync(new TaskTimeLog
                {
                    TaskItemId = task.Id,
                    UserId = userId,
                    StartedAt = DateTime.Now
                });
            }
        }
        else if (oldStatus == DomainTaskStatus.InProgress)
        {
            var openSession = await _timeLogRepo.GetOpenSessionAsync(task.Id);

            if (openSession != null)
                openSession.EndedAt = DateTime.Now;
        }
    }
}