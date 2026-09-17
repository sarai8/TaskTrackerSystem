using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskTrackerSystem.Application.DTOs;
using TaskTrackerSystem.Application.Interfaces;
using DomainTaskStatus = TaskTrackerSystem.Domain.Enums.TaskStatus;
using TaskTrackerSystem.Domain.Enums;
using TaskTrackerSystem.Domain.Constants;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ITaskService _tasks;
    private readonly IDepartmentJoinRequestService _joinRequests;

    public DashboardController(ITaskService tasks, IDepartmentJoinRequestService joinRequests)
    {
        _tasks = tasks;
        _joinRequests = joinRequests;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole(Roles.Admin))
            return RedirectToAction("Index", "AdminDashboard");

        if (User.IsInRole(Roles.DepartmentManager) || User.IsInRole(Roles.Director))
            return RedirectToAction("Index", "ManagerDashboard");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var list = await _tasks.GetTasksAsync(userId);
        var activeTask = await _tasks.GetActiveTaskAsync(userId);
        var joinRequest = await _joinRequests.GetLatestForUserAsync(userId);

        var today = DateTime.Today;

        return View(
            new DashboardVm(
                list.Count,
                list.Count(x => x.Status == DomainTaskStatus.Completed),
                list.Count(x => x.Status != DomainTaskStatus.Completed),
                list.Count(x =>
                    x.Status != DomainTaskStatus.Completed &&
                    x.DueDate.Date < today),
                list.Take(5).ToList(),
                activeTask,
                joinRequest));
    }
}

public record DashboardVm(
    int Total,
    int Completed,
    int Pending,
    int Overdue,
    List<TaskDto> Recent,
    ActiveTaskDto? ActiveTask,
    DepartmentJoinRequestDto? JoinRequest);