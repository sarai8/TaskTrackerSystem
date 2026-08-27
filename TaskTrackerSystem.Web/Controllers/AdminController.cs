using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerSystem.Infrastructure.Persistence;
using TaskTrackerSystem.Web.ViewModels;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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
            list.Add(new AdminUserListItemViewModel
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email!,
                UserName = u.UserName!,
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

        var vm = new AdminUserListItemViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email!,
            UserName = user.UserName!,
            CreatedAt = user.CreatedAt,
            IsActive = !await _userManager.IsLockedOutAsync(user)
        };

        return View(vm);
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
            UserName = user.UserName!
        };

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminUserEditViewModel m)
    {
        if (!ModelState.IsValid)
            return View(m);

        var user = await _userManager.FindByIdAsync(m.Id);
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
}