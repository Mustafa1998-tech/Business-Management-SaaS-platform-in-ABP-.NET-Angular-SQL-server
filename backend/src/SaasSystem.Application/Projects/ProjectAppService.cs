using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using SaasSystem.Domain.Entities;
using SaasSystem.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SaasSystem.Projects;

[Authorize(SaasSystemPermissions.Projects.Default)]
public class ProjectAppService : ApplicationService, IProjectAppService
{
    private readonly IRepository<Project, Guid> _repository;

    public ProjectAppService(IRepository<Project, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<ProjectDto>> GetListAsync(ProjectListRequestDto input)
    {
        IQueryable<Project> queryable = await _repository.GetQueryableAsync();

        if (input.CustomerId.HasValue)
        {
            queryable = queryable.Where(x => x.CustomerId == input.CustomerId);
        }

        long totalCount = queryable.LongCount();

        List<ProjectDto> items = queryable
            .OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? nameof(Project.StartDate) + " desc" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .Select(x => new ProjectDto
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Budget = x.Budget,
                Status = x.Status,
                CreationTime = x.CreationTime,
                CreatorId = x.CreatorId,
                LastModificationTime = x.LastModificationTime,
                LastModifierId = x.LastModifierId,
                DeletionTime = x.DeletionTime,
                DeleterId = x.DeleterId,
                IsDeleted = x.IsDeleted
            })
            .ToList();

        return new PagedResultDto<ProjectDto>(totalCount, items);
    }
}
