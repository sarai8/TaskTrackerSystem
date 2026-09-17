namespace TaskTrackerSystem.Web.ViewModels;

public class DepartmentRequestViewModel
{
    public int Id { get; set; }

    public string UserName { get; set; } = "";

    public string UserEmail { get; set; } = "";

    public string DepartmentName { get; set; } = "";

    public DateTime RequestedAt { get; set; }
}