using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Domain.Entities;
using TaskTrackerSystem.Infrastructure.Persistence;
using TaskTrackerSystem.Web.Common;
using TaskTrackerSystem.Web.ViewModels;
using TaskTrackerSystem.Domain.Constants;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITaskService _taskService;
    private readonly IDepartmentService _departmentService;
    private readonly IDepartmentJoinRequestService _joinRequestService;
    private readonly IEmailSender _emailSender;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        ITaskService taskService,
        IDepartmentService departmentService,
         IDepartmentJoinRequestService joinRequestService,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _taskService = taskService;
        _departmentService = departmentService;
        _joinRequestService = joinRequestService;
        _emailSender = emailSender;


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

        var departmentName = "Departman yok";

        if (user.DepartmentId.HasValue)
        {
            var dept = await _departmentService.GetByIdAsync(user.DepartmentId.Value);
            departmentName = dept?.Name ?? "Departman yok";
        }

        var position = "Çalışan";

        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            position = "Admin";
        else if (await _userManager.IsInRoleAsync(user, Roles.Director))
            position = "Müdür";
        else if (await _userManager.IsInRoleAsync(user, Roles.DepartmentManager))
            position = "Departman Yöneticisi";

        var vm = new ProfileViewModel
        {
            Name = user.Name,
            Email = user.Email!,
            UserName = user.UserName!,
            DepartmentName = departmentName,
            Position = position,
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
            UserName = user.UserName!,
            DepartmentId = user.DepartmentId
        };

        ViewBag.Departments = await _departmentService.GetAllAsync();

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileEditViewModel m)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(m);
        }

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

            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(m);
        }

        if (m.DepartmentId.HasValue && m.DepartmentId != user.DepartmentId)
        {
            await _joinRequestService.RequestJoinAsync(user.Id, m.DepartmentId.Value);
            await NotifyApproversAsync(m.DepartmentId.Value, user);

            TempData["Success"] = "Profil bilgileriniz güncellendi. Departman değişikliği onay bekliyor.";
        }
        else
        {
            TempData["Success"] = "Profil bilgileriniz güncellendi.";
        }

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

    private async Task NotifyApproversAsync(int departmentId, ApplicationUser requester)
    {
        var department = await _departmentService.GetByIdAsync(departmentId);
        var departmentName = department?.Name ?? "Departman";

        var directors = await _userManager.GetUsersInRoleAsync(Roles.Director);
        var deptManagers = await _userManager.GetUsersInRoleAsync(Roles.DepartmentManager);

        var approvers = directors
            .Concat(deptManagers.Where(m => m.DepartmentId == departmentId))
            .GroupBy(u => u.Id)
            .Select(g => g.First());

        var subject = $"Departman katılım onayı bekleniyor: {departmentName}";
        var body = $"<p>{requester.Name} ({requester.Email}), <strong>{departmentName}</strong> departmanına katılmak için istek gönderdi.</p><p>Onaylamak/reddetmek için \"Departman İstekleri\" sayfasını ziyaret edin.</p>";

        foreach (var approver in approvers)
        {
            if (!string.IsNullOrWhiteSpace(approver.Email))
                await _emailSender.SendAsync(approver.Email, subject, body);
        }
    }
}