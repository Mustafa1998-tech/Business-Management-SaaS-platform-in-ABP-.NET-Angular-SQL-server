using Microsoft.AspNetCore.Authorization;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SaasSystem.Domain.Entities;
using SaasSystem.Enums;
using SaasSystem.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SaasSystem.Reports;

[Authorize(SaasSystemPermissions.Reports.Default)]
public class ReportsAppService : ApplicationService, IReportsAppService
{
    private const int MaxPdfRows = 1500;

    private readonly IRepository<Invoice, Guid> _invoiceRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IReportFileTokenCache _fileTokenCache;

    static ReportsAppService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ReportsAppService(
        IRepository<Invoice, Guid> invoiceRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Project, Guid> projectRepository,
        IRepository<Payment, Guid> paymentRepository,
        IReportFileTokenCache fileTokenCache)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _projectRepository = projectRepository;
        _paymentRepository = paymentRepository;
        _fileTokenCache = fileTokenCache;
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime from, DateTime to)
    {
        IQueryable<Payment> queryable = await _paymentRepository.GetQueryableAsync();
        List<Payment> payments = await AsyncExecuter.ToListAsync(
            queryable
                .Where(x => x.PaidAt >= from && x.PaidAt <= to)
                .OrderBy(x => x.PaidAt));

        List<RevenueBucketDto> buckets = payments
            .GroupBy(x => new { x.PaidAt.Year, x.PaidAt.Month })
            .Select(x => new RevenueBucketDto
            {
                Period = $"{x.Key.Year}-{x.Key.Month:00}",
                Amount = x.Sum(y => y.Amount)
            })
            .ToList();

        return new RevenueReportDto
        {
            From = from,
            To = to,
            TotalRevenue = payments.Sum(x => x.Amount),
            Buckets = buckets
        };
    }

    public async Task<InvoicesReportResultDto> GetInvoicesReportAsync(InvoiceReportFilterDto input)
    {
        List<InvoiceProjection> invoiceRows = await GetInvoiceRowsAsync(input);
        return await BuildResultAsync(invoiceRows);
    }

    public async Task<FileDto> GetInvoicesExcelAsync(InvoiceReportFilterDto input)
    {
        InvoicesReportResultDto report = await GetInvoicesReportAsync(input);
        byte[] bytes = await BuildExcelAsync(report);
        string fileName = $"Invoices-{Clock.Now:yyyyMMdd-HHmmss}.xlsx";

        return new FileDto
        {
            FileName = fileName,
            MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileToken = await _fileTokenCache.SetAsync(bytes)
        };
    }

    public async Task<FileDto> GetInvoicesPdfAsync(InvoiceReportFilterDto input)
    {
        InvoicesReportResultDto report = await GetInvoicesReportAsync(input);
        byte[] bytes = BuildPdf(report);
        string fileName = $"Invoices-{Clock.Now:yyyyMMdd-HHmmss}.pdf";

        return new FileDto
        {
            FileName = fileName,
            MimeType = "application/pdf",
            FileToken = await _fileTokenCache.SetAsync(bytes)
        };
    }

    public async Task<byte[]> GetReportFileContentAsync(string fileToken)
    {
        ReportFileCacheItem? item = await _fileTokenCache.GetAsync(fileToken);
        if (item is null)
        {
            throw new AbpException("Report file token expired.");
        }

        return item.Content;
    }

    private async Task<List<InvoiceProjection>> GetInvoiceRowsAsync(InvoiceReportFilterDto input)
    {
        IQueryable<Invoice> queryable = await _invoiceRepository.GetQueryableAsync();

        if (input.FromDate.HasValue)
        {
            DateTime fromDate = input.FromDate.Value.Date;
            queryable = queryable.Where(x => x.IssueDate >= fromDate);
        }

        if (input.ToDate.HasValue)
        {
            DateTime toDateInclusive = input.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            queryable = queryable.Where(x => x.IssueDate <= toDateInclusive);
        }

        if (input.CustomerId.HasValue)
        {
            queryable = queryable.Where(x => x.CustomerId == input.CustomerId.Value);
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        return await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.IssueDate)
                .Select(x => new InvoiceProjection
                {
                    Id = x.Id,
                    InvoiceNo = x.InvoiceNumber,
                    CustomerId = x.CustomerId,
                    ProjectId = x.ProjectId,
                    Amount = x.TotalAmount,
                    Status = x.Status,
                    Date = x.IssueDate
                }));
    }

    private async Task<InvoicesReportResultDto> BuildResultAsync(List<InvoiceProjection> invoiceRows)
    {
        if (invoiceRows.Count == 0)
        {
            return new InvoicesReportResultDto
            {
                Summary = new InvoiceReportSummaryDto(),
                Items = Array.Empty<InvoiceReportDto>()
            };
        }

        List<Guid> customerIds = invoiceRows
            .Select(x => x.CustomerId)
            .Distinct()
            .ToList();

        List<Guid> projectIds = invoiceRows
            .Where(x => x.ProjectId.HasValue)
            .Select(x => x.ProjectId!.Value)
            .Distinct()
            .ToList();

        IQueryable<Customer> customerQueryable = await _customerRepository.GetQueryableAsync();
        List<CustomerLookupItem> customers = await AsyncExecuter.ToListAsync(
            customerQueryable
                .Where(x => customerIds.Contains(x.Id))
                .Select(x => new CustomerLookupItem { Id = x.Id, Name = x.Name }));

        IQueryable<Project> projectQueryable = await _projectRepository.GetQueryableAsync();
        List<ProjectLookupItem> projects = await AsyncExecuter.ToListAsync(
            projectQueryable
                .Where(x => projectIds.Contains(x.Id))
                .Select(x => new ProjectLookupItem { Id = x.Id, Name = x.Name }));

        Dictionary<Guid, string> customerMap = customers.ToDictionary(x => x.Id, x => x.Name);
        Dictionary<Guid, string> projectMap = projects.ToDictionary(x => x.Id, x => x.Name);

        List<InvoiceReportDto> rows = invoiceRows
            .Select(x => new InvoiceReportDto
            {
                InvoiceNo = x.InvoiceNo,
                CustomerName = customerMap.TryGetValue(x.CustomerId, out string? customerName) ? customerName : "Unknown Customer",
                ProjectName = x.ProjectId.HasValue && projectMap.TryGetValue(x.ProjectId.Value, out string? projectName)
                    ? projectName
                    : "N/A",
                Amount = x.Amount,
                Status = x.Status,
                Date = x.Date
            })
            .ToList();

        List<Guid> invoiceIds = invoiceRows.Select(x => x.Id).ToList();
        IQueryable<Payment> paymentQueryable = await _paymentRepository.GetQueryableAsync();

        decimal totalRevenue = (
            await AsyncExecuter.ToListAsync(
                paymentQueryable
                    .Where(x => invoiceIds.Contains(x.InvoiceId))
                    .Select(x => x.Amount)))
            .Sum();

        int paidInvoices = rows.Count(x => x.Status == InvoiceStatus.Paid);
        int pendingInvoices = rows.Count(x => x.Status is InvoiceStatus.Draft or InvoiceStatus.Sent or InvoiceStatus.PartiallyPaid);
        int overdueInvoices = rows.Count(x => x.Status == InvoiceStatus.Overdue);

        return new InvoicesReportResultDto
        {
            Summary = new InvoiceReportSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalInvoices = rows.Count,
                PaidInvoices = paidInvoices,
                PendingInvoices = pendingInvoices,
                OverdueInvoices = overdueInvoices
            },
            Items = rows
        };
    }

    private static async Task<byte[]> BuildExcelAsync(InvoicesReportResultDto report)
    {
        List<SummaryExcelRow> summarySheet =
        [
            new() { Metric = "Total Revenue", Value = $"{report.Summary.TotalRevenue:N2} SAR" },
            new() { Metric = "Total Invoices", Value = report.Summary.TotalInvoices.ToString() },
            new() { Metric = "Paid Invoices", Value = report.Summary.PaidInvoices.ToString() },
            new() { Metric = "Pending Invoices", Value = report.Summary.PendingInvoices.ToString() },
            new() { Metric = "Overdue Invoices", Value = report.Summary.OverdueInvoices.ToString() }
        ];

        List<InvoiceExcelRow> invoicesSheet = report.Items
            .Select(x => new InvoiceExcelRow
            {
                InvoiceNo = x.InvoiceNo,
                Customer = x.CustomerName,
                Project = x.ProjectName,
                Amount = x.Amount,
                Status = x.Status.ToString(),
                Date = x.Date.ToString("yyyy-MM-dd")
            })
            .ToList();

        Dictionary<string, object> sheets = new()
        {
            ["Summary"] = summarySheet,
            ["Invoices"] = invoicesSheet
        };

        OpenXmlConfiguration config = new()
        {
            TableStyles = TableStyles.Default
        };

        await using MemoryStream stream = new();
        await stream.SaveAsAsync(sheets, configuration: config);
        return stream.ToArray();
    }

    private byte[] BuildPdf(InvoicesReportResultDto report)
    {
        IReadOnlyList<InvoiceReportDto> rows = report.Items.Take(MaxPdfRows).ToList();
        bool isTruncated = report.Items.Count > MaxPdfRows;
        DateTime generatedOn = Clock.Now;

        Document document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.BlueGrey.Darken3));

                page.Header().Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(44).Height(30).Background("#1476CC").AlignCenter().AlignMiddle()
                            .Text("SaaS").FontColor(Colors.White).Bold().FontSize(11);

                        row.RelativeItem().PaddingLeft(10).Column(left =>
                        {
                            left.Item().Text("Invoices Report").FontSize(18).Bold().FontColor("#0F5A9C");
                            left.Item().Text($"Generated: {generatedOn:yyyy-MM-dd HH:mm}").FontSize(9).FontColor("#5B7890");
                        });
                    });

                    column.Item().PaddingTop(8).LineHorizontal(1).LineColor("#D6E6F6");
                });

                page.Content().Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Element(c => SummaryCard(c, "Total Revenue", $"{report.Summary.TotalRevenue:N2} SAR", "#E3F6ED", "#1A9B64"));
                        row.RelativeItem().Element(c => SummaryCard(c, "Invoices", report.Summary.TotalInvoices.ToString(), "#EAF3FF", "#1476CC"));
                        row.RelativeItem().Element(c => SummaryCard(c, "Paid", report.Summary.PaidInvoices.ToString(), "#E8F8EF", "#157347"));
                        row.RelativeItem().Element(c => SummaryCard(c, "Pending", report.Summary.PendingInvoices.ToString(), "#FFF3CD", "#996300"));
                        row.RelativeItem().Element(c => SummaryCard(c, "Overdue", report.Summary.OverdueInvoices.ToString(), "#FCEAEA", "#B42318"));
                    });

                    if (isTruncated)
                    {
                        column.Item().Background("#FFF4D6").Padding(6).Text(
                            $"Showing first {MaxPdfRows} rows out of {report.Items.Count} rows. Apply filters for a full PDF.")
                            .FontColor("#996300")
                            .FontSize(9);
                    }

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Invoice No").SemiBold().FontColor("#0F5A9C");
                            header.Cell().Element(HeaderCell).Text("Customer").SemiBold().FontColor("#0F5A9C");
                            header.Cell().Element(HeaderCell).Text("Project").SemiBold().FontColor("#0F5A9C");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Amount").SemiBold().FontColor("#0F5A9C");
                            header.Cell().Element(HeaderCell).Text("Status").SemiBold().FontColor("#0F5A9C");
                            header.Cell().Element(HeaderCell).Text("Date").SemiBold().FontColor("#0F5A9C");
                        });

                        foreach (InvoiceReportDto item in rows)
                        {
                            table.Cell().Element(BodyCell).Text(item.InvoiceNo);
                            table.Cell().Element(BodyCell).Text(item.CustomerName);
                            table.Cell().Element(BodyCell).Text(item.ProjectName);
                            table.Cell().Element(BodyCell).AlignRight().Text($"{item.Amount:N2} SAR");
                            table.Cell().Element(BodyCell).Text(text =>
                            {
                                text.Span(item.Status.ToString()).FontColor(GetStatusColor(item.Status)).SemiBold();
                            });
                            table.Cell().Element(BodyCell).Text(item.Date.ToString("yyyy-MM-dd"));
                        }
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated by SaaS System | ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        using MemoryStream stream = new();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private static IContainer SummaryCard(IContainer container, string label, string value, string background, string valueColor)
    {
        IContainer card = container
            .Background(background)
            .Border(1)
            .BorderColor("#D6E6F6")
            .PaddingVertical(8)
            .PaddingHorizontal(10);

        card.Column(column =>
        {
            column.Item().Text(label).FontSize(9).FontColor("#5B7890");
            column.Item().Text(value).SemiBold().FontSize(12).FontColor(valueColor);
        });

        return card;
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .Background("#EAF3FF")
            .Border(1)
            .BorderColor("#D6E6F6")
            .PaddingVertical(6)
            .PaddingHorizontal(6);
    }

    private static IContainer BodyCell(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor("#E2E8F0")
            .PaddingVertical(5)
            .PaddingHorizontal(6);
    }

    private static string GetStatusColor(InvoiceStatus status)
    {
        return status switch
        {
            InvoiceStatus.Paid => "#1A9B64",
            InvoiceStatus.Overdue => "#B42318",
            InvoiceStatus.PartiallyPaid => "#996300",
            InvoiceStatus.Sent => "#1476CC",
            InvoiceStatus.Draft => "#5B7890",
            InvoiceStatus.Cancelled => "#6B7280",
            _ => "#12314A"
        };
    }

    private sealed class InvoiceProjection
    {
        public Guid Id { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public Guid? ProjectId { get; set; }
        public decimal Amount { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime Date { get; set; }
    }

    private sealed class CustomerLookupItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class ProjectLookupItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class SummaryExcelRow
    {
        public string Metric { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    private sealed class InvoiceExcelRow
    {
        public string InvoiceNo { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Project { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
    }
}
