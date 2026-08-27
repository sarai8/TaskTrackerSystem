using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskTrackerSystem.Application.DTOs;
using TaskTrackerSystem.Application.Interfaces;
using DomainTaskStatus = TaskTrackerSystem.Domain.Enums.TaskStatus;
using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ITaskService _tasks;

    public DashboardController(ITaskService tasks)
        => _tasks = tasks;

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var list = await _tasks.GetTasksAsync(userId);
        var activeTask = await _tasks.GetActiveTaskAsync(userId);

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
                activeTask));
    }
}

public record DashboardVm(
    int Total,
    int Completed,
    int Pending,
    int Overdue,
    List<TaskDto> Recent,
    ActiveTaskDto? ActiveTask);