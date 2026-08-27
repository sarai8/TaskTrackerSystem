using Microsoft.AspNetCore.Identity;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Infrastructure.Persistence;

namespace TaskTrackerSystem.Web.Services;

public class TaskReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TaskReminderBackgroundService> _logger;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ReminderWindow = TimeSpan.FromHours(2);

    public TaskReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<TaskReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendDueRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görev hatırlatma servisinde hata oluştu.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task SendDueRemindersAsync()
    {
        using var scope = _scopeFactory.CreateScope();

        var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var now = DateTime.Now;

        var dueTasks = await taskService.GetTasksDueForReminderAsync(
            now,
            now.Add(ReminderWindow));

        foreach (var task in dueTasks)
        {
            var user = await userManager.FindByIdAsync(task.UserId);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                continue;

            var subject = $"Hatırlatma: \"{task.Title}\" görevinin son tarihi yaklaşıyor";

            var body = $@"
                <p>Merhaba {user.Name},</p>
                <p><strong>{task.Title}</strong> adlı görevinizin son tarihi
                <strong>{task.DueDate:dd.MM.yyyy HH:mm}</strong> ve
                yaklaşık 2 saat içinde doluyor.</p>
                <p>Görevi zamanında tamamlamayı unutmayın.</p>";

            await emailSender.SendAsync(user.Email, subject, body);
            await taskService.MarkReminderSentAsync(task.Id);
        }
    }
}