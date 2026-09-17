using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Web.ViewModels;

public class TaskApprovalViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public TaskPriority Priority { get; set; }

    public DateTime DueDate { get; set; }

    public string ClaimedByName { get; set; } = "";

    public string ClaimedByEmail { get; set; } = "";
}