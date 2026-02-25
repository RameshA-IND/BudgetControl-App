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
    /// Priority:
    ///   1. Individual DB_HOST / DB_USER / DB_PASS / DB_NAME / DB_PORT env vars (safest for Supabase pooler)
    ///   2. CONNECTION_STRING env var (raw Npgsql keyword=value string)
    ///   3. DATABASE_URL env var (postgres:// URI — used by Render Postgres)
    ///   4. appsettings.json DefaultConnection (local dev)
    /// </summary>
    private static string GetConnectionString(IConfiguration configuration)
    {
        // Priority 1: Explicit individual variables — zero parsing needed.
        // This guarantees the Supabase pooler username "postgres.PROJECTREF" is NEVER mangled.
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPass = Environment.GetEnvironmentVariable("DB_PASS");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "postgres";
        var dbPort = int.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out var p) ? p : 5432;

        if (!string.IsNullOrWhiteSpace(dbHost) && !string.IsNullOrWhiteSpace(dbUser))
        {
            return new NpgsqlConnectionStringBuilder
            {
                Host                   = dbHost,
                Port                   = dbPort,
                Database               = dbName,
                Username               = dbUser,
                Password               = dbPass,
                SslMode                = SslMode.Require,
                TrustServerCertificate = true,
                Pooling                = false, // Supavisor handles pooling
            }.ConnectionString;
        }

        // Priority 2: Plain Npgsql keyword=value connection string
        var directConnStr = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(directConnStr))
        {
            var parsed = new NpgsqlConnectionStringBuilder(directConnStr)
            {
                SslMode                = SslMode.Require,
                TrustServerCertificate = true,
                Pooling                = false,
            };
            return parsed.ConnectionString;
        }

        // Priority 3: Render's postgres:// DATABASE_URL URI
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            if (databaseUrl.StartsWith("postgres://"))
                databaseUrl = "postgresql" + databaseUrl[8..];

            var uri        = new Uri(databaseUrl);
            var userInfo   = uri.UserInfo;
            var colonIdx   = userInfo.IndexOf(':');
            var username   = Uri.UnescapeDataString(userInfo[..colonIdx]);
            var password   = Uri.UnescapeDataString(userInfo[(colonIdx + 1)..]);

            return new NpgsqlConnectionStringBuilder
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
            }.ConnectionString;
        }

        // Priority 4: appsettings.json (local development)
        return configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("No database connection string configured.");
    }
}
