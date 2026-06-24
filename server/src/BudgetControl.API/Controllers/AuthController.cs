using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BudgetControl.Application.DTOs.Auth;
using BudgetControl.Application.Interfaces;

namespace BudgetControl.API.Controllers;

/// <summary>
/// Controller handling user authentication and registration operations.
/// </summary>
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="dto">The registration details (email, password, etc.).</param>
    /// <returns>Authentication response containing JWT token and user info.</returns>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates an existing user and issues a JWT token.
    /// </summary>
    /// <param name="dto">The login credentials (email and password).</param>
    /// <returns>Authentication response containing JWT token and user info.</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the profile details of the currently authenticated user.
    /// </summary>
    /// <returns>The profile information of the user.</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = GetUserId();
        var profile = await _authService.GetProfileAsync(userId);
        return Ok(profile);
    }
}
