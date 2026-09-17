using TaskTrackerSystem.Application.DTOs;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Domain.Entities;

namespace TaskTrackerSystem.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repo;

    public DepartmentService(IDepartmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        var departments = await _repo.GetAllAsync();

        var result = new List<DepartmentDto>();

        foreach (var d in departments)
        {
            result.Add(new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                EmployeeCount = await _repo.CountUsersAsync(d.Id)
            });
        }

        return result;
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var d = await _repo.GetByIdAsync(id);

        if (d == null)
            return null;

        return new DepartmentDto
        {
            Id = d.Id,
            Name = d.Name,
            EmployeeCount = await _repo.CountUsersAsync(d.Id)
        };
    }

    public async Task<bool> CreateAsync(string name)
    {
        name = name.Trim();

        if (await _repo.ExistsByNameAsync(name))
            return false;

        await _repo.AddAsync(new Department { Name = name });
        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var d = await _repo.GetByIdAsync(id);

        if (d == null)
            return false;

        _repo.Remove(d);
        await _repo.SaveChangesAsync();

        return true;
    }
}