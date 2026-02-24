using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BudgetControl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) throw new UnauthorizedAccessException("User not authenticated.");
        return int.Parse(claim.Value);
    }

    protected string GetUserRole()
    {
        var claim = User.FindFirst(ClaimTypes.Role);
        return claim?.Value ?? "Employee";
    }

    protected int? GetUserDepartmentId()
    {
        var claim = User.FindFirst("departmentId");
        if (claim == null || string.IsNullOrEmpty(claim.Value)) return null;
        return int.Parse(claim.Value);
    }
}
