using BudgetControl.Application.DTOs.Department;

namespace BudgetControl.Application.Interfaces;

public interface IDepartmentService
{
    Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto);
    Task<DepartmentResponseDto> UpdateAsync(int id, CreateDepartmentDto dto);
    Task<DepartmentResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<DepartmentResponseDto>> GetAllAsync();
}
