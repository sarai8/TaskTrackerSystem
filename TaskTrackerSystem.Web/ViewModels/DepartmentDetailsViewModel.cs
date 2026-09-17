namespace TaskTrackerSystem.Web.ViewModels;

public class DepartmentMemberViewModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";

    public string Email { get; set; } = "";
    public int? DepartmentId { get; set; }

    public int ReturnDepartmentId { get; set; }

    public List<DepartmentSelectViewModel> Departments { get; set; } = new();
    public string Position { get; set; } = "Çalışan";
}

public class DepartmentDetailsViewModel
{
    public string DepartmentName { get; set; } = "";

    public List<DepartmentMemberViewModel> Managers { get; set; } = new();

    public List<DepartmentMemberViewModel> Employees { get; set; } = new();

    public List<DepartmentMemberViewModel> UnassignedUsers { get; set; } = new();
}
