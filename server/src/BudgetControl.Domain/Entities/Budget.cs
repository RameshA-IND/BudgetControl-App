using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BudgetControl.Domain.Enums;

namespace BudgetControl.Domain.Entities;

[Table("budgets")]
public class Budget
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("department_id")]
    public int DepartmentId { get; set; }

    [ForeignKey("DepartmentId")]
    public Department Department { get; set; } = null!;

    [Required, MaxLength(10)]
    [Column("fiscal_year")]
    public string FiscalYear { get; set; } = string.Empty;

    [Required]
    [Column("period_start")]
    public DateTime PeriodStart { get; set; }

    [Required]
    [Column("period_end")]
    public DateTime PeriodEnd { get; set; }

    [Required]
    [Column("allocated_amount")]
    public decimal AllocatedAmount { get; set; }

    [Column("spent_amount")]
    public decimal SpentAmount { get; set; } = 0;

    [Column("warning_threshold_pct")]
    public decimal WarningThresholdPct { get; set; } = 75.00m;

    [Column("critical_threshold_pct")]
    public decimal CriticalThresholdPct { get; set; } = 90.00m;

    [Required]
    [Column("status")]
    public BudgetStatus Status { get; set; } = BudgetStatus.Active;

    [Required]
    [Column("created_by_id")]
    public int CreatedById { get; set; }

    [ForeignKey("CreatedById")]
    public User CreatedBy { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    [NotMapped]
    public decimal RemainingAmount => AllocatedAmount - SpentAmount;

    [NotMapped]
    public decimal UtilizationPercent => AllocatedAmount > 0
        ? Math.Round((SpentAmount / AllocatedAmount) * 100, 2)
        : 0;
}
