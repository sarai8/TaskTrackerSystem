using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Infrastructure.Email;
using TaskTrackerSystem.Infrastructure.Persistence;

namespace TaskTrackerSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITaskTimeLogRepository, TaskTimeLogRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDepartmentJoinRequestRepository, DepartmentJoinRequestRepository>();
        services.AddScoped<ITaskAssignmentRepository, TaskAssignmentRepository>();


        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}