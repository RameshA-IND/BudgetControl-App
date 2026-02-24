using Microsoft.EntityFrameworkCore;
using BudgetControl.Application.DTOs.Dashboard;
using BudgetControl.Application.Interfaces;
using BudgetControl.Domain.Enums;
using BudgetControl.Infrastructure.Data;

namespace BudgetControl.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context) => _context = context;

    public async Task<DashboardSummaryDto> GetSummaryAsync(string role)
    {
        var budgets = await _context.Budgets
            .Where(b => b.Status == BudgetStatus.Active)
            .ToListAsync();

        var totalBudget = budgets.Sum(b => b.AllocatedAmount);
        
        var totalSpent = await _context.Expenses
            .Where(e => e.Status == ExpenseStatus.Approved)
            .SumAsync(e => e.Amount);

        return new DashboardSummaryDto
        {
            TotalBudget = totalBudget,
            TotalSpent = totalSpent,
            TotalRemaining = totalBudget - totalSpent,
            ActiveAlerts = await _context.Alerts.CountAsync(a => !a.IsRead),
            PendingApprovals = await _context.Expenses.CountAsync(e =>
                (role == UserRole.DepartmentManager.ToString() && e.Status == ExpenseStatus.Pending) ||
                (role == UserRole.FinanceAdmin.ToString() && (e.Status == ExpenseStatus.Pending || e.Status == ExpenseStatus.DepartmentApproved))
            ),
            TotalDepartments = await _context.Departments.CountAsync(d => d.IsActive),
            OverallUtilization = totalBudget > 0 ? Math.Round((totalSpent / totalBudget) * 100, 2) : 0
        };
    }

    public async Task<IEnumerable<DepartmentSpendingDto>> GetDepartmentSpendingAsync()
    {
        var departments = await _context.Departments
            .Where(d => d.IsActive)
            .ToListAsync();

        var budgets = await _context.Budgets
            .Where(b => b.Status == BudgetStatus.Active)
            .ToListAsync();

        var expenses = await _context.Expenses
            .Where(e => e.Status == ExpenseStatus.Approved)
            .ToListAsync();

        return departments.Select(d =>
        {
            var b = budgets.FirstOrDefault(budget => budget.DepartmentId == d.Id);
            var departmentSpent = expenses.Where(e => e.DepartmentId == d.Id).Sum(e => e.Amount);

            if (b == null)
            {
                return new DepartmentSpendingDto
                {
                    DepartmentId = d.Id,
                    DepartmentName = d.Name,
                    AllocatedAmount = 0,
                    SpentAmount = departmentSpent,
                    RemainingAmount = -departmentSpent,
                    UtilizationPercent = departmentSpent > 0 ? 100M : 0M,
                    HealthStatus = departmentSpent > 0 ? "Critical" : "Healthy"
                };
            }

            var utilization = b.AllocatedAmount > 0 
                ? Math.Round((departmentSpent / b.AllocatedAmount) * 100, 2) 
                : 0;
            
            string health = "Healthy";
            if (utilization >= b.CriticalThresholdPct) health = "Critical";
            else if (utilization >= b.WarningThresholdPct) health = "Warning";

            return new DepartmentSpendingDto
            {
                DepartmentId = d.Id,
                DepartmentName = d.Name,
                AllocatedAmount = b.AllocatedAmount,
                SpentAmount = departmentSpent,
                RemainingAmount = b.AllocatedAmount - departmentSpent,
                UtilizationPercent = utilization,
                HealthStatus = health
            };
        }).OrderByDescending(d => d.UtilizationPercent);
    }

    public async Task<IEnumerable<CategorySpendingDto>> GetCategorySpendingAsync()
    {
        var expenses = await _context.Expenses
            .Where(e => e.Status == ExpenseStatus.Approved)
            .ToListAsync();

        var totalAmount = expenses.Sum(e => e.Amount);

        return expenses
            .GroupBy(e => e.Category)
            .Select(g => new CategorySpendingDto
            {
                Category = g.Key,
                TotalAmount = g.Sum(e => e.Amount),
                Percentage = totalAmount > 0 ? Math.Round((g.Sum(e => e.Amount) / totalAmount) * 100, 2) : 0,
                ExpenseCount = g.Count()
            })
            .OrderByDescending(c => c.TotalAmount);
    }

    public async Task<IEnumerable<MonthlyTrendDto>> GetMonthlyTrendsAsync()
    {
        var currentYear = DateTime.UtcNow.Year;

        var expenses = await _context.Expenses
            .Where(e => e.Status == ExpenseStatus.Approved && e.SubmittedAt.Year == currentYear)
            .ToListAsync();

        var budgets = await _context.Budgets
            .Where(b => b.Status == BudgetStatus.Active)
            .ToListAsync();

        var monthlyBudget = budgets.Sum(b => b.AllocatedAmount);

        var months = Enumerable.Range(1, 12).Select(m =>
        {
            var monthName = new DateTime(currentYear, m, 1).ToString("MMM");
            var spent = expenses.Where(e => e.SubmittedAt.Month == m).Sum(e => e.Amount);

            return new MonthlyTrendDto
            {
                Month = monthName,
                BudgetAmount = monthlyBudget,
                SpentAmount = spent
            };
        });

        return months;
    }

    public async Task<IEnumerable<PendingApprovalDto>> GetPendingApprovalsAsync(string role)
    {
        var query = _context.Expenses
            .Include(e => e.SubmittedBy)
            .Include(e => e.Department)
            .AsQueryable();

        if (role == UserRole.DepartmentManager.ToString())
        {
            query = query.Where(e => e.Status == ExpenseStatus.Pending);
        }
        else if (role == UserRole.FinanceAdmin.ToString())
        {
            query = query.Where(e => e.Status == ExpenseStatus.Pending || e.Status == ExpenseStatus.DepartmentApproved);
        }
        else
        {
            query = query.Where(e => false); // None visible
        }

        var expenses = await query
            .OrderBy(e => e.SubmittedAt)
            .Take(10)
            .ToListAsync();

        return expenses.Select(e => new PendingApprovalDto
        {
            Id = e.Id,
            Title = e.Title,
            SubmittedBy = e.SubmittedBy?.FullName ?? "",
            Department = e.Department?.Name ?? "",
            Amount = e.Amount,
            Category = e.Category,
            SubmittedAt = e.SubmittedAt
        });
    }
}
