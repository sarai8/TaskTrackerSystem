using TaskTrackerSystem.Application.DTOs;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Domain.Entities;
using TaskTrackerSystem.Domain.Enums;
using DomainTaskStatus = TaskTrackerSystem.Domain.Enums.TaskStatus;

namespace TaskTrackerSystem.Application.Services;

public class TaskAssignmentService : ITaskAssignmentService
{
    private readonly ITaskAssignmentRepository _repo;
    private readonly ITaskRepository _taskRepo;

    public TaskAssignmentService(
        ITaskAssignmentRepository repo,
        ITaskRepository taskRepo)
    {
        _repo = repo;
        _taskRepo = taskRepo;
    }

    public async Task AssignToDepartmentAsync(
        string managerId,
        int departmentId,
        CreateTaskAssignmentDto dto)
    {
        await _repo.AddAsync(new TaskAssignmentPost
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            AssignedByUserId = managerId,
            DepartmentId = departmentId,
            Status = TaskAssignmentStatus.PendingClaim
        });

        await _repo.SaveChangesAsync();
    }

    public async Task AssignToUserAsync(
        string managerId,
        string userId,
        CreateTaskAssignmentDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            Status = DomainTaskStatus.Pending,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _taskRepo.AddAsync(task);
        await _taskRepo.SaveChangesAsync();

        await _repo.AddAsync(new TaskAssignmentPost
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            AssignedByUserId = managerId,
            AssignedToUserId = userId,
            Status = TaskAssignmentStatus.DirectAssigned,
            CreatedTaskId = task.Id
        });

        await _repo.SaveChangesAsync();
    }

    public async Task<List<TaskAssignmentPostDto>> GetPoolForDepartmentAsync(int departmentId)
    {
        var posts = await _repo.GetPoolForDepartmentAsync(departmentId);

        return posts.Select(ToDto).ToList();
    }

    public async Task<TaskAssignmentPostDto?> ClaimAsync(
    int postId,
    string userId,
    int userDepartmentId,
    string departmentManagerId)
    {
        var post = await _repo.GetByIdAsync(postId);

        if (post == null)
            return null;

        if (post.Status != TaskAssignmentStatus.PendingClaim)
            return null;

        if (post.DepartmentId != userDepartmentId)
            return null;

        post.AssignedToUserId = userId;
        post.AssignedByUserId = departmentManagerId;
        post.Status = TaskAssignmentStatus.PendingApproval;

        await _repo.SaveChangesAsync();

        return ToDto(post);
    }

    public async Task<List<TaskAssignmentPostDto>> GetPendingApprovalByManagerAsync(string managerId)
    {
        var posts = await _repo.GetPendingApprovalByManagerAsync(managerId);

        return posts.Select(ToDto).ToList();
    }

    public async Task<List<TaskAssignmentPostDto>> GetAllPendingApprovalAsync()
    {
        var posts = await _repo.GetAllPendingApprovalAsync();
        return posts.Select(ToDto).ToList();
    }

    public async Task<TaskAssignmentPostDto?> ApproveAsync(
        int postId,
        string approverId,
        bool canApproveAny)
    {
        var post = await _repo.GetByIdAsync(postId);

        if (post == null)
            return null;

        if (!canApproveAny && post.AssignedByUserId != approverId)
            return null;

        if (post.Status != TaskAssignmentStatus.PendingApproval || post.AssignedToUserId == null)
            return null;

        var task = new TaskItem
        {
            Title = post.Title,
            Description = post.Description,
            Priority = post.Priority,
            DueDate = post.DueDate,
            Status = DomainTaskStatus.Pending,
            UserId = post.AssignedToUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _taskRepo.AddAsync(task);
        await _taskRepo.SaveChangesAsync();

        post.Status = TaskAssignmentStatus.Approved;
        post.CreatedTaskId = task.Id;

        await _repo.SaveChangesAsync();

        return ToDto(post);
    }

    public async Task<TaskAssignmentPostDto?> RejectAsync(
        int postId,
        string approverId,
        bool canApproveAny)
    {
        var post = await _repo.GetByIdAsync(postId);

        if (post == null)
            return null;

        if (!canApproveAny && post.AssignedByUserId != approverId)
            return null;

        if (post.Status != TaskAssignmentStatus.PendingApproval)
            return null;

        post.Status = TaskAssignmentStatus.PendingClaim;
        post.AssignedToUserId = null;

        await _repo.SaveChangesAsync();

        return ToDto(post);
    }

    private static TaskAssignmentPostDto ToDto(TaskAssignmentPost p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Description = p.Description,
        Priority = p.Priority,
        DueDate = p.DueDate,
        AssignedByUserId = p.AssignedByUserId,
        DepartmentId = p.DepartmentId,
        AssignedToUserId = p.AssignedToUserId,
        Status = p.Status,
        CreatedAt = p.CreatedAt
    };
}