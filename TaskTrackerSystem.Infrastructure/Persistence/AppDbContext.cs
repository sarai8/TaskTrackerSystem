using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskTrackerSystem.Domain.Entities;

namespace TaskTrackerSystem.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public DbSet<TaskTimeLog> TaskTimeLogs => Set<TaskTimeLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<TaskItem>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Title)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(x => x.Description)
                .HasMaxLength(1000);

            e.Property(x => x.UserId)
                .IsRequired();

            e.HasIndex(x => new
            {
                x.UserId,
                x.DueDate
            });
        });

        b.Entity<TaskTimeLog>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.UserId)
                .IsRequired();

            e.HasIndex(x => new
            {
                x.UserId,
                x.StartedAt
            });
        });


    }
}