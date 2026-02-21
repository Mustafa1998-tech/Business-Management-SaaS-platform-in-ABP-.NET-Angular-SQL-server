using SaasSystem.Enums;
using System.ComponentModel.DataAnnotations;
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
    [StringLength(SaasSystemConsts.NameMaxLength)]
    public string? Filter { get; set; }

    public Guid? CustomerId { get; set; }

    public ProjectStatus? Status { get; set; }
}

public class CreateUpdateProjectDto
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    [StringLength(SaasSystemConsts.NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(SaasSystemConsts.DescriptionMaxLength)]
    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

    public DateTime? EndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Budget { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;
}

public interface IProjectAppService : IApplicationService
{
    Task<ProjectDto> GetAsync(Guid id);

    Task<PagedResultDto<ProjectDto>> GetListAsync(ProjectListRequestDto input);

    Task<ProjectDto> CreateAsync(CreateUpdateProjectDto input);

    Task<ProjectDto> UpdateAsync(Guid id, CreateUpdateProjectDto input);

    Task DeleteAsync(Guid id);
}
