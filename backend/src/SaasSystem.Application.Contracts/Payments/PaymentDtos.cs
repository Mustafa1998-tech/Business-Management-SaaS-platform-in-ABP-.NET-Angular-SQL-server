using SaasSystem.Enums;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SaasSystem.Payments;

public class PaymentDto : FullAuditedEntityDto<Guid>
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public PaymentMethod Method { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}

public class PaymentListRequestDto : PagedAndSortedResultRequestDto
{
    [StringLength(SaasSystemConsts.NameMaxLength)]
    public string? Filter { get; set; }

    public Guid? InvoiceId { get; set; }

    public PaymentMethod? Method { get; set; }
}

public class CreateUpdatePaymentDto
{
    [Required]
    public Guid InvoiceId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    public DateTime PaidAt { get; set; } = DateTime.UtcNow;

    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;

    [Required]
    [StringLength(SaasSystemConsts.NameMaxLength)]
    public string ReferenceNumber { get; set; } = string.Empty;
}

public interface IPaymentAppService : IApplicationService
{
    Task<PaymentDto> GetAsync(Guid id);

    Task<PagedResultDto<PaymentDto>> GetListAsync(PaymentListRequestDto input);

    Task<PaymentDto> CreateAsync(CreateUpdatePaymentDto input);

    Task<PaymentDto> UpdateAsync(Guid id, CreateUpdatePaymentDto input);

    Task DeleteAsync(Guid id);
}
