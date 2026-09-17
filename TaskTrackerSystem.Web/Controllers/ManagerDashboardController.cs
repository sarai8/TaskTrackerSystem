using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerSystem.Application.DTOs;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Domain.Constants;
using TaskTrackerSystem.Infrastructure.Persistence;
using DomainTaskStatus = TaskTrackerSystem.Domain.Enums.TaskStatus;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize(Roles = Roles.ManagerAndDirector)]
public class ManagerDashboardController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDepartmentService _departmentService;
    private readonly ITaskService _taskService;
    private readonly ITaskAssignmentService _taskAssignmentService;

    public ManagerDashboardController(
        UserManager<ApplicationUser> userManager,
        IDepartmentService departmentService,
        ITaskService taskService,
        ITaskAssignmentService taskAssignmentService)
    {
        _userManager = userManager;
        _departmentService = departmentService;
        _taskService = taskService;
        _taskAssignmentService = taskAssignmentService;
    }

    public async Task<IActionResult> Index(int? departmentId)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        var isDirector = User.IsInRole(Roles.Director);

        // Müdür için departman listesini ViewBag'e gönderiyoruz.
        if (isDirector)
        {
            ViewBag.Departments = await _departmentService.GetAllAsync();
            ViewBag.SelectedDepartmentId = departmentId;
        }

        int? selectedDepartmentId;

        if (isDirector)
        {
            selectedDepartmentId = departmentId;
        }
        else
        {
            selectedDepartmentId = currentUser?.DepartmentId;
        }

        if (selectedDepartmentId == null)
        {
            return View(new ManagerDashboardVm(
                null, 0, 0, 0, 0, 0, 0,
                new List<EmployeeWorkloadVm>(),
                new List<TaskAssignmentPostDto>()));
        }

        var department = await _departmentService
            .GetByIdAsync(selectedDepartmentId.Value);

        var employees = _userManager.Users
            .Where(u => u.DepartmentId == selectedDepartmentId.Value)
            .OrderBy(u => u.Name)
            .ToList();

        var employeeIds = employees
            .Select(e => e.Id)
            .ToHashSet();

        var allTasks = await _taskService.GetAllTasksAsync();

        var deptTasks = allTasks
            .Where(t => employeeIds.Contains(t.UserId))
            .ToList();

        var workload = employees
            .Select(e => new EmployeeWorkloadVm(
                e.Name,
                deptTasks.Count(t =>
                    t.UserId == e.Id &&
                    t.Status != DomainTaskStatus.Completed)))
            .OrderByDescending(w => w.TaskCount)
            .ToList();

        var unassignedPosts =
            await _taskAssignmentService
                .GetPoolForDepartmentAsync(selectedDepartmentId.Value);

        var vm = new ManagerDashboardVm(
            DepartmentName: department?.Name ?? "Departman",
            EmployeeCount: employees.Count,
            TotalTasks: deptTasks.Count,
            InProgress: deptTasks.Count(t =>
                t.Status == DomainTaskStatus.InProgress),
            Completed: deptTasks.Count(t =>
                t.Status == DomainTaskStatus.Completed),
            Pending: deptTasks.Count(t =>
                t.Status == DomainTaskStatus.Pending),
            UnassignedCount: unassignedPosts.Count,
            Workload: workload,
            UnassignedPosts: unassignedPosts);

        return View(vm);
    }
}

public record EmployeeWorkloadVm(
    string EmployeeName,
    int TaskCount);

public record ManagerDashboardVm(
    string? DepartmentName,
    int EmployeeCount,
    int TotalTasks,
    int InProgress,
    int Completed,
    int Pending,
    int UnassignedCount,
    List<EmployeeWorkloadVm> Workload,
    List<TaskAssignmentPostDto> UnassignedPosts);