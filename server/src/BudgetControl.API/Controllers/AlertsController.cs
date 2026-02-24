using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BudgetControl.Application.DTOs.Alert;
using BudgetControl.Application.Interfaces;

namespace BudgetControl.API.Controllers;

[Authorize]
public class AlertsController : BaseController
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService) => _alertService = alertService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AlertResponseDto>>> GetAll()
    {
        var alerts = await _alertService.GetAllAsync();
        return Ok(alerts);
    }

    [HttpGet("unread")]
    public async Task<ActionResult<IEnumerable<AlertResponseDto>>> GetUnread()
    {
        var alerts = await _alertService.GetUnreadAsync();
        return Ok(alerts);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var count = await _alertService.GetUnreadCountAsync();
        return Ok(count);
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        await _alertService.MarkAsReadAsync(id);
        return NoContent();
    }
}
