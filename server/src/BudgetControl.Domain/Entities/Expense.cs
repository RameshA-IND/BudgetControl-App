using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BudgetControl.Domain.Enums;

namespace BudgetControl.Domain.Entities;

[Table("expenses")]
public class Expense
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("amount")]
    public decimal Amount { get; set; }

    [Required, MaxLength(50)]
    [Column("category")]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Column("department_id")]
    public int DepartmentId { get; set; }

    [ForeignKey("DepartmentId")]
    public Department Department { get; set; } = null!;

    [Column("budget_id")]
    public int? BudgetId { get; set; }

    [ForeignKey("BudgetId")]
    public Budget? Budget { get; set; }

    [Required]
    [Column("submitted_by_id")]
    public int SubmittedById { get; set; }

    [ForeignKey("SubmittedById")]
    public User SubmittedBy { get; set; } = null!;

    [Required]
    [Column("status")]
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;

    [Column("receipt_url")]
    public string? ReceiptUrl { get; set; }

    [Column("submitted_at")]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ExpenseApproval> Approvals { get; set; } = new List<ExpenseApproval>();
}
