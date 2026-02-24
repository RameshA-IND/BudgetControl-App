using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BudgetControl.Application.DTOs.Expense;
using BudgetControl.Application.Interfaces;

namespace BudgetControl.API.Controllers;

[Authorize]
public class ExpensesController : BaseController
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService) => _expenseService = expenseService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseResponseDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] string? category, [FromQuery] int? departmentId)
    {
        var expenses = await _expenseService.GetAllAsync(status, category, departmentId);
        return Ok(expenses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseResponseDto>> GetById(int id)
    {
        var expense = await _expenseService.GetByIdAsync(id);
        if (expense == null) return NotFound();
        return Ok(expense);
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<ExpenseResponseDto>>> GetMyExpenses()
    {
        var userId = GetUserId();
        var expenses = await _expenseService.GetByUserAsync(userId);
        return Ok(expenses);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "DepartmentManager,FinanceAdmin")]
    public async Task<ActionResult<IEnumerable<ExpenseResponseDto>>> GetPending([FromQuery] int? departmentId)
    {
        var role = GetUserRole();
        var expenses = await _expenseService.GetPendingApprovalsAsync(role, departmentId);
        return Ok(expenses);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponseDto>> Submit([FromBody] SubmitExpenseDto dto)
    {
        var userId = GetUserId();
        var expense = await _expenseService.SubmitAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, expense);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "DepartmentManager,FinanceAdmin")]
    public async Task<ActionResult<ExpenseResponseDto>> UpdateStatus(int id, [FromBody] UpdateExpenseStatusDto dto)
    {
        var approverId = GetUserId();
        var approverRole = GetUserRole();
        var expense = await _expenseService.UpdateStatusAsync(id, dto, approverId, approverRole);
        return Ok(expense);
    }
}
