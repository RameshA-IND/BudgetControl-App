using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using Microsoft.EntityFrameworkCore;
using BudgetControl.Application.Interfaces;
using BudgetControl.Domain.Enums;
using BudgetControl.Infrastructure.Data;

namespace BudgetControl.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context) => _context = context;

    public async Task<byte[]> ExportExpensesToExcelAsync(int? departmentId = null, string? fiscalYear = null)
    {
        var query = _context.Expenses
            .Include(e => e.Department)
            .Include(e => e.SubmittedBy)
            .Where(e => e.Status == ExpenseStatus.Approved);

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        var expenses = await query.OrderByDescending(e => e.SubmittedAt).ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Expense Report");

        // Header
        var headers = new[] { "Title", "Category", "Department", "Submitted By", "Amount", "Status", "Date" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.DarkBlue;
            worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        // Data rows
        for (int i = 0; i < expenses.Count; i++)
        {
            var e = expenses[i];
            worksheet.Cell(i + 2, 1).Value = e.Title;
            worksheet.Cell(i + 2, 2).Value = e.Category;
            worksheet.Cell(i + 2, 3).Value = e.Department?.Name ?? "";
            worksheet.Cell(i + 2, 4).Value = e.SubmittedBy?.FullName ?? "";
            worksheet.Cell(i + 2, 5).Value = e.Amount;
            worksheet.Cell(i + 2, 5).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(i + 2, 6).Value = e.Status.ToString();
            worksheet.Cell(i + 2, 7).Value = e.SubmittedAt.ToString("MMM dd, yyyy");
        }

        // Summary row
        var summaryRow = expenses.Count + 3;
        worksheet.Cell(summaryRow, 4).Value = "Total:";
        worksheet.Cell(summaryRow, 4).Style.Font.Bold = true;
        worksheet.Cell(summaryRow, 5).Value = expenses.Sum(e => e.Amount);
        worksheet.Cell(summaryRow, 5).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(summaryRow, 5).Style.Font.Bold = true;

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportExpensesToPdfAsync(int? departmentId = null, string? fiscalYear = null)
    {
        var query = _context.Expenses
            .Include(e => e.Department)
            .Include(e => e.SubmittedBy)
            .Where(e => e.Status == ExpenseStatus.Approved);

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        var expenses = await query.OrderByDescending(e => e.SubmittedAt).ToListAsync();

        using var stream = new MemoryStream();
        using var writer = new PdfWriter(stream);
        using var pdf = new PdfDocument(writer);
        var document = new Document(pdf);

        // Title
        document.Add(new Paragraph("Expense Report")
            .SetFontSize(20)
            .SetBold()
            .SetTextAlignment(TextAlignment.CENTER));

        document.Add(new Paragraph($"Generated on {DateTime.UtcNow:MMMM dd, yyyy}")
            .SetFontSize(10)
            .SetTextAlignment(TextAlignment.CENTER));

        document.Add(new Paragraph("\n"));

        // Table
        var table = new Table(new float[] { 3, 2, 2, 2, 2, 1.5f, 2 })
            .UseAllAvailableWidth();

        // Headers
        var headerBg = new DeviceRgb(30, 41, 59);
        foreach (var header in new[] { "Title", "Category", "Department", "Submitted By", "Amount", "Status", "Date" })
        {
            table.AddHeaderCell(new Cell()
                .Add(new Paragraph(header).SetFontColor(ColorConstants.WHITE).SetBold())
                .SetBackgroundColor(headerBg)
                .SetPadding(5));
        }

        // Rows
        foreach (var e in expenses)
        {
            table.AddCell(new Cell().Add(new Paragraph(e.Title)).SetPadding(4));
            table.AddCell(new Cell().Add(new Paragraph(e.Category)).SetPadding(4));
            table.AddCell(new Cell().Add(new Paragraph(e.Department?.Name ?? "")).SetPadding(4));
            table.AddCell(new Cell().Add(new Paragraph(e.SubmittedBy?.FullName ?? "")).SetPadding(4));
            table.AddCell(new Cell().Add(new Paragraph($"${e.Amount:N2}")).SetPadding(4));
            table.AddCell(new Cell().Add(new Paragraph(e.Status.ToString())).SetPadding(4));
            table.AddCell(new Cell().Add(new Paragraph(e.SubmittedAt.ToString("MMM dd, yyyy"))).SetPadding(4));
        }

        document.Add(table);

        // Total
        document.Add(new Paragraph($"\nTotal Approved Expenses: ${expenses.Sum(e => e.Amount):N2}")
            .SetBold()
            .SetFontSize(12));

        document.Close();
        return stream.ToArray();
    }
}
