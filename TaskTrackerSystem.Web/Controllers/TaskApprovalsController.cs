using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Infrastructure.Persistence;
using TaskTrackerSystem.Web.ViewModels;
using TaskTrackerSystem.Domain.Constants;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize(Roles = Roles.DepartmentManager)]
public class TaskApprovalsController : Controller
{
    private readonly ITaskAssignmentService _service;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public TaskApprovalsController(
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
        var managerId = _userManager.GetUserId(User)!;

        var pending = await _service
            .GetPendingApprovalByManagerAsync(managerId);

        var vmList = new List<TaskApprovalViewModel>();

        foreach (var p in pending)
        {
            var claimant = p.AssignedToUserId == null
                ? null
                : await _userManager.FindByIdAsync(p.AssignedToUserId);

            vmList.Add(new TaskApprovalViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Priority = p.Priority,
                DueDate = p.DueDate,
                ClaimedByName = claimant?.Name ?? "(bilinmiyor)",
                ClaimedByEmail = claimant?.Email ?? ""
            });
        }

        return View(vmList);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var approverId = _userManager.GetUserId(User)!;

        var result = await _service.ApproveAsync(
            id,
            approverId,
            false);

        if (result == null)
            return NotFound();

        if (result.AssignedToUserId != null)
        {
            var claimant = await _userManager.FindByIdAsync(result.AssignedToUserId);

            if (claimant != null && !string.IsNullOrWhiteSpace(claimant.Email))
            {
                await _emailSender.SendAsync(
                    claimant.Email,
                    $"Görev onaylandı: {result.Title}",
                    $"<p>\"<strong>{result.Title}</strong>\" görevi onaylandı, artık \"Görevlerim\" listenizde.</p>");
            }
        }

        TempData["Success"] = "Görev onaylandı.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var approverId = _userManager.GetUserId(User)!;

        var result = await _service.RejectAsync(
            id,
            approverId,
            false);

        if (result == null)
            return NotFound();

        TempData["Success"] = "Görev reddedildi, havuza geri döndü.";

        return RedirectToAction(nameof(Index));
    }
}