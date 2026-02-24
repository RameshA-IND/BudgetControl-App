namespace BudgetControl.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public decimal TotalBudget { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalRemaining { get; set; }
    public int ActiveAlerts { get; set; }
    public int PendingApprovals { get; set; }
    public int TotalDepartments { get; set; }
    public decimal OverallUtilization { get; set; }
}

public class DepartmentSpendingDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public decimal AllocatedAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal UtilizationPercent { get; set; }
    public string HealthStatus { get; set; } = "Healthy";
}

public class CategorySpendingDto
{
    public string Category { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
    public int ExpenseCount { get; set; }
}

public class MonthlyTrendDto
{
    public string Month { get; set; } = string.Empty;
    public decimal BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
}

public class PendingApprovalDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}
