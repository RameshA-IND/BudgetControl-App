using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BudgetControl.Application.Interfaces;
using BudgetControl.Domain.Interfaces;
using BudgetControl.Infrastructure.Data;
using BudgetControl.Infrastructure.Repositories;
using BudgetControl.Infrastructure.Services;
using Npgsql;

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
    /// We convert that to an Npgsql-compatible connection string.
    /// </summary>
    private static string GetConnectionString(IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            // Ensure it has a valid scheme for Uri parsing
            if (databaseUrl.StartsWith("postgres://"))
                databaseUrl = "postgresql" + databaseUrl[8..];

            var uri = new Uri(databaseUrl);

            // Split only on the FIRST colon to handle passwords that contain colons
            var userInfoRaw = uri.UserInfo;
            var firstColon  = userInfoRaw.IndexOf(':');
            var username     = Uri.UnescapeDataString(userInfoRaw[..firstColon]);
            var password     = Uri.UnescapeDataString(userInfoRaw[(firstColon + 1)..]);

            // Build a clean Npgsql connection string using NpgsqlConnectionStringBuilder
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host                = uri.Host,
                Port                = uri.Port > 0 ? uri.Port : 5432,
                Database            = uri.AbsolutePath.TrimStart('/'),
                Username            = username,
                Password            = password,
                // Internal Render connections work fine with Prefer; external need Require
                SslMode             = SslMode.Prefer,
                TrustServerCertificate = true,
                Pooling             = true,
                MaxPoolSize         = 20,
                ConnectionIdleLifetime = 300,
            };

            return builder.ConnectionString;
        }

        // Fallback to appsettings.json for local development
        return configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("No database connection string configured.");
    }
}
