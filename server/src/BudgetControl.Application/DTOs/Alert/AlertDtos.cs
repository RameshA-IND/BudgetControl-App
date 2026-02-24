namespace BudgetControl.Application.DTOs.Alert;

public class AlertResponseDto
{
    public int Id { get; set; }
    public int BudgetId { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal UtilizationPercent { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
