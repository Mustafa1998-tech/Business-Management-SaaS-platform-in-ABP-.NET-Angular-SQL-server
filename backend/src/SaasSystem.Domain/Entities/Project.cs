using SaasSystem.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SaasSystem.Domain.Entities;

public class Project : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public decimal Budget { get; private set; }
    public ProjectStatus Status { get; private set; }

    public Customer? Customer { get; private set; }
    public ICollection<WorkTask> Tasks { get; private set; } = new List<WorkTask>();

    protected Project()
    {
    }

    public Project(Guid id, Guid? tenantId, Guid customerId, string name, string description, DateTime startDate, decimal budget)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: SaasSystemConsts.NameMaxLength);
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), maxLength: SaasSystemConsts.DescriptionMaxLength);
        StartDate = startDate;
        Budget = budget;
        Status = ProjectStatus.Planned;
    }

    public void ChangeStatus(ProjectStatus status)
    {
        Status = status;
    }

    public void SetEndDate(DateTime? endDate)
    {
        EndDate = endDate;
    }
}
