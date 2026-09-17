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

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<DepartmentJoinRequest> DepartmentJoinRequests => Set<DepartmentJoinRequest>();

    public DbSet<TaskAssignmentPost> TaskAssignmentPosts => Set<TaskAssignmentPost>();

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

        b.Entity<Department>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            e.HasIndex(x => x.Name)
                .IsUnique();
        });

        b.Entity<DepartmentJoinRequest>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.UserId)
                .IsRequired();

            e.HasIndex(x => new { x.UserId, x.Status });
        });

        b.Entity<TaskAssignmentPost>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Title)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(x => x.AssignedByUserId)
                .IsRequired();

            e.HasIndex(x => new { x.DepartmentId, x.Status });
        });

        b.Entity<ApplicationUser>(e =>
        {
            e.HasOne<Department>()
                .WithMany()
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}