using BudgetControl.Domain.Entities;
using BudgetControl.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BudgetControl.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Ensure database and all tables exist (handles fresh Render PostgreSQL deployment)
        await context.Database.EnsureCreatedAsync();

        // 1. Ensure Departments exist
        if (!await context.Departments.AnyAsync())
        {
            var dept = new Department
            {
                Name = "Finance",
                Code = "FIN",
                CreatedAt = DateTime.UtcNow
            };
            context.Departments.Add(dept);
            await context.SaveChangesAsync();
        }

        var financeDept = await context.Departments.FirstAsync(d => d.Code == "FIN");

        // 2. Ensure Admin User exists
        if (!await context.Users.AnyAsync(u => u.Email == "admin@budgetq.com"))
        {
            var admin = new User
            {
                FullName = "System Admin",
                Email = "admin@budgetq.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPassword123!"),
                Role = UserRole.FinanceAdmin,
                DepartmentId = financeDept.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(admin);
        }

        // 3. Ensure a test employee exists
        if (!await context.Users.AnyAsync(u => u.Email == "user@budgetq.com"))
        {
            var user = new User
            {
                FullName = "Test User",
                Email = "user@budgetq.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("UserPassword123!"),
                Role = UserRole.Employee,
                DepartmentId = financeDept.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
        }

        await context.SaveChangesAsync();
    }
}
