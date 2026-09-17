using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerSystem.Application.DTOs;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Domain.Entities;
using TaskTrackerSystem.Infrastructure.Persistence;
using TaskTrackerSystem.Web.ViewModels;
using TaskTrackerSystem.Domain.Constants;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize(Roles = Roles.AllRolesCombined)]
public class DepartmentsController : Controller
{
    private readonly IDepartmentService _service;
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly IEmailSender _emailSender;

    public DepartmentsController(IDepartmentService service, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
    {
        _service = service;
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var departments = await _service.GetAllAsync();
        return View(departments);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var department = await _service.GetByIdAsync(id);

        if (department == null)
            return NotFound();

        var users = _userManager.Users.ToList();

        var vm = new DepartmentDetailsViewModel
        {
            DepartmentName = department.Name
        };

        foreach (var u in users)
        {
            var isAdmin = await _userManager.IsInRoleAsync(u, Roles.Admin);
            var isDirector = await _userManager.IsInRoleAsync(u, Roles.Director);
            var isManager = await _userManager.IsInRoleAsync(u, Roles.DepartmentManager);

            var position = "Çalışan";

            if (isAdmin)
                position = "Admin";
            else if (isDirector)
                position = "Müdür";
            else if (isManager)
                position = "Departman Yöneticisi";

            var member = new DepartmentMemberViewModel
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email ?? "",
                Position = position
            };

      
            if (!u.DepartmentId.HasValue)
            {
                vm.UnassignedUsers.Add(member);
                continue;
            }

           
            if (u.DepartmentId.Value != id)
                continue;


            if (isAdmin || isDirector || isManager)
                vm.Managers.Add(member);
            else
                vm.Employees.Add(member);
        }

        return View(vm);
    }


    [HttpGet]
    public async Task<IActionResult> EditMember(string id, int departmentId)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        if (!await CanManageEmployeeAsync(user))
            return Forbid();


        if (user.DepartmentId.HasValue && user.DepartmentId != departmentId)
        {
            TempData["Error"] = "Bu kullanıcının departmanı değişmiş görünüyor. Liste güncellendi.";
            return RedirectToAction(nameof(Details), new { id = departmentId });
        }


        var departments = await _service.GetAllAsync();

        var vm = new EditDepartmentMemberViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email ?? "",
            DepartmentId = user.DepartmentId,
            ReturnDepartmentId = departmentId,
            Departments = departments.Select(d => new DepartmentSelectViewModel
            {
                Id = d.Id,
                Name = d.Name
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMember(EditDepartmentMemberViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);

        if (user == null)
            return NotFound();

        if (!await CanManageEmployeeAsync(user))
            return Forbid();

        if (user.DepartmentId.HasValue && user.DepartmentId != model.ReturnDepartmentId)
        {
            TempData["Error"] = "Bu kullanıcının departmanı değişmiş görünüyor. Liste güncellendi.";
            return RedirectToAction(nameof(Details), new { id = model.ReturnDepartmentId });
        }

        DepartmentDto? department = null;

        if (model.DepartmentId.HasValue)
        {
            department = await _service.GetByIdAsync(model.DepartmentId.Value);

            if (department == null)
            {
                ModelState.AddModelError(
                    nameof(model.DepartmentId),
                    "Seçilen departman bulunamadı.");
            }
        }

        if (!ModelState.IsValid)
        {
            var departments = await _service.GetAllAsync();

            model.Name = user.Name;
            model.Email = user.Email ?? "";
            model.Departments = departments.Select(d => new DepartmentSelectViewModel
            {
                Id = d.Id,
                Name = d.Name
            }).ToList();

            return View(model);
        }

        var previousDepartmentId = user.DepartmentId;

        user.DepartmentId = model.DepartmentId;

        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            var departments = await _service.GetAllAsync();

            model.Name = user.Name;
            model.Email = user.Email ?? "";
            model.Departments = departments.Select(d => new DepartmentSelectViewModel
            {
                Id = d.Id,
                Name = d.Name
            }).ToList();

            return View(model);
        }

        if (model.DepartmentId.HasValue &&
           model.DepartmentId != previousDepartmentId &&
           department != null)
        {
            await NotifyDepartmentManagersAsync(department, user);
        }

        TempData["Success"] = model.DepartmentId.HasValue
            ? "Çalışanın departmanı güncellendi."
            : "Çalışan departmandan çıkarıldı.";

        return RedirectToAction(
            nameof(Details),
            new { id = model.ReturnDepartmentId });
    }

    private async Task NotifyDepartmentManagersAsync(DepartmentDto department, ApplicationUser assignedUser)
    {
        var managers = new List<ApplicationUser>();

        foreach (var candidate in _userManager.Users.Where(u => u.DepartmentId == department.Id))
        {
            if (await _userManager.IsInRoleAsync(candidate, Roles.DepartmentManager))
                managers.Add(candidate);
        }

        foreach (var manager in managers)
        {
            if (string.IsNullOrWhiteSpace(manager.Email))
                continue;

            await _emailSender.SendAsync(
                manager.Email,
                $"Departmanınıza yeni bir çalışan atandı: {assignedUser.Name}",
                $"<p><strong>{assignedUser.Name}</strong> ({assignedUser.Email}) adlı çalışan " +
                $"<strong>{department.Name}</strong> departmanına atandı.</p>");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMember(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        if (!await CanManageMemberAsync(user))
            return Forbid();

        var departmentId = user.DepartmentId;

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            TempData["Error"] = "Kullanıcı silinemedi.";
            return RedirectToAction(nameof(Details), new { id = departmentId });
        }

        TempData["Success"] = "Kullanıcı silindi.";

        return RedirectToAction(nameof(Details), new { id = departmentId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Departman adı boş olamaz.";
            return RedirectToAction(nameof(Index));
        }

        name = name.Trim();
        var created = await _service.CreateAsync(name);

        if (!created)
        {
            TempData["Error"] = "Bu departman zaten var.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Departman oluşturuldu.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Director + "," + Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _service.DeleteAsync(id))
            return NotFound();

        TempData["Success"] = "Departman silindi.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CanManageMemberAsync(ApplicationUser member)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return false;

        //kullanıcı kendisini yönetemez
        if (currentUser.Id == member.Id)
            return false;

        var currentUserIsDirector =
            await _userManager.IsInRoleAsync(currentUser, Roles.Director);

        var currentUserIsDepartmentManager =
            await _userManager.IsInRoleAsync(currentUser, Roles.DepartmentManager);

        var memberIsDirector =
            await _userManager.IsInRoleAsync(member, Roles.Director);

        var memberIsDepartmentManager =
            await _userManager.IsInRoleAsync(member, Roles.DepartmentManager);

        //müdür başka bir müdürü veya departman yöneticisini yönetemez
        if (currentUserIsDirector)
        {
            if (memberIsDirector || memberIsDepartmentManager)
                return false;

            return true;
        }

        //departman yöneticisi sadece kendi departmanındaki normal çalışanları yönetebilir
        if (currentUserIsDepartmentManager)
        {
            if (memberIsDirector || memberIsDepartmentManager)
                return false;

            if (!currentUser.DepartmentId.HasValue ||
                !member.DepartmentId.HasValue)
                return false;

            return currentUser.DepartmentId == member.DepartmentId;
        }

        return false;
    }

    private async Task<bool> CanManageEmployeeAsync(ApplicationUser member)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return false;

        //müdür yönetici veya admin olan kullanıcılar normal çalışan olarak düzenlenemez
        if (await _userManager.IsInRoleAsync(member, Roles.Director) ||
            await _userManager.IsInRoleAsync(member, Roles.DepartmentManager) ||
            await _userManager.IsInRoleAsync(member, Roles.Admin))
        {
            return false;
        }

        //müdür tüm departmanlardaki çalışanların departmanını değiştirebilir
        if (await _userManager.IsInRoleAsync(currentUser, Roles.Director))
            return true;

        //departman yöneticisi sadece kendi departmanındaki normal çalışanları yönetebilir
        if (await _userManager.IsInRoleAsync(currentUser, Roles.DepartmentManager))
        {
            return currentUser.DepartmentId.HasValue &&
                   member.DepartmentId.HasValue &&
                   currentUser.DepartmentId == member.DepartmentId;
        }

        return false;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromDepartment(
string id,
int departmentId)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        if (!await CanManageEmployeeAsync(user))
            return Forbid();

        if (user.DepartmentId != departmentId)
        {
            TempData["Error"] = "Bu kullanıcının departmanı değişmiş görünüyor. Liste güncellendi.";
            return RedirectToAction(nameof(Details), new { id = departmentId });
        }

        user.DepartmentId = null;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            TempData["Error"] = "Çalışan departmandan çıkarılamadı.";
            return RedirectToAction(nameof(Details), new { id = departmentId });
        }

        TempData["Success"] = "Çalışan departmandan çıkarıldı.";

        return RedirectToAction(nameof(Details), new { id = departmentId });
    }
}
