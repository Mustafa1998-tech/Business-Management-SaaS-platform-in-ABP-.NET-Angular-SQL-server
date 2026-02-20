using SaasSystem.Enums;
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
    public Guid? InvoiceId { get; set; }
}

public interface IPaymentAppService : IApplicationService
{
    Task<PagedResultDto<PaymentDto>> GetListAsync(PaymentListRequestDto input);
}
