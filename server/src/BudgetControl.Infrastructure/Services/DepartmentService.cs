using Microsoft.EntityFrameworkCore;
using BudgetControl.Application.DTOs.Department;
using BudgetControl.Application.Interfaces;
using BudgetControl.Domain.Entities;
using BudgetControl.Infrastructure.Data;

namespace BudgetControl.Infrastructure.Services;

public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _context;

    public DepartmentService(AppDbContext context) => _context = context;

    public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto)
    {
        var dept = new Department
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            ManagerId = dto.ManagerId
        };

        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(dept.Id) ?? throw new Exception("Failed to create department.");
    }

    public async Task<DepartmentResponseDto> UpdateAsync(int id, CreateDepartmentDto dto)
    {
        var dept = await _context.Departments.FindAsync(id)
            ?? throw new KeyNotFoundException("Department not found.");

        dept.Name = dto.Name;
        dept.Code = dto.Code;
        dept.Description = dto.Description;
        dept.ManagerId = dto.ManagerId;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id) ?? throw new Exception("Failed to update department.");
    }

    public async Task<DepartmentResponseDto?> GetByIdAsync(int id)
    {
        var dept = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.Users)
            .FirstOrDefaultAsync(d => d.Id == id);

        return dept == null ? null : MapToDto(dept);
    }

    public async Task<IEnumerable<DepartmentResponseDto>> GetAllAsync()
    {
        var departments = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.Users)
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();

        return departments.Select(MapToDto);
    }

    private static DepartmentResponseDto MapToDto(Department dept) => new()
    {
        Id = dept.Id,
        Name = dept.Name,
        Code = dept.Code,
        Description = dept.Description,
        ManagerId = dept.ManagerId,
        ManagerName = dept.Manager?.FullName,
        IsActive = dept.IsActive,
        EmployeeCount = dept.Users.Count,
        CreatedAt = dept.CreatedAt
    };
}
