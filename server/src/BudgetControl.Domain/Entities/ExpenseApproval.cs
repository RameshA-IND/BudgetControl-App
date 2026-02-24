using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BudgetControl.Domain.Enums;

namespace BudgetControl.Domain.Entities;

[Table("expense_approvals")]
public class ExpenseApproval
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("expense_id")]
    public int ExpenseId { get; set; }

    [ForeignKey("ExpenseId")]
    public Expense Expense { get; set; } = null!;

    [Required]
    [Column("approver_id")]
    public int ApproverId { get; set; }

    [ForeignKey("ApproverId")]
    public User Approver { get; set; } = null!;

    [Required]
    [Column("approver_role")]
    public UserRole ApproverRole { get; set; }

    [Required]
    [Column("action")]
    public ApprovalAction Action { get; set; }

    [Column("comments")]
    public string? Comments { get; set; }

    [Column("action_date")]
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
}
