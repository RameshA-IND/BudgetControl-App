namespace BudgetControl.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> ExportExpensesToExcelAsync(int? departmentId = null, string? fiscalYear = null);
    Task<byte[]> ExportExpensesToPdfAsync(int? departmentId = null, string? fiscalYear = null);
}
