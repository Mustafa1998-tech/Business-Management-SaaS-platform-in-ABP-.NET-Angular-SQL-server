using SaasSystem.Enums;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SaasSystem.Projects;

public class ProjectDto : FullAuditedEntityDto<Guid>
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Budget { get; set; }
    public ProjectStatus Status { get; set; }
}

public class ProjectListRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? CustomerId { get; set; }
}

public interface IProjectAppService : IApplicationService
{
    Task<PagedResultDto<ProjectDto>> GetListAsync(ProjectListRequestDto input);
}
