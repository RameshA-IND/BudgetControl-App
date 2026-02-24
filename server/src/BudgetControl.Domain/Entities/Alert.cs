using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BudgetControl.Domain.Enums;

namespace BudgetControl.Domain.Entities;

[Table("alerts")]
public class Alert
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("budget_id")]
    public int BudgetId { get; set; }

    [ForeignKey("BudgetId")]
    public Budget Budget { get; set; } = null!;

    [Required]
    [Column("department_id")]
    public int DepartmentId { get; set; }

    [ForeignKey("DepartmentId")]
    public Department Department { get; set; } = null!;

    [Required]
    [Column("severity")]
    public AlertSeverity Severity { get; set; }

    [Required]
    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("utilization_percent")]
    public decimal UtilizationPercent { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
