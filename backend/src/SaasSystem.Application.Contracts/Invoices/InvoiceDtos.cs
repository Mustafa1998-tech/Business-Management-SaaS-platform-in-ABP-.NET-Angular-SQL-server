using SaasSystem.Enums;
using System.ComponentModel.DataAnnotations;
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
    [StringLength(SaasSystemConsts.InvoiceNumberMaxLength)]
    public string? Filter { get; set; }

    public Guid? CustomerId { get; set; }

    public InvoiceStatus? Status { get; set; }
}

public class CreateUpdateInvoiceDto
{
    [Required]
    public Guid CustomerId { get; set; }

    public Guid? ProjectId { get; set; }

    [Required]
    [StringLength(SaasSystemConsts.InvoiceNumberMaxLength)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; } = DateTime.UtcNow.Date;

    public DateTime DueDate { get; set; } = DateTime.UtcNow.Date.AddDays(30);

    [Range(0, double.MaxValue)]
    public decimal SubTotal { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TaxAmount { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
}

public interface IInvoiceAppService : IApplicationService
{
    Task<InvoiceDto> GetAsync(Guid id);

    Task<PagedResultDto<InvoiceDto>> GetListAsync(InvoiceListRequestDto input);

    Task<InvoiceDto> CreateAsync(CreateUpdateInvoiceDto input);

    Task<InvoiceDto> UpdateAsync(Guid id, CreateUpdateInvoiceDto input);

    Task DeleteAsync(Guid id);
}
