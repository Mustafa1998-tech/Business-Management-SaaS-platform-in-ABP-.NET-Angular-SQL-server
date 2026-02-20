using SaasSystem.Enums;
using Volo.Abp.Application.Services;

namespace SaasSystem.Reports;

public class InvoiceReportFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? CustomerId { get; set; }
    public InvoiceStatus? Status { get; set; }
}

public class InvoiceReportDto
{
    public string InvoiceNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime Date { get; set; }
}

public class InvoiceReportSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int PendingInvoices { get; set; }
    public int OverdueInvoices { get; set; }
}

public class InvoicesReportResultDto
{
    public InvoiceReportSummaryDto Summary { get; set; } = new();
    public IReadOnlyList<InvoiceReportDto> Items { get; set; } = Array.Empty<InvoiceReportDto>();
}

public class FileDto
{
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string FileToken { get; set; } = string.Empty;
}

public class RevenueReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal TotalRevenue { get; set; }
    public IReadOnlyList<RevenueBucketDto> Buckets { get; set; } = Array.Empty<RevenueBucketDto>();
}

public class RevenueBucketDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public interface IReportsAppService : IApplicationService
{
    Task<RevenueReportDto> GetRevenueReportAsync(DateTime from, DateTime to);
    Task<InvoicesReportResultDto> GetInvoicesReportAsync(InvoiceReportFilterDto input);
    Task<FileDto> GetInvoicesExcelAsync(InvoiceReportFilterDto input);
    Task<FileDto> GetInvoicesPdfAsync(InvoiceReportFilterDto input);
    Task<byte[]> GetReportFileContentAsync(string fileToken);
}
