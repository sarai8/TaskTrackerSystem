using TaskTrackerSystem.Application.DTOs;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Domain.Entities;
using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Application.Services;

public class DepartmentJoinRequestService : IDepartmentJoinRequestService
{
    private readonly IDepartmentJoinRequestRepository _repo;
    private readonly IDepartmentRepository _departmentRepo;

    public DepartmentJoinRequestService(
        IDepartmentJoinRequestRepository repo,
        IDepartmentRepository departmentRepo)
    {
        _repo = repo;
        _departmentRepo = departmentRepo;
    }

    public async Task RequestJoinAsync(string userId, int departmentId)
    {
        var existing = await _repo.GetPendingByUserIdAsync(userId);

        if (existing != null)
        {
            existing.DepartmentId = departmentId;
            existing.RequestedAt = DateTime.UtcNow;
        }
        else
        {
            await _repo.AddAsync(new DepartmentJoinRequest
            {
                UserId = userId,
                DepartmentId = departmentId
            });
        }

        await _repo.SaveChangesAsync();
    }


    public async Task<DepartmentJoinRequestDto?> GetLatestForUserAsync(string userId)
    {
        var req = await _repo.GetLatestByUserIdAsync(userId);

        if (req == null)
            return null;

        var dept = await _departmentRepo.GetByIdAsync(req.DepartmentId);

        return new DepartmentJoinRequestDto
        {
            Id = req.Id,
            UserId = req.UserId,
            DepartmentId = req.DepartmentId,
            DepartmentName = dept?.Name ?? "(silinmiş departman)",
            RequestedAt = req.RequestedAt,
            Status = req.Status
        };
    }

   
    public async Task<List<DepartmentJoinRequestDto>> GetAllPendingAsync()
    {
        var requests = await _repo.GetAllPendingAsync();
        return await ToDtosAsync(requests);
    }

    public async Task<List<DepartmentJoinRequestDto>> GetPendingForDepartmentsAsync(List<int> departmentIds)
    {
        var requests = await _repo.GetPendingByDepartmentIdsAsync(departmentIds);
        return await ToDtosAsync(requests);
    }

    public async Task<DepartmentJoinRequestDto?> ApproveAsync(int requestId)
    {
        var req = await _repo.GetByIdAsync(requestId);

        if (req == null || req.Status != JoinRequestStatus.Pending)
            return null;

        req.Status = JoinRequestStatus.Approved;
        req.ReviewedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync();

        return new DepartmentJoinRequestDto { Id = req.Id, UserId = req.UserId, DepartmentId = req.DepartmentId };
    }

    public async Task<DepartmentJoinRequestDto?> RejectAsync(int requestId)
    {
        var req = await _repo.GetByIdAsync(requestId);

        if (req == null || req.Status != JoinRequestStatus.Pending)
            return null;

        req.Status = JoinRequestStatus.Rejected;
        req.ReviewedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync();

        return new DepartmentJoinRequestDto { Id = req.Id, UserId = req.UserId, DepartmentId = req.DepartmentId };
    }

    private async Task<List<DepartmentJoinRequestDto>> ToDtosAsync(List<DepartmentJoinRequest> requests)
    {
        var result = new List<DepartmentJoinRequestDto>();

        foreach (var r in requests)
        {
            var dept = await _departmentRepo.GetByIdAsync(r.DepartmentId);

            result.Add(new DepartmentJoinRequestDto
            {
                Id = r.Id,
                UserId = r.UserId,
                DepartmentId = r.DepartmentId,
                DepartmentName = dept?.Name ?? "(silinmiş departman)",
                RequestedAt = r.RequestedAt
            });
        }

        return result;
    }
}