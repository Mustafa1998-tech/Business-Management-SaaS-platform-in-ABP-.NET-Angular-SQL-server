using SaasSystem.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SaasSystem.Domain.Entities;

public class Payment : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime PaidAt { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string ReferenceNumber { get; private set; } = string.Empty;

    public Invoice? Invoice { get; private set; }

    protected Payment()
    {
    }

    public Payment(Guid id, Guid? tenantId, Guid invoiceId, decimal amount, DateTime paidAt, PaymentMethod method, string referenceNumber)
        : base(id)
    {
        TenantId = tenantId;
        InvoiceId = invoiceId;
        Amount = amount;
        PaidAt = paidAt;
        Method = method;
        ReferenceNumber = Check.NotNullOrWhiteSpace(referenceNumber, nameof(referenceNumber), maxLength: SaasSystemConsts.NameMaxLength);
    }
}
