using DomainTaskStatus = TaskTrackerSystem.Domain.Enums.TaskStatus;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskTrackerSystem.Application.DTOs;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Domain.Enums;
using TaskTrackerSystem.Web.ViewModels;

namespace TaskTrackerSystem.Web.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly ITaskService _service;
    private readonly IValidator<CreateTaskDto> _create;
    private readonly IValidator<UpdateTaskDto> _update;

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public TasksController(
        ITaskService s,
        IValidator<CreateTaskDto> c,
        IValidator<UpdateTaskDto> u)
    {
        _service = s;
        _create = c;
        _update = u;
    }

    public async Task<IActionResult> Index(string? priority, string? sort)
    {
        var tasks = await _service.GetTasksAsync(UserId);

        IEnumerable<TaskDto> filtered = tasks;

        filtered = priority switch
        {
            "high" => filtered.Where(t => t.Priority == TaskPriority.High),
            "medium" => filtered.Where(t => t.Priority == TaskPriority.Medium),
            "low" => filtered.Where(t => t.Priority == TaskPriority.Low),
            _ => filtered
        };

        filtered = sort switch
        {
            "duedate_desc" => filtered.OrderByDescending(t => t.DueDate),
            "priority_desc" => filtered.OrderByDescending(t => t.Priority),
            _ => filtered.OrderBy(t => t.DueDate)
        };

        var list = filtered.ToList();

        var vm = new TasksIndexViewModel
        {
            Pending = list.Where(t => t.Status == DomainTaskStatus.Pending).ToList(),
            InProgress = list.Where(t => t.Status == DomainTaskStatus.InProgress).ToList(),
            Completed = list.Where(t => t.Status == DomainTaskStatus.Completed && !t.IsArchived).ToList(),
            PriorityFilter = priority,
            SortOption = sort
        };

        return View(vm);
    }
    public async Task<IActionResult> Details(int id)
    {
        var x = await _service.GetTaskAsync(id, UserId);

        return x == null
            ? NotFound()
            : View(x);
    }

    [HttpGet]
    public IActionResult Create()
        => View(new TaskFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskFormViewModel m)
    {
        var dto = new CreateTaskDto
        {
            Title = m.Title,
            Description = m.Description,
            Priority = m.Priority,
            DueDate = m.DueDate
        };

        var r = await _create.ValidateAsync(dto);

        foreach (var e in r.Errors)
            ModelState.AddModelError(
                e.PropertyName,
                e.ErrorMessage);

        if (!ModelState.IsValid)
            return View(m);

        await _service.CreateAsync(dto, UserId);

        TempData["Success"] = "Görev oluşturuldu.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var x = await _service.GetTaskAsync(id, UserId);

        if (x == null)
            return NotFound();

        return View(
            new TaskFormViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Priority = x.Priority,
                DueDate = x.DueDate,
                Status = x.Status
            });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TaskFormViewModel m)
    {
        var dto = new UpdateTaskDto
        {
            Id = m.Id,
            Title = m.Title,
            Description = m.Description,
            Priority = m.Priority,
            DueDate = m.DueDate,
            Status = m.Status
        };

        var r = await _update.ValidateAsync(dto);

        foreach (var e in r.Errors)
            ModelState.AddModelError(
                e.PropertyName,
                e.ErrorMessage);

        if (!ModelState.IsValid)
            return View(m);

        if (!await _service.UpdateAsync(dto, UserId))
            return NotFound();

        TempData["Success"] = "Görev güncellendi.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _service.DeleteAsync(id, UserId))
            return NotFound();

        TempData["Success"] = "Görev silindi.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id, string? returnUrl = null)
    {
        if (!await _service.CompleteAsync(id, UserId))
            return NotFound();

        TempData["Success"] = "Görev tamamlandı.";

        return string.IsNullOrEmpty(returnUrl)
            ? RedirectToAction(nameof(Index))
            : Redirect(returnUrl);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int id, string? returnUrl = null)
    {
        if (!await _service.StartAsync(id, UserId))
            return NotFound();

        TempData["Success"] = "Göreve başlandı.";

        return string.IsNullOrEmpty(returnUrl)
            ? RedirectToAction(nameof(Index))
            : Redirect(returnUrl);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id)
    {
        if (!await _service.ArchiveAsync(id, UserId))
            return NotFound();

        TempData["Success"] = "Görev arşivlendi.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Archived()
    {
        var tasks = await _service.GetTasksAsync(UserId);

        var archived = tasks
            .Where(t => t.IsArchived)
            .OrderByDescending(t => t.DueDate)
            .ToList();

        return View(archived);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unarchive(int id)
    {
        if (!await _service.UnarchiveAsync(id, UserId))
            return NotFound();

        TempData["Success"] = "Görev arşivden çıkarıldı.";

        return RedirectToAction(nameof(Archived));
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Pause(int id, string? returnUrl = null)
    {
        if (!await _service.PauseAsync(id, UserId))
            return NotFound();

        TempData["Success"] = "Görev duraklatıldı.";

        return string.IsNullOrEmpty(returnUrl)
            ? RedirectToAction(nameof(Index))
            : Redirect(returnUrl);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Resume(int id, string? returnUrl = null)
    {
        if (!await _service.ResumeAsync(id, UserId))
            return NotFound();

        TempData["Success"] = "Göreve devam ediliyor.";

        return string.IsNullOrEmpty(returnUrl)
            ? RedirectToAction(nameof(Index))
            : Redirect(returnUrl);
    }

    [HttpGet]
    public IActionResult Calendar()
    => View();

    [HttpGet]
    public async Task<IActionResult> CalendarEvents()
    {
        var tasks = await _service.GetTasksAsync(UserId);

        var events = tasks.Select(t => new
        {
            id = t.Id,
            title = t.Title,
            start = t.DueDate.ToString("yyyy-MM-dd"),
            color = t.Status.ToString() == "Completed" ? "#198754" : "#0d6efd",
            url = Url.Action("Details", "Tasks", new { id = t.Id })
        });

        return Json(events);
    }
}