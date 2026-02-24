using Microsoft.EntityFrameworkCore;
using BudgetControl.Domain.Entities;
using BudgetControl.Domain.Enums;
using BudgetControl.Domain.Interfaces;

namespace BudgetControl.Infrastructure.Data;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseApproval> ExpenseApprovals => Set<ExpenseApproval>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        => base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ──── User ─────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(e => e.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Store enum as string (varchar) – no custom PG enum needed
            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(30);
        });

        // ──── Department ───────────────────────────────────────
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ──── Budget ───────────────────────────────────────────
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasIndex(e => new { e.DepartmentId, e.FiscalYear }).IsUnique();
            entity.HasOne(e => e.Department)
                .WithMany(d => d.Budgets)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.AllocatedAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SpentAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.WarningThresholdPct).HasColumnType("decimal(5,2)");
            entity.Property(e => e.CriticalThresholdPct).HasColumnType("decimal(5,2)");
        });

        // ──── Expense ──────────────────────────────────────────
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasOne(e => e.Department)
                .WithMany(d => d.Expenses)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Budget)
                .WithMany(b => b.Expenses)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.SubmittedBy)
                .WithMany(u => u.SubmittedExpenses)
                .HasForeignKey(e => e.SubmittedById);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        });

        // ──── ExpenseApproval ──────────────────────────────────
        modelBuilder.Entity<ExpenseApproval>(entity =>
        {
            entity.HasOne(e => e.Expense)
                .WithMany(ex => ex.Approvals)
                .HasForeignKey(e => e.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Approver)
                .WithMany(u => u.Approvals)
                .HasForeignKey(e => e.ApproverId);

            entity.Property(e => e.ApproverRole)
                .HasConversion<string>()
                .HasMaxLength(30);
            entity.Property(e => e.Action)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        // ──── Alert ────────────────────────────────────────────
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasOne(e => e.Budget)
                .WithMany(b => b.Alerts)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Severity)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.UtilizationPercent).HasColumnType("decimal(5,2)");
        });

        // ──── ExpenseCategory ──────────────────────────────────
        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });
    }
}
