using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerSystem.Application.DTOs;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Infrastructure.Persistence;
using TaskTrackerSystem.Web.ViewModels;
using TaskTrackerSystem.Domain.Constants;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize(Roles = Roles.ManagerAndDirector)]
public class TaskAssignmentsController : Controller
{
    private readonly ITaskAssignmentService _service;
    private readonly IDepartmentService _departmentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TaskAssignmentsController(
        ITaskAssignmentService service,
        IDepartmentService departmentService,
        UserManager<ApplicationUser> userManager)
    {
        _service = service;
        _departmentService = departmentService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateViewBagAsync();
        return View(new TaskAssignmentCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskAssignmentCreateViewModel m)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var isDeptManagerOnly = await IsDepartmentManagerOnlyAsync(currentUser!);

        if (isDeptManagerOnly)
        {
            m.DepartmentId = currentUser!.DepartmentId;

            if (m.AssignMode == "user" && !string.IsNullOrEmpty(m.UserId))
            {
                var targetUser = await _userManager.FindByIdAsync(m.UserId);

                if (targetUser?.DepartmentId != currentUser.DepartmentId)
                    ModelState.AddModelError("", "Sadece kendi departmanınızdaki kişilere atama yapabilirsiniz.");
            }
        }

        if (m.AssignMode == "department" && m.DepartmentId == null)
            ModelState.AddModelError(nameof(m.DepartmentId), "Bir departman seçin.");

        if (m.AssignMode == "user" && string.IsNullOrEmpty(m.UserId))
            ModelState.AddModelError(nameof(m.UserId), "Bir kişi seçin.");

        if (!ModelState.IsValid)
        {
            await PopulateViewBagAsync();
            return View(m);
        }

        var managerId = _userManager.GetUserId(User)!;

        var dto = new CreateTaskAssignmentDto
        {
            Title = m.Title,
            Description = m.Description,
            Priority = m.Priority,
            DueDate = m.DueDate
        };

        if (m.AssignMode == "department")
            await _service.AssignToDepartmentAsync(managerId, m.DepartmentId!.Value, dto);
        else
            await _service.AssignToUserAsync(managerId, m.UserId!, dto);

        TempData["Success"] = "Görev ataması yapıldı.";

        return RedirectToAction(nameof(Create));
    }

    private async Task PopulateViewBagAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var isDeptManagerOnly = await IsDepartmentManagerOnlyAsync(currentUser!);

        if (isDeptManagerOnly && currentUser!.DepartmentId.HasValue)
        {
            var dept = await _departmentService.GetByIdAsync(currentUser.DepartmentId.Value);

            ViewBag.Departments = dept == null
                ? new List<DepartmentDto>()
                : new List<DepartmentDto> { dept };

            ViewBag.Users = _userManager.Users
                .Where(u => u.DepartmentId == currentUser.DepartmentId)
                .OrderBy(u => u.Name)
                .ToList();

            ViewBag.LockedDepartmentName = dept?.Name;
        }
        else
        {
            ViewBag.Departments = await _departmentService.GetAllAsync();
            ViewBag.Users = _userManager.Users.OrderBy(u => u.Name).ToList();
            ViewBag.LockedDepartmentName = null;
        }
    }

    private async Task<bool> IsDepartmentManagerOnlyAsync(ApplicationUser user)
    {
        var isDirector = await _userManager.IsInRoleAsync(user, Roles.Director);
        var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);

        return !isDirector && !isAdmin;
    }
}