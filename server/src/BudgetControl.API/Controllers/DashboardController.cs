using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BudgetControl.Application.DTOs.Dashboard;
using BudgetControl.Application.Interfaces;

namespace BudgetControl.API.Controllers;

[Authorize]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var role = GetUserRole();
        var summary = await _dashboardService.GetSummaryAsync(role);
        return Ok(summary);
    }

    [HttpGet("department-spending")]
    public async Task<ActionResult<IEnumerable<DepartmentSpendingDto>>> GetDepartmentSpending()
    {
        var spending = await _dashboardService.GetDepartmentSpendingAsync();
        return Ok(spending);
    }

    [HttpGet("category-spending")]
    public async Task<ActionResult<IEnumerable<CategorySpendingDto>>> GetCategorySpending()
    {
        var spending = await _dashboardService.GetCategorySpendingAsync();
        return Ok(spending);
    }

    [HttpGet("monthly-trends")]
    public async Task<ActionResult<IEnumerable<MonthlyTrendDto>>> GetMonthlyTrends()
    {
        var trends = await _dashboardService.GetMonthlyTrendsAsync();
        return Ok(trends);
    }

    [HttpGet("pending-approvals")]
    [Authorize(Roles = "DepartmentManager,FinanceAdmin")]
    public async Task<ActionResult<IEnumerable<PendingApprovalDto>>> GetPendingApprovals()
    {
        var role = GetUserRole();
        var approvals = await _dashboardService.GetPendingApprovalsAsync(role);
        return Ok(approvals);
    }
}
