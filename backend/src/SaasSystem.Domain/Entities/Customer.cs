using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SaasSystem.Domain.Entities;

public class Customer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public ICollection<Project> Projects { get; private set; } = new List<Project>();
    public ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();

    protected Customer()
    {
    }

    public Customer(Guid id, Guid? tenantId, string name, string email, string phone, string address) : base(id)
    {
        TenantId = tenantId;
        SetName(name);
        SetEmail(email);
        SetPhone(phone);
        SetAddress(address);
        IsActive = true;
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: SaasSystemConsts.NameMaxLength);
    }

    public void SetEmail(string email)
    {
        Email = Check.NotNullOrWhiteSpace(email, nameof(email), maxLength: SaasSystemConsts.EmailMaxLength);
    }

    public void SetPhone(string phone)
    {
        Phone = Check.NotNullOrWhiteSpace(phone, nameof(phone), maxLength: SaasSystemConsts.PhoneMaxLength);
    }

    public void SetAddress(string address)
    {
        Address = Check.NotNullOrWhiteSpace(address, nameof(address), maxLength: SaasSystemConsts.DescriptionMaxLength);
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
