using System.ComponentModel.DataAnnotations;
using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Web.ViewModels;

public class TaskAssignmentCreateViewModel
{
    [Required]
    [MaxLength(100)]
    [Display(Name = "Görev Başlığı")]
    public string Title { get; set; } = "";

    [MaxLength(1000)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Öncelik")]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [Required]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    [Display(Name = "Son Tarih ve Saat")]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(1).AddHours(17);

    [Required]
    [Display(Name = "Atama Şekli")]
    public string AssignMode { get; set; } = "department";

    public int? DepartmentId { get; set; }

    public string? UserId { get; set; }
}