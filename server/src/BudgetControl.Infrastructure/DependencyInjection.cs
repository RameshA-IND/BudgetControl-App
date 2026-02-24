using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BudgetControl.Application.Interfaces;
using BudgetControl.Domain.Interfaces;
using BudgetControl.Infrastructure.Data;
using BudgetControl.Infrastructure.Repositories;
using BudgetControl.Infrastructure.Services;

namespace BudgetControl.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database — support Render's DATABASE_URL env var OR appsettings DefaultConnection
        var connectionString = GetConnectionString(configuration);
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }

    /// <summary>
    /// Resolves the DB connection string.
    /// On Render, DATABASE_URL is injected as postgres://user:pass@host:port/db
    /// We convert that to an Npgsql-compatible format.
    /// </summary>
    private static string GetConnectionString(IConfiguration configuration)
    {
        // Check for Render's DATABASE_URL first
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            // Convert postgres://user:pass@host:port/dbname  →  Host=...;Database=...;...
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
                   $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";
        }

        // Fallback to appsettings.json for local development
        return configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("No database connection string configured.");
    }
}
