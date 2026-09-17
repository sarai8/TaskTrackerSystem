using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Infrastructure.Persistence;
using TaskTrackerSystem.Domain.Constants;
using DomainTaskStatus = TaskTrackerSystem.Domain.Enums.TaskStatus;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminDashboardController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDepartmentService _departmentService;
    private readonly ITaskService _taskService;
    private readonly ITaskAssignmentService _taskAssignmentService;

    public AdminDashboardController(
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

    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var departments = await _departmentService.GetAllAsync();
        var allTasks = await _taskService.GetAllTasksAsync();
        var pendingApprovals = await _taskAssignmentService.GetAllPendingApprovalAsync();

        var userIdsByDept = users
            .Where(u => u.DepartmentId.HasValue)
            .ToLookup(u => u.DepartmentId!.Value, u => u.Id);

        var activeTaskCountByUser = allTasks
            .Where(t => t.Status != DomainTaskStatus.Completed)
            .GroupBy(t => t.UserId)
            .ToDictionary(g => g.Key, g => g.Count());

        var departmentSummaries = departments
            .Select(d => new DepartmentSummaryVm(
                d.Name,
                d.EmployeeCount,
                userIdsByDept[d.Id].Sum(uid => activeTaskCountByUser.GetValueOrDefault(uid, 0))))
            .ToList();

        int adminCount = 0, directorCount = 0, managerCount = 0;

        foreach (var u in users)
        {
            if (await _userManager.IsInRoleAsync(u, Roles.Admin))
                adminCount++;
            else if (await _userManager.IsInRoleAsync(u, Roles.Director))
                directorCount++;
            else if (await _userManager.IsInRoleAsync(u, Roles.DepartmentManager))
                managerCount++;
        }

        var employeeCount = users.Count - adminCount - directorCount - managerCount;

        var vm = new AdminDashboardVm(
            TotalUsers: users.Count,
            TotalDepartments: departments.Count,
            TotalTasks: allTasks.Count,
            PendingApprovals: pendingApprovals.Count,
            Pending: allTasks.Count(t => t.Status == DomainTaskStatus.Pending),
            InProgress: allTasks.Count(t => t.Status == DomainTaskStatus.InProgress),
            Completed: allTasks.Count(t => t.Status == DomainTaskStatus.Completed),
            AdminCount: adminCount,
            DirectorCount: directorCount,
            ManagerCount: managerCount,
            EmployeeCount: employeeCount,
            Departments: departmentSummaries);

        return View(vm);
    }
}

public record DepartmentSummaryVm(
    string Name,
    int EmployeeCount,
    int ActiveTaskCount);

public record AdminDashboardVm(
    int TotalUsers,
    int TotalDepartments,
    int TotalTasks,
    int PendingApprovals,
    int Pending,
    int InProgress,
    int Completed,
    int AdminCount,
    int DirectorCount,
    int ManagerCount,
    int EmployeeCount,
    List<DepartmentSummaryVm> Departments);