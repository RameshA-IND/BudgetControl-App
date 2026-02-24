using BudgetControl.Application.DTOs.Dashboard;

namespace BudgetControl.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(string role);
    Task<IEnumerable<DepartmentSpendingDto>> GetDepartmentSpendingAsync();
    Task<IEnumerable<CategorySpendingDto>> GetCategorySpendingAsync();
    Task<IEnumerable<MonthlyTrendDto>> GetMonthlyTrendsAsync();
    Task<IEnumerable<PendingApprovalDto>> GetPendingApprovalsAsync(string role);
}
