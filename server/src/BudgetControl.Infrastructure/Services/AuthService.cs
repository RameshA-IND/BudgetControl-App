using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BudgetControl.Application.DTOs.Auth;
using BudgetControl.Application.Interfaces;
using BudgetControl.Domain.Entities;
using BudgetControl.Domain.Enums;
using BudgetControl.Infrastructure.Data;

namespace BudgetControl.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = GenerateJwtToken(user);
        return new AuthResponseDto
        {
            Token = token,
            User = MapToProfile(user)
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Email already registered.");

        if (!Enum.TryParse<UserRole>(dto.Role, out var role))
            throw new ArgumentException("Invalid role specified.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role,
            DepartmentId = dto.DepartmentId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Reload with department
        user = await _context.Users
            .Include(u => u.Department)
            .FirstAsync(u => u.Id == user.Id);

        var token = GenerateJwtToken(user);
        return new AuthResponseDto
        {
            Token = token,
            User = MapToProfile(user)
        };
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        return MapToProfile(user);
    }

    private string GenerateJwtToken(User user)
    {
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? _configuration["JwtSettings:SecretKey"]!;
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["JwtSettings:Issuer"];
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["JwtSettings:Audience"];

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("departmentId", user.DepartmentId?.ToString() ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserProfileDto MapToProfile(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role.ToString(),
        DepartmentId = user.DepartmentId,
        DepartmentName = user.Department?.Name
    };
}
