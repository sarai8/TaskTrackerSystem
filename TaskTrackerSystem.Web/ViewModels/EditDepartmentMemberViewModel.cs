using System.ComponentModel.DataAnnotations;

namespace TaskTrackerSystem.Web.ViewModels;

public class EditDepartmentMemberViewModel
{
    public string Id { get; set; } = "";

    [Display(Name = "Ad Soyad")]
    public string Name { get; set; } = "";

    [Display(Name = "E-posta")]
    public string Email { get; set; } = "";

    [Display(Name = "Departman")]
    public int? DepartmentId { get; set; }

    public int ReturnDepartmentId { get; set; }

    public List<DepartmentSelectViewModel> Departments { get; set; } = new();
}

public class DepartmentSelectViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
}