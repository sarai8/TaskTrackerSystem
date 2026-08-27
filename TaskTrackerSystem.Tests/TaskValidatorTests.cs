using Xunit;
using TaskTrackerSystem.Application.DTOs;
using TaskTrackerSystem.Application.Validators;
using TaskTrackerSystem.Domain.Enums;

namespace TaskTrackerSystem.Tests;

public class TaskValidatorTests
{
    [Fact]
    public void CreateTask_TitleRequired()
    {
        var v = new CreateTaskValidator();

        var r = v.Validate(
            new CreateTaskDto
            {
                Title = "",
                DueDate = DateTime.Today
            });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void CreateTask_ValidModel()
    {
        var v = new CreateTaskValidator();

        var r = v.Validate(
            new CreateTaskDto
            {
                Title = "Test görevi",
                Description = "Açıklama",
                Priority = TaskPriority.High,
                DueDate = DateTime.Today
            });

        Assert.True(r.IsValid);
    }
}