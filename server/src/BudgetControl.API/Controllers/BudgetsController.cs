using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BudgetControl.Application.DTOs.Budget;
using BudgetControl.Application.Interfaces;

namespace BudgetControl.API.Controllers;

[Authorize]
public class BudgetsController : BaseController
{
    private readonly IBudgetService _budgetService;

    public BudgetsController(IBudgetService budgetService) => _budgetService = budgetService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BudgetResponseDto>>> GetAll()
    {
        var budgets = await _budgetService.GetAllAsync();
        return Ok(budgets);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BudgetResponseDto>> GetById(int id)
    {
        var budget = await _budgetService.GetByIdAsync(id);
        if (budget == null) return NotFound();
        return Ok(budget);
    }

    [HttpGet("department/{departmentId}")]
    public async Task<ActionResult<IEnumerable<BudgetResponseDto>>> GetByDepartment(int departmentId)
    {
        var budgets = await _budgetService.GetByDepartmentAsync(departmentId);
        return Ok(budgets);
    }

    [HttpPost]
    [Authorize(Roles = "FinanceAdmin")]
    public async Task<ActionResult<BudgetResponseDto>> Create([FromBody] CreateBudgetDto dto)
    {
        var userId = GetUserId();
        var budget = await _budgetService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = budget.Id }, budget);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "FinanceAdmin")]
    public async Task<ActionResult<BudgetResponseDto>> Update(int id, [FromBody] UpdateBudgetDto dto)
    {
        var budget = await _budgetService.UpdateAsync(id, dto);
        return Ok(budget);
    }
}
