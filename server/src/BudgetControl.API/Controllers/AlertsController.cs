using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BudgetControl.Application.DTOs.Alert;
using BudgetControl.Application.Interfaces;

namespace BudgetControl.API.Controllers;

/// <summary>
/// Controller for managing system alerts and notifications for users.
/// </summary>
[Authorize]
public class AlertsController : BaseController
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService) => _alertService = alertService;

    /// <summary>
    /// Retrieves all alerts generated for the authenticated user.
    /// </summary>
    /// <returns>A list of all alerts.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AlertResponseDto>>> GetAll()
    {
        var alerts = await _alertService.GetAllAsync();
        return Ok(alerts);
    }

    /// <summary>
    /// Retrieves only the unread alerts for the authenticated user.
    /// </summary>
    /// <returns>A list of unread alerts.</returns>
    [HttpGet("unread")]
    public async Task<ActionResult<IEnumerable<AlertResponseDto>>> GetUnread()
    {
        var alerts = await _alertService.GetUnreadAsync();
        return Ok(alerts);
    }

    /// <summary>
    /// Gets the total count of unread alerts for the authenticated user.
    /// </summary>
    /// <returns>The number of unread alerts.</returns>
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var count = await _alertService.GetUnreadCountAsync();
        return Ok(count);
    }

    /// <summary>
    /// Marks a specific alert as read.
    /// </summary>
    /// <param name="id">The unique identifier of the alert to mark as read.</param>
    /// <returns>NoContent result indicating success.</returns>
    [HttpPut("{id}/read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        await _alertService.MarkAsReadAsync(id);
        return NoContent();
    }
}
