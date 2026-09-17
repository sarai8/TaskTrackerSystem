using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Infrastructure.Persistence;
using TaskTrackerSystem.Web.ViewModels;
using TaskTrackerSystem.Domain.Constants;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDepartmentService _departmentService;

    public AdminController(UserManager<ApplicationUser> userManager,
        IDepartmentService departmentService)
    {
        _userManager = userManager;
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users
            .OrderBy(u => u.Name)
            .ToList();

        var list = new List<AdminUserListItemViewModel>();

        foreach (var u in users)
        {
            var departmentName = "Departman yok";

            if (u.DepartmentId.HasValue)
            {
                var dept = await _departmentService.GetByIdAsync(u.DepartmentId.Value);
                departmentName = dept?.Name ?? "Departman yok";
            }

            var position = "Çalışan";

            if (await _userManager.IsInRoleAsync(u, Roles.Admin))
                position = "Admin";
            else if (await _userManager.IsInRoleAsync(u, Roles.Director))
                position = "Müdür";
            else if (await _userManager.IsInRoleAsync(u, Roles.DepartmentManager))
                position = "Departman Yöneticisi";

            list.Add(new AdminUserListItemViewModel
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email!,
                UserName = u.UserName!,
                DepartmentName = departmentName,
                Position = position,
                CreatedAt = u.CreatedAt,
                IsActive = !await _userManager.IsLockedOutAsync(u)
            });
        }

        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

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

        var vm = new AdminUserListItemViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email!,
            UserName = user.UserName!,
            DepartmentName = departmentName,
            Position = position,
            CreatedAt = user.CreatedAt,
            IsActive = !await _userManager.IsLockedOutAsync(user)
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Departments = await _departmentService.GetAllAsync();

        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserCreateViewModel m)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(m);
        }

        var user = new ApplicationUser
        {
            Name = m.Name,
            Email = m.Email,
            UserName = m.UserName,
            DepartmentId = m.DepartmentId,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, m.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.Departments = await _departmentService.GetAllAsync();

            return View(m);
        }

        if (m.Role is Roles.Admin or Roles.Director or Roles.DepartmentManager)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, m.Role);

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                ViewBag.Departments = await _departmentService.GetAllAsync();

                return View(m);
            }
        }

        TempData["Success"] = "Yeni kullanıcı başarıyla oluşturuldu.";

        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var vm = new AdminUserEditViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email!,
            UserName = user.UserName!,
            DepartmentId = user.DepartmentId,
            Role = await _userManager.IsInRoleAsync(user, Roles.Admin)
                ? Roles.Admin
                : await _userManager.IsInRoleAsync(user, Roles.Director)
                    ? Roles.Director
                    : await _userManager.IsInRoleAsync(user, Roles.DepartmentManager)
                        ? Roles.DepartmentManager
                        : Roles.Employee
        };

        ViewBag.Departments = await _departmentService.GetAllAsync();

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminUserEditViewModel m)
    {
        var currentUserId = _userManager.GetUserId(User);

        if (m.Id == currentUserId && m.Role != Roles.Admin && await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), Roles.Admin))
        {
            TempData["Error"] = "Kendi admin yetkinizi bu ekrandan kaldıramazsınız.";
            return RedirectToAction(nameof(Index));
        }
        if (!ModelState.IsValid)
            return View(m);

        var user = await _userManager.FindByIdAsync(m.Id);
        if (user == null) return NotFound();

        user.Name = m.Name;
        user.Email = m.Email;
        user.NormalizedEmail = _userManager.NormalizeEmail(m.Email);
        user.UserName = m.UserName;
        user.DepartmentId = m.DepartmentId;
        user.NormalizedUserName = _userManager.NormalizeName(m.UserName);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);

            return View(m);
        }

        foreach (var role in Roles.All)
        {
            if (await _userManager.IsInRoleAsync(user, role))
                await _userManager.RemoveFromRoleAsync(user, role);
        }

        if (Roles.All.Contains(m.Role))
            await _userManager.AddToRoleAsync(user, m.Role);

        TempData["Success"] = "Kullanıcı bilgileri güncellendi.";
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Kendi hesabınızı silemezsiniz.";
            return RedirectToAction("Index");
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        await _userManager.DeleteAsync(user);

        TempData["Success"] = "Kullanıcı silindi.";
        return RedirectToAction("Index");
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        if (id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Kendi hesabınızı pasif hale getiremezsiniz.";
            return RedirectToAction("Index");
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var isLockedOut = await _userManager.IsLockedOutAsync(user);

        if (isLockedOut)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            TempData["Success"] = "Kullanıcı aktif hale getirildi.";
        }
        else
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            TempData["Success"] = "Kullanıcı pasif hale getirildi.";
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        var model = new AdminChangePasswordViewModel
        {
            UserId = user.Id,
            UserName = user.UserName ?? ""
        };

        return View(model);
    }

    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ChangePassword(AdminChangePasswordViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var user = await _userManager.FindByIdAsync(model.UserId);

    if (user == null)
        return NotFound();

    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

    var result = await _userManager.ResetPasswordAsync(
        user,
        token,
        model.NewPassword
    );

    if (!result.Succeeded)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

    TempData["Success"] =
        $"{user.UserName} kullanıcısının şifresi başarıyla değiştirildi.";

    return RedirectToAction(nameof(Index));
}
}