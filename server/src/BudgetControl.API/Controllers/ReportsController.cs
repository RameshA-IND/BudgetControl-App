using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BudgetControl.Application.Interfaces;

namespace BudgetControl.API.Controllers;

[Authorize(Roles = "FinanceAdmin")]
public class ReportsController : BaseController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService) => _reportService = reportService;

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] int? departmentId, [FromQuery] string? fiscalYear)
    {
        var fileBytes = await _reportService.ExportExpensesToExcelAsync(departmentId, fiscalYear);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"expense_report_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] int? departmentId, [FromQuery] string? fiscalYear)
    {
        var fileBytes = await _reportService.ExportExpensesToPdfAsync(departmentId, fiscalYear);
        return File(fileBytes, "application/pdf",
            $"expense_report_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
