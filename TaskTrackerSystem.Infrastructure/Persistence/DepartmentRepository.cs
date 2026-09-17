using Microsoft.EntityFrameworkCore;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Domain.Entities;

namespace TaskTrackerSystem.Infrastructure.Persistence;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _db;

    public DepartmentRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Department>> GetAllAsync()
        => _db.Departments.OrderBy(x => x.Name).ToListAsync();

    public Task<Department?> GetByIdAsync(int id)
        => _db.Departments.FirstOrDefaultAsync(x => x.Id == id);

    public Task<int> CountUsersAsync(int departmentId)
        => _db.Users.CountAsync(x => x.DepartmentId == departmentId);

    public Task<bool> ExistsByNameAsync(string name)
    => _db.Departments.AnyAsync(x => x.Name.ToLower() == name.ToLower());

    public async Task AddAsync(Department department)
        => await _db.Departments.AddAsync(department);

    public void Remove(Department department)
        => _db.Departments.Remove(department);

    public Task SaveChangesAsync()
        => _db.SaveChangesAsync();
}