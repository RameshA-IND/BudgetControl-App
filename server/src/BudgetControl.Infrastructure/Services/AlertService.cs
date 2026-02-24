using Microsoft.EntityFrameworkCore;
using BudgetControl.Application.DTOs.Alert;
using BudgetControl.Application.Interfaces;
using BudgetControl.Domain.Entities;
using BudgetControl.Domain.Enums;
using BudgetControl.Infrastructure.Data;

namespace BudgetControl.Infrastructure.Services;

public class AlertService : IAlertService
{
    private readonly AppDbContext _context;

    public AlertService(AppDbContext context) => _context = context;

    /// <summary>
    /// Threshold alert calculation logic:
    /// 1. Fetch the budget
    /// 2. Calculate utilization% = (SpentAmount / AllocatedAmount) × 100
    /// 3. If utilization% >= CriticalThreshold AND no existing unread Critical alert → Create Critical Alert
    /// 4. Else If utilization% >= WarningThreshold AND no existing unread Warning alert → Create Warning Alert
    /// </summary>
    public async Task CheckAndCreateAlertsAsync(int budgetId)
    {
        var budget = await _context.Budgets
            .Include(b => b.Department)
            .FirstOrDefaultAsync(b => b.Id == budgetId);

        if (budget == null) return;

        var utilization = budget.UtilizationPercent;

        // Check Critical threshold
        if (utilization >= budget.CriticalThresholdPct)
        {
            var existingCritical = await _context.Alerts
                .AnyAsync(a => a.BudgetId == budgetId &&
                              a.Severity == AlertSeverity.Critical &&
                              !a.IsRead);

            if (!existingCritical)
            {
                _context.Alerts.Add(new Alert
                {
                    BudgetId = budgetId,
                    DepartmentId = budget.DepartmentId,
                    Severity = AlertSeverity.Critical,
                    Message = $"Budget utilization has exceeded {budget.CriticalThresholdPct}% threshold. Current: {utilization}%",
                    UtilizationPercent = utilization
                });
                await _context.SaveChangesAsync();
            }
        }
        // Check Warning threshold
        else if (utilization >= budget.WarningThresholdPct)
        {
            var existingWarning = await _context.Alerts
                .AnyAsync(a => a.BudgetId == budgetId &&
                              a.Severity == AlertSeverity.Warning &&
                              !a.IsRead);

            if (!existingWarning)
            {
                _context.Alerts.Add(new Alert
                {
                    BudgetId = budgetId,
                    DepartmentId = budget.DepartmentId,
                    Severity = AlertSeverity.Warning,
                    Message = $"Budget utilization has crossed {budget.WarningThresholdPct}% warning threshold. Current: {utilization}%",
                    UtilizationPercent = utilization
                });
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task<IEnumerable<AlertResponseDto>> GetAllAsync()
    {
        var alerts = await _context.Alerts
            .Include(a => a.Department)
            .Include(a => a.Budget)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return alerts.Select(MapToDto);
    }

    public async Task<IEnumerable<AlertResponseDto>> GetUnreadAsync()
    {
        var alerts = await _context.Alerts
            .Include(a => a.Department)
            .Include(a => a.Budget)
            .Where(a => !a.IsRead)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return alerts.Select(MapToDto);
    }

    public async Task MarkAsReadAsync(int alertId)
    {
        var alert = await _context.Alerts.FindAsync(alertId)
            ?? throw new KeyNotFoundException("Alert not found.");

        alert.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync()
        => await _context.Alerts.CountAsync(a => !a.IsRead);

    private static AlertResponseDto MapToDto(Alert alert) => new()
    {
        Id = alert.Id,
        BudgetId = alert.BudgetId,
        DepartmentId = alert.DepartmentId,
        DepartmentName = alert.Department?.Name ?? "",
        Severity = alert.Severity.ToString(),
        Message = alert.Message,
        UtilizationPercent = alert.UtilizationPercent,
        AllocatedAmount = alert.Budget?.AllocatedAmount ?? 0,
        SpentAmount = alert.Budget?.SpentAmount ?? 0,
        IsRead = alert.IsRead,
        CreatedAt = alert.CreatedAt
    };
}
