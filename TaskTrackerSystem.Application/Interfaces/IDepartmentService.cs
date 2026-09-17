using TaskTrackerSystem.Application.DTOs;

namespace TaskTrackerSystem.Application.Interfaces;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();

    Task<bool> CreateAsync(string name);

    Task<bool> DeleteAsync(int id);

    Task<DepartmentDto?> GetByIdAsync(int id);

}
