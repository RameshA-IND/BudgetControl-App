using Microsoft.EntityFrameworkCore;
using BudgetControl.Application.DTOs.Budget;
using BudgetControl.Application.Interfaces;
using BudgetControl.Domain.Entities;
using BudgetControl.Domain.Enums;
using BudgetControl.Infrastructure.Data;

namespace BudgetControl.Infrastructure.Services;

public class BudgetService : IBudgetService
{
    private readonly AppDbContext _context;

    public BudgetService(AppDbContext context) => _context = context;

    public async Task<BudgetResponseDto> CreateAsync(CreateBudgetDto dto, int createdById)
    {
        if (await _context.Budgets.AnyAsync(b => b.DepartmentId == dto.DepartmentId && b.FiscalYear == dto.FiscalYear))
            throw new InvalidOperationException($"A budget already exists for this department in fiscal year {dto.FiscalYear}.");

        var budget = new Budget
        {
            DepartmentId = dto.DepartmentId,
            FiscalYear = dto.FiscalYear,
            PeriodStart = dto.PeriodStart,
            PeriodEnd = dto.PeriodEnd,
            AllocatedAmount = dto.AllocatedAmount,
            WarningThresholdPct = dto.WarningThresholdPct,
            CriticalThresholdPct = dto.CriticalThresholdPct,
            CreatedById = createdById,
            Status = BudgetStatus.Active
        };

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(budget.Id) ?? throw new Exception("Failed to create budget.");
    }

    public async Task<BudgetResponseDto> UpdateAsync(int id, UpdateBudgetDto dto)
    {
        var budget = await _context.Budgets.FindAsync(id)
            ?? throw new KeyNotFoundException("Budget not found.");

        if (dto.AllocatedAmount.HasValue) budget.AllocatedAmount = dto.AllocatedAmount.Value;
        if (dto.WarningThresholdPct.HasValue) budget.WarningThresholdPct = dto.WarningThresholdPct.Value;
        if (dto.CriticalThresholdPct.HasValue) budget.CriticalThresholdPct = dto.CriticalThresholdPct.Value;
        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<BudgetStatus>(dto.Status, out var status))
            budget.Status = status;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id) ?? throw new Exception("Failed to update budget.");
    }

    public async Task<BudgetResponseDto?> GetByIdAsync(int id)
    {
        var budget = await _context.Budgets
            .Include(b => b.Department).ThenInclude(d => d.Manager)
            .FirstOrDefaultAsync(b => b.Id == id);

        return budget == null ? null : MapToDto(budget);
    }

    public async Task<IEnumerable<BudgetResponseDto>> GetAllAsync()
    {
        var budgets = await _context.Budgets
            .Include(b => b.Department).ThenInclude(d => d.Manager)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return budgets.Select(MapToDto);
    }

    public async Task<IEnumerable<BudgetResponseDto>> GetByDepartmentAsync(int departmentId)
    {
        var budgets = await _context.Budgets
            .Include(b => b.Department).ThenInclude(d => d.Manager)
            .Where(b => b.DepartmentId == departmentId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return budgets.Select(MapToDto);
    }

    private static BudgetResponseDto MapToDto(Budget budget)
    {
        var utilization = budget.UtilizationPercent;
        string health = "Healthy";
        if (utilization >= budget.CriticalThresholdPct) health = "Critical";
        else if (utilization >= budget.WarningThresholdPct) health = "Warning";

        return new BudgetResponseDto
        {
            Id = budget.Id,
            DepartmentId = budget.DepartmentId,
            DepartmentName = budget.Department?.Name ?? "",
            DepartmentCode = budget.Department?.Code ?? "",
            ManagerName = budget.Department?.Manager?.FullName,
            FiscalYear = budget.FiscalYear,
            PeriodStart = budget.PeriodStart,
            PeriodEnd = budget.PeriodEnd,
            AllocatedAmount = budget.AllocatedAmount,
            SpentAmount = budget.SpentAmount,
            RemainingAmount = budget.RemainingAmount,
            UtilizationPercent = utilization,
            WarningThresholdPct = budget.WarningThresholdPct,
            CriticalThresholdPct = budget.CriticalThresholdPct,
            Status = budget.Status.ToString(),
            HealthStatus = health,
            CreatedAt = budget.CreatedAt
        };
    }
}
