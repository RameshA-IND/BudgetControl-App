using BudgetControl.Application.DTOs.Auth;

namespace BudgetControl.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<UserProfileDto> GetProfileAsync(int userId);
}
