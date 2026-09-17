using TaskTrackerSystem.Domain.Entities;

namespace TaskTrackerSystem.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();

    Task<Department?> GetByIdAsync(int id);

    Task<int> CountUsersAsync(int departmentId);

    Task AddAsync(Department department);

    void Remove(Department department);

    Task SaveChangesAsync();

    Task<bool> ExistsByNameAsync(string name);


}

