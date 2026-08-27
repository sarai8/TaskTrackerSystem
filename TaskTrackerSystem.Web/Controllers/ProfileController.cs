using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Infrastructure.Persistence;
using TaskTrackerSystem.Web.ViewModels;
using TaskTrackerSystem.Web.Common;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITaskService _taskService;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        ITaskService taskService)
    {
        _userManager = userManager;
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var toDate = DateTime.Now.Date.AddDays(1);
        var fromDate = toDate.AddDays(-14);

        var summary = await _taskService.GetDailyTimeSummaryAsync(user.Id, fromDate, toDate);

        var chart = new List<DailyTimeChartItem>();

        for (var day = fromDate; day < toDate; day = day.AddDays(1))
        {
            var match = summary.FirstOrDefault(x => x.Date == day);

            chart.Add(new DailyTimeChartItem
            {
                DateLabel = day.ToString("dd.MM"),
                Minutes = match != null ? (int)Math.Round(match.TotalMinutes) : 0
            });
        }

        var vm = new ProfileViewModel
        {
            Name = user.Name,
            Email = user.Email!,
            UserName = user.UserName!,
            CreatedAt = user.CreatedAt,
            AvatarId = user.AvatarId,
            TimeChart = chart
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var vm = new ProfileEditViewModel
        {
            Name = user.Name,
            Email = user.Email!,
            UserName = user.UserName!
        };

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileEditViewModel m)
    {
        if (!ModelState.IsValid)
            return View(m);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.Name = m.Name;
        user.Email = m.Email;
        user.NormalizedEmail = _userManager.NormalizeEmail(m.Email);
        user.UserName = m.UserName;
        user.NormalizedUserName = _userManager.NormalizeName(m.UserName);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);

            return View(m);
        }

        TempData["Success"] = "Profil bilgileriniz güncellendi.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Avatar()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        ViewBag.SelectedAvatarId = user.AvatarId;

        return View(AvatarCatalog.Options);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Avatar(string avatarId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (AvatarCatalog.Options.Any(x => x.Id == avatarId))
        {
            user.AvatarId = avatarId;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Avatarınız güncellendi.";
        }

        return RedirectToAction("Index");
    }
}