using BudgetControl.Application.DTOs.Expense;

namespace BudgetControl.Application.Interfaces;

public interface IExpenseService
{
    Task<ExpenseResponseDto> SubmitAsync(SubmitExpenseDto dto, int submittedById);
    Task<ExpenseResponseDto> UpdateStatusAsync(int expenseId, UpdateExpenseStatusDto dto, int approverId, string approverRole);
    Task<ExpenseResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<ExpenseResponseDto>> GetAllAsync(string? status = null, string? category = null, int? departmentId = null);
    Task<IEnumerable<ExpenseResponseDto>> GetByUserAsync(int userId);
    Task<IEnumerable<ExpenseResponseDto>> GetPendingApprovalsAsync(string role, int? departmentId = null);
}
