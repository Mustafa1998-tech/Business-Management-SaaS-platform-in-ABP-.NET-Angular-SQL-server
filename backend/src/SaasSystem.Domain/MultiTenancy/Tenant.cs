using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SaasSystem.Domain.MultiTenancy;

public class TenantProfile : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Edition { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    protected TenantProfile()
    {
    }

    public TenantProfile(Guid id, Guid? tenantId, string name, string edition) : base(id)
    {
        TenantId = tenantId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: SaasSystemConsts.NameMaxLength);
        Edition = Check.NotNullOrWhiteSpace(edition, nameof(edition), maxLength: SaasSystemConsts.NameMaxLength);
        IsActive = true;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
