using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Infrastructure.Persistence;
using TaskTrackerSystem.Web.ViewModels;
using TaskTrackerSystem.Domain.Constants;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize(Roles = Roles.ManagerAndDirector)]
public class DepartmentRequestsController : Controller
{
    private readonly IDepartmentJoinRequestService _service;
    private readonly UserManager<ApplicationUser> _userManager;

    public DepartmentRequestsController(
        IDepartmentJoinRequestService service,
        UserManager<ApplicationUser> userManager)
    {
        _service = service;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var pending = User.IsInRole(Roles.Director)
            ? await _service.GetAllPendingAsync()   
            : await GetPendingForCurrentManagerAsync();

        var vmList = new List<DepartmentRequestViewModel>();

        foreach (var r in pending)
        {
            var user = await _userManager.FindByIdAsync(r.UserId);

            vmList.Add(new DepartmentRequestViewModel
            {
                Id = r.Id,
                UserName = user?.Name ?? "(silinmiş kullanıcı)",
                UserEmail = user?.Email ?? "",
                DepartmentName = r.DepartmentName,
                RequestedAt = r.RequestedAt
            });
        }

        return View(vmList);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var dto = await _service.ApproveAsync(id);

        if (dto == null)
            return NotFound();

        var user = await _userManager.FindByIdAsync(dto.UserId);

        if (user != null)
        {
            user.DepartmentId = dto.DepartmentId;
            await _userManager.UpdateAsync(user);
        }

        TempData["Success"] = "İstek onaylandı.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        if (await _service.RejectAsync(id) == null)
            return NotFound();

        TempData["Success"] = "İstek reddedildi.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<TaskTrackerSystem.Application.DTOs.DepartmentJoinRequestDto>> GetPendingForCurrentManagerAsync()
    {
        var me = await _userManager.GetUserAsync(User);

        if (me?.DepartmentId == null)
            return new();

        return await _service.GetPendingForDepartmentsAsync(new List<int> { me.DepartmentId.Value });
    }
}