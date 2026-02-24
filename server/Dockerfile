# ── Build Stage ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy solution + project files first (layer-cache friendly)
COPY BudgetControl.sln .
COPY src/BudgetControl.API/BudgetControl.API.csproj              src/BudgetControl.API/
COPY src/BudgetControl.Application/BudgetControl.Application.csproj src/BudgetControl.Application/
COPY src/BudgetControl.Infrastructure/BudgetControl.Infrastructure.csproj src/BudgetControl.Infrastructure/
COPY src/BudgetControl.Domain/BudgetControl.Domain.csproj         src/BudgetControl.Domain/

RUN dotnet restore

# Copy remaining source and publish
COPY . .
RUN dotnet publish src/BudgetControl.API/BudgetControl.API.csproj \
    -c Release \
    -o /app \
    --no-restore

# ── Runtime Stage ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app .

# Render injects PORT; default to 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BudgetControl.API.dll"]
