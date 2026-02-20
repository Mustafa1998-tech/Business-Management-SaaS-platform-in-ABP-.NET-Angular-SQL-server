using SaasSystem.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SaasSystem.Domain.Entities;

public class WorkTask : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public WorkTaskStatus Status { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime? DueDate { get; private set; }
    public decimal EstimatedHours { get; private set; }
    public decimal SpentHours { get; private set; }
    public Guid? AssignedUserId { get; private set; }

    public Project? Project { get; private set; }

    protected WorkTask()
    {
    }

    public WorkTask(Guid id, Guid? tenantId, Guid projectId, string title, string description, int sortOrder)
        : base(id)
    {
        TenantId = tenantId;
        ProjectId = projectId;
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), maxLength: SaasSystemConsts.NameMaxLength);
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), maxLength: SaasSystemConsts.DescriptionMaxLength);
        Status = WorkTaskStatus.Backlog;
        SortOrder = sortOrder;
    }

    public void MoveTo(WorkTaskStatus status, int sortOrder)
    {
        Status = status;
        SortOrder = sortOrder;
    }

    public void UpdateEffort(decimal estimatedHours, decimal spentHours)
    {
        EstimatedHours = estimatedHours;
        SpentHours = spentHours;
    }

    public void Assign(Guid? userId)
    {
        AssignedUserId = userId;
    }

    public void SetDueDate(DateTime? dueDate)
    {
        DueDate = dueDate;
    }
}
