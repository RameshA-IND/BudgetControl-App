using BudgetControl.Application.DTOs.Budget;

namespace BudgetControl.Application.Interfaces;

public interface IBudgetService
{
    Task<BudgetResponseDto> CreateAsync(CreateBudgetDto dto, int createdById);
    Task<BudgetResponseDto> UpdateAsync(int id, UpdateBudgetDto dto);
    Task<BudgetResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<BudgetResponseDto>> GetAllAsync();
    Task<IEnumerable<BudgetResponseDto>> GetByDepartmentAsync(int departmentId);
}
