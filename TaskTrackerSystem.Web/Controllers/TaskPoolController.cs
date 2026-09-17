using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Infrastructure.Persistence;
using TaskTrackerSystem.Domain.Constants;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize]
public class TaskPoolController : Controller
{
    private readonly ITaskAssignmentService _service;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public TaskPoolController(
        ITaskAssignmentService service,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender)
    {
        _service = service;
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user?.DepartmentId == null)
            return View(new List<TaskTrackerSystem.Application.DTOs.TaskAssignmentPostDto>());

        var pool = await _service.GetPoolForDepartmentAsync(user.DepartmentId.Value);

        return View(pool);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Claim(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user?.DepartmentId == null)
            return NotFound();

        var usersInDepartment = _userManager.Users
            .Where(x => x.DepartmentId == user.DepartmentId)
            .ToList();

        ApplicationUser? departmentManager = null;

        foreach (var candidate in usersInDepartment)
        {
            if (await _userManager.IsInRoleAsync(candidate, Roles.DepartmentManager))
            {
                departmentManager = candidate;
                break;
            }
        }

        if (departmentManager == null)
        {
            TempData["Error"] =
                "Bu departmana atanmış bir departman yöneticisi bulunamadı.";

            return RedirectToAction(nameof(Index));
        }

        var result = await _service.ClaimAsync(
            id,
            user.Id,
            user.DepartmentId.Value,
            departmentManager.Id);

        if (result == null)
        {
            TempData["Error"] =
                "Bu görev artık üstlenilemez (biri sizden önce aldı).";

            return RedirectToAction(nameof(Index));
        }

        if (!string.IsNullOrWhiteSpace(departmentManager.Email))
        {
            await _emailSender.SendAsync(
                departmentManager.Email,
                $"Görev üstlenildi: {result.Title}",
                $"<p><strong>{user.Name}</strong>, \"<strong>{result.Title}</strong>\" görevini üstlendi ve onayınızı bekliyor.</p>");
        }

        TempData["Success"] =
            "Görevi üstlendiniz, yönetici onayı bekleniyor.";

        return RedirectToAction(nameof(Index));
    }
}