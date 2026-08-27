using System.ComponentModel.DataAnnotations;
using TaskTrackerSystem.Application.DTOs;
using TaskTrackerSystem.Domain.Enums;
using DomainTaskStatus = TaskTrackerSystem.Domain.Enums.TaskStatus;


namespace TaskTrackerSystem.Web.ViewModels;

public class TaskFormViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = "";

    [MaxLength(1000)]
    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [Required]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    [Display(Name = "Son Tarih ve Saat")]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(1).AddHours(17);

    public DomainTaskStatus Status { get; set; } = DomainTaskStatus.Pending;

 
}


public class TasksIndexViewModel
{
    public List<TaskDto> Pending { get; set; } = new();

    public List<TaskDto> InProgress { get; set; } = new();

    public List<TaskDto> Completed { get; set; } = new();

    public string? PriorityFilter { get; set; }

    public string? SortOption { get; set; }
}