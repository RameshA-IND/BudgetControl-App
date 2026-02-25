using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BudgetControl.API.Middleware;
using BudgetControl.Infrastructure;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add Infrastructure services (Clean Architecture)
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication — env vars override appsettings on Render
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey   = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")   ?? jwtSettings["SecretKey"]!;
var jwtIssuer   = Environment.GetEnvironmentVariable("JWT_ISSUER")        ?? jwtSettings["Issuer"]!;
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")      ?? jwtSettings["Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// CORS — reads ALLOWED_ORIGINS env var on Render (comma-separated)
// e.g. "https://budget-control.vercel.app,https://other-domain.com"
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = (
            Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
            ?? builder.Configuration["AllowedOrigins"]
            ?? "http://localhost:5173"
        ).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        policy
            .WithOrigins(allowedOrigins)
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BudgetControl API",
        Version = "v1",
        Description = "Departmental Budget Control & Expense Monitoring Platform"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// TEMP DEBUG: Remove after fixing connection issue
app.MapGet("/api/debug-env", () =>
{
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    var dbUser = Environment.GetEnvironmentVariable("DB_USER");
    var dbPass = Environment.GetEnvironmentVariable("DB_PASS") != null ? "SET" : "NOT_SET";
    var dbName = Environment.GetEnvironmentVariable("DB_NAME");
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
    var connStr = Environment.GetEnvironmentVariable("CONNECTION_STRING");
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    return Results.Ok(new
    {
        DB_HOST = dbHost ?? "NOT_SET",
        DB_USER = dbUser ?? "NOT_SET",
        DB_PASS = dbPass,
        DB_NAME = dbName ?? "NOT_SET",
        DB_PORT = dbPort ?? "NOT_SET",
        CONNECTION_STRING_PREFIX = connStr != null ? connStr[..Math.Min(40, connStr.Length)] : "NOT_SET",
        DATABASE_URL_SET = databaseUrl != null ? "YES" : "NO",
        ASPNETCORE_ENV = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "NOT_SET"
    });
});

// Seed Database — retry because Render DB can take a few seconds to be ready
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<BudgetControl.Infrastructure.Data.AppDbContext>();

    const int maxRetries = 5;
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Database seed attempt {Attempt}/{Max}...", attempt, maxRetries);
            await BudgetControl.Infrastructure.Data.DbSeeder.SeedAsync(context);
            logger.LogInformation("Database seeded successfully.");
            break;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            logger.LogWarning(ex, "DB seed attempt {Attempt} failed. Retrying in 3s...", attempt);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database seeding failed after {Max} attempts. App will start anyway.", maxRetries);
        }
    }
}


// Render sets PORT env var; locally defaults to 5000
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
