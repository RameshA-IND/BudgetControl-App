using Microsoft.EntityFrameworkCore;
using BudgetControl.Application.DTOs.Expense;
using BudgetControl.Application.Interfaces;
using BudgetControl.Domain.Entities;
using BudgetControl.Domain.Enums;
using BudgetControl.Infrastructure.Data;

namespace BudgetControl.Infrastructure.Services;

public class ExpenseService : IExpenseService
{
    private readonly AppDbContext _context;
    private readonly IAlertService _alertService;

    public ExpenseService(AppDbContext context, IAlertService alertService)
    {
        _context = context;
        _alertService = alertService;
    }

    public async Task<ExpenseResponseDto> SubmitAsync(SubmitExpenseDto dto, int submittedById)
    {
        // Find active budget for the department
        var budget = await _context.Budgets
            .Where(b => b.DepartmentId == dto.DepartmentId && b.Status == BudgetStatus.Active)
            .OrderByDescending(b => b.PeriodEnd)
            .FirstOrDefaultAsync();

        var expense = new Expense
        {
            Title = dto.Title,
            Description = dto.Description,
            Amount = dto.Amount,
            Category = dto.Category,
            DepartmentId = dto.DepartmentId,
            BudgetId = budget?.Id,
            SubmittedById = submittedById,
            Status = ExpenseStatus.Pending,
            ReceiptUrl = dto.ReceiptUrl
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(expense.Id) ?? throw new Exception("Failed to submit expense.");
    }

    public async Task<ExpenseResponseDto> UpdateStatusAsync(int expenseId, UpdateExpenseStatusDto dto, int approverId, string approverRole)
    {
        var expense = await _context.Expenses
            .Include(e => e.Budget)
            .FirstOrDefaultAsync(e => e.Id == expenseId)
            ?? throw new KeyNotFoundException("Expense not found.");

        if (!Enum.TryParse<ApprovalAction>(dto.Action, out var action))
            throw new ArgumentException("Invalid action. Use 'Approved' or 'Rejected'.");

        if (!Enum.TryParse<UserRole>(approverRole, out var role))
            throw new ArgumentException("Invalid approver role.");

        // Guard: Prevent duplicate or conflicting actions
        if (expense.Status == ExpenseStatus.Approved || expense.Status == ExpenseStatus.Rejected)
            throw new InvalidOperationException($"This expense has already been {expense.Status.ToString().ToLower()}.");

        if (role == UserRole.DepartmentManager && expense.Status != ExpenseStatus.Pending)
            throw new InvalidOperationException("This expense is not in a state that can be approved by a Department Manager.");

        if (role == UserRole.FinanceAdmin && expense.Status != ExpenseStatus.DepartmentApproved && expense.Status != ExpenseStatus.Pending)
            throw new InvalidOperationException("This expense is not in a state that can be approved by a Finance Admin.");


        // Create approval record
        var approval = new ExpenseApproval
        {
            ExpenseId = expenseId,
            ApproverId = approverId,
            ApproverRole = role,
            Action = action,
            Comments = dto.Comments
        };
        _context.ExpenseApprovals.Add(approval);

        // Update expense status
        if (action == ApprovalAction.Rejected)
        {
            expense.Status = ExpenseStatus.Rejected;
        }
        else if (action == ApprovalAction.Approved)
        {
            if (role == UserRole.DepartmentManager)
            {
                expense.Status = ExpenseStatus.DepartmentApproved;
            }
            else if (role == UserRole.FinanceAdmin)
            {
                expense.Status = ExpenseStatus.Approved;

                // Update budget spent amount
                if (expense.Budget != null)
                {
                    expense.Budget.SpentAmount += expense.Amount;
                }
            }
        }

        await _context.SaveChangesAsync();

        // Check alerts after approval
        if (expense.BudgetId.HasValue && expense.Status == ExpenseStatus.Approved)
        {
            await _alertService.CheckAndCreateAlertsAsync(expense.BudgetId.Value);
        }

        return await GetByIdAsync(expenseId) ?? throw new Exception("Failed to update expense.");
    }

    public async Task<ExpenseResponseDto?> GetByIdAsync(int id)
    {
        var expense = await _context.Expenses
            .Include(e => e.Department)
            .Include(e => e.SubmittedBy)
            .Include(e => e.Approvals).ThenInclude(a => a.Approver)
            .FirstOrDefaultAsync(e => e.Id == id);

        return expense == null ? null : MapToDto(expense);
    }

    public async Task<IEnumerable<ExpenseResponseDto>> GetAllAsync(string? status = null, string? category = null, int? departmentId = null)
    {
        var query = _context.Expenses
            .Include(e => e.Department)
            .Include(e => e.SubmittedBy)
            .Include(e => e.Approvals).ThenInclude(a => a.Approver)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ExpenseStatus>(status, out var s))
            query = query.Where(e => e.Status == s);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(e => e.Category == category);

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        var expenses = await query.OrderByDescending(e => e.SubmittedAt).ToListAsync();
        return expenses.Select(MapToDto);
    }

    public async Task<IEnumerable<ExpenseResponseDto>> GetByUserAsync(int userId)
    {
        var expenses = await _context.Expenses
            .Include(e => e.Department)
            .Include(e => e.SubmittedBy)
            .Include(e => e.Approvals).ThenInclude(a => a.Approver)
            .Where(e => e.SubmittedById == userId)
            .OrderByDescending(e => e.SubmittedAt)
            .ToListAsync();

        return expenses.Select(MapToDto);
    }

    public async Task<IEnumerable<ExpenseResponseDto>> GetPendingApprovalsAsync(string role, int? departmentId = null)
    {
        var query = _context.Expenses
            .Include(e => e.Department)
            .Include(e => e.SubmittedBy)
            .Include(e => e.Approvals).ThenInclude(a => a.Approver)
            .AsQueryable();

        if (role == UserRole.DepartmentManager.ToString())
        {
            query = query.Where(e => e.Status == ExpenseStatus.Pending);
        }
        else if (role == UserRole.FinanceAdmin.ToString())
        {
            query = query.Where(e => e.Status == ExpenseStatus.DepartmentApproved || e.Status == ExpenseStatus.Pending);
        }
        else
        {
            query = query.Where(e => false); // no pending approvals for others
        }

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        var expenses = await query.OrderBy(e => e.SubmittedAt).ToListAsync();
        return expenses.Select(MapToDto);
    }

    private static ExpenseResponseDto MapToDto(Expense expense) => new()
    {
        Id = expense.Id,
        Title = expense.Title,
        Description = expense.Description,
        Amount = expense.Amount,
        Category = expense.Category,
        DepartmentId = expense.DepartmentId,
        DepartmentName = expense.Department?.Name ?? "",
        BudgetId = expense.BudgetId,
        SubmittedById = expense.SubmittedById,
        SubmittedByName = expense.SubmittedBy?.FullName ?? "",
        Status = expense.Status.ToString(),
        ReceiptUrl = expense.ReceiptUrl,
        SubmittedAt = expense.SubmittedAt,
        UpdatedAt = expense.UpdatedAt,
        Approvals = expense.Approvals.Select(a => new ApprovalRecordDto
        {
            Id = a.Id,
            ApproverName = a.Approver?.FullName ?? "",
            ApproverRole = a.ApproverRole.ToString(),
            Action = a.Action.ToString(),
            Comments = a.Comments,
            ActionDate = a.ActionDate
        }).ToList()
    };
}
