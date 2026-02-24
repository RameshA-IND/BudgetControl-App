using System.ComponentModel.DataAnnotations;

namespace BudgetControl.Application.DTOs.Expense;

public class SubmitExpenseDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string Category { get; set; } = string.Empty;

    [Required]
    public int DepartmentId { get; set; }

    public string? ReceiptUrl { get; set; }
}

public class UpdateExpenseStatusDto
{
    [Required]
    public string Action { get; set; } = string.Empty; // Approved or Rejected

    public string? Comments { get; set; }
}

public class ExpenseResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int? BudgetId { get; set; }
    public int SubmittedById { get; set; }
    public string SubmittedByName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ReceiptUrl { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ApprovalRecordDto> Approvals { get; set; } = new();
}

public class ApprovalRecordDto
{
    public int Id { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string ApproverRole { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime ActionDate { get; set; }
}
