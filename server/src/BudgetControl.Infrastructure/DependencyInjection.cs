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
    /// Always parses through NpgsqlConnectionStringBuilder to ensure correct
    /// encoding — especially for Supabase Supavisor pooler usernames like "postgres.projectref"
    /// which must be sent verbatim (not URL-encoded).
    /// </summary>
    private static string GetConnectionString(IConfiguration configuration)
    {
        // Priority 1: CONNECTION_STRING env var (Supabase pooler on Render)
        var directConnStr = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(directConnStr))
        {
            // Parse and re-emit via NpgsqlConnectionStringBuilder so that
            // the username "postgres.PROJECTREF" is passed correctly to Supavisor.
            var parsed = new NpgsqlConnectionStringBuilder(directConnStr);
            // Force these Supabase-pooler-safe settings
            parsed.SslMode                = SslMode.Require;
            parsed.TrustServerCertificate = true;
            parsed.Pooling                = false;  // Let Supavisor pool; don't double-pool
            return parsed.ConnectionString;
        }

        // Priority 2: Render's postgres:// DATABASE_URL (used with Render Postgres)
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            // Ensure it has a valid scheme for Uri parsing
            if (databaseUrl.StartsWith("postgres://"))
                databaseUrl = "postgresql" + databaseUrl[8..];

            var uri = new Uri(databaseUrl);

            // Split only on the FIRST colon — passwords can contain colons
            var userInfoRaw = uri.UserInfo;
            var firstColon  = userInfoRaw.IndexOf(':');
            var username     = Uri.UnescapeDataString(userInfoRaw[..firstColon]);
            var password     = Uri.UnescapeDataString(userInfoRaw[(firstColon + 1)..]);

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host                   = uri.Host,
                Port                   = uri.Port > 0 ? uri.Port : 5432,
                Database               = uri.AbsolutePath.TrimStart('/'),
                Username               = username,
                Password               = password,
                SslMode                = SslMode.Prefer,
                TrustServerCertificate = true,
                Pooling                = true,
                MaxPoolSize            = 20,
            };

            return builder.ConnectionString;
        }

        // Priority 3: appsettings.json (local development)
        return configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("No database connection string configured.");
    }
}
