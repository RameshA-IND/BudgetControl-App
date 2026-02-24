using System.ComponentModel.DataAnnotations;

namespace BudgetControl.Application.DTOs.Budget;

public class CreateBudgetDto
{
    [Required]
    public int DepartmentId { get; set; }

    [Required]
    public string FiscalYear { get; set; } = string.Empty;

    [Required]
    public DateTime PeriodStart { get; set; }

    [Required]
    public DateTime PeriodEnd { get; set; }

    [Required]
    public decimal AllocatedAmount { get; set; }

    public decimal WarningThresholdPct { get; set; } = 75.00m;
    public decimal CriticalThresholdPct { get; set; } = 90.00m;
}

public class UpdateBudgetDto
{
    public decimal? AllocatedAmount { get; set; }
    public decimal? WarningThresholdPct { get; set; }
    public decimal? CriticalThresholdPct { get; set; }
    public string? Status { get; set; }
}

public class BudgetResponseDto
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public string FiscalYear { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal UtilizationPercent { get; set; }
    public decimal WarningThresholdPct { get; set; }
    public decimal CriticalThresholdPct { get; set; }
    public string Status { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty; // Healthy, Warning, Critical
    public DateTime CreatedAt { get; set; }
}
