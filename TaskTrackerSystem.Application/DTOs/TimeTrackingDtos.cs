namespace TaskTrackerSystem.Application.DTOs;

public class DailyTimeDto
{
    public DateTime Date { get; set; }

    public double TotalMinutes { get; set; }
}

public class ActiveTaskDto
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public DateTime DueDate { get; set; }

    public bool IsRunning { get; set; }

    public int ElapsedMinutes { get; set; }
}