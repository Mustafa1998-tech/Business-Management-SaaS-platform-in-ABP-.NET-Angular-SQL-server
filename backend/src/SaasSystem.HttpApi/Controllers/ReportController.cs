using Microsoft.AspNetCore.Mvc;
using SaasSystem.Reports;

namespace SaasSystem.Controllers;

[ApiController]
[Route("api/app/reports")]
public class ReportController : ControllerBase
{
    private readonly IReportsAppService _service;

    public ReportController(IReportsAppService service)
    {
        _service = service;
    }

    [HttpGet("revenue")]
    public async Task<RevenueReportDto> GetRevenueReportAsync([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        return await _service.GetRevenueReportAsync(from, to);
    }

    [HttpGet("invoices")]
    public async Task<InvoicesReportResultDto> GetInvoicesReportAsync([FromQuery] InvoiceReportFilterDto input)
    {
        return await _service.GetInvoicesReportAsync(input);
    }

    [HttpGet("invoices-excel")]
    public async Task<FileResult> GetInvoicesExcelAsync([FromQuery] InvoiceReportFilterDto input)
    {
        FileDto file = await _service.GetInvoicesExcelAsync(input);
        byte[] content = await _service.GetReportFileContentAsync(file.FileToken);
        return File(content, file.MimeType, file.FileName);
    }

    [HttpGet("invoices-pdf")]
    public async Task<FileResult> GetInvoicesPdfAsync([FromQuery] InvoiceReportFilterDto input)
    {
        FileDto file = await _service.GetInvoicesPdfAsync(input);
        byte[] content = await _service.GetReportFileContentAsync(file.FileToken);
        return File(content, file.MimeType, file.FileName);
    }
}
