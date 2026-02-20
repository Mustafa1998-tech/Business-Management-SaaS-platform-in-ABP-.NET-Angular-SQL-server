using SaasSystem.Enums;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SaasSystem.Invoices;

public class InvoiceDto : FullAuditedEntityDto<Guid>
{
    public Guid CustomerId { get; set; }
    public Guid? ProjectId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; }
}

public class InvoiceListRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? CustomerId { get; set; }
}

public interface IInvoiceAppService : IApplicationService
{
    Task<PagedResultDto<InvoiceDto>> GetListAsync(InvoiceListRequestDto input);
}
