using SaasSystem.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SaasSystem.Domain.Entities;

public class Invoice : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? ProjectId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public DateTime IssueDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public InvoiceStatus Status { get; private set; }

    public Customer? Customer { get; private set; }
    public Project? Project { get; private set; }
    public ICollection<Payment> Payments { get; private set; } = new List<Payment>();

    protected Invoice()
    {
    }

    public Invoice(
        Guid id,
        Guid? tenantId,
        Guid customerId,
        Guid? projectId,
        string invoiceNumber,
        DateTime issueDate,
        DateTime dueDate,
        decimal subTotal,
        decimal taxAmount)
        : base(id)
    {
        TenantId = tenantId;
        Update(customerId, projectId, invoiceNumber, issueDate, dueDate, subTotal, taxAmount);
        Status = InvoiceStatus.Draft;
    }

    public void Update(
        Guid customerId,
        Guid? projectId,
        string invoiceNumber,
        DateTime issueDate,
        DateTime dueDate,
        decimal subTotal,
        decimal taxAmount)
    {
        CustomerId = customerId;
        ProjectId = projectId;
        InvoiceNumber = Check.NotNullOrWhiteSpace(invoiceNumber, nameof(invoiceNumber), maxLength: SaasSystemConsts.InvoiceNumberMaxLength);
        IssueDate = issueDate;
        DueDate = dueDate;
        SubTotal = subTotal;
        TaxAmount = taxAmount;
        TotalAmount = subTotal + taxAmount;
    }

    public void ChangeStatus(InvoiceStatus status)
    {
        Status = status;
    }
}
