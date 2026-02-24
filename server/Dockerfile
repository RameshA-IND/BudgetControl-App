# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy solution and projects
COPY BudgetControl.sln .
COPY src/BudgetControl.API/BudgetControl.API.csproj src/BudgetControl.API/
COPY src/BudgetControl.Application/BudgetControl.Application.csproj src/BudgetControl.Application/
COPY src/BudgetControl.Infrastructure/BudgetControl.Infrastructure.csproj src/BudgetControl.Infrastructure/
COPY src/BudgetControl.Domain/BudgetControl.Domain.csproj src/BudgetControl.Domain/
RUN dotnet restore

# Copy all source and build
COPY . .
WORKDIR /source/src/BudgetControl.API
RUN dotnet publish -c Release -o /app

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "BudgetControl.API.dll"]
