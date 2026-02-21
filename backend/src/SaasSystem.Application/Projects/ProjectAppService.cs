using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using SaasSystem.Domain.Entities;
using SaasSystem.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SaasSystem.Projects;

[Authorize(SaasSystemPermissions.Projects.Default)]
public class ProjectAppService : ApplicationService, IProjectAppService
{
    private readonly IRepository<Project, Guid> _repository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public ProjectAppService(
        IRepository<Project, Guid> repository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    public async Task<ProjectDto> GetAsync(Guid id)
    {
        Project entity = await _repository.GetAsync(id);
        return Map(entity);
    }

    public async Task<PagedResultDto<ProjectDto>> GetListAsync(ProjectListRequestDto input)
    {
        IQueryable<Project> queryable = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x =>
                x.Name.Contains(input.Filter) ||
                x.Description.Contains(input.Filter));
        }

        if (input.CustomerId.HasValue)
        {
            queryable = queryable.Where(x => x.CustomerId == input.CustomerId);
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        long totalCount = queryable.LongCount();

        List<ProjectDto> items = queryable
            .OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? nameof(Project.StartDate) + " desc" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .Select(Map)
            .ToList();

        return new PagedResultDto<ProjectDto>(totalCount, items);
    }

    public async Task<ProjectDto> CreateAsync(CreateUpdateProjectDto input)
    {
        Project entity = new(
            _guidGenerator.Create(),
            _currentTenant.Id,
            input.CustomerId,
            input.Name,
            input.Description,
            input.StartDate,
            input.Budget);

        entity.ChangeStatus(input.Status);
        entity.SetEndDate(input.EndDate);

        Project created = await _repository.InsertAsync(entity, autoSave: true);
        return Map(created);
    }

    public async Task<ProjectDto> UpdateAsync(Guid id, CreateUpdateProjectDto input)
    {
        Project entity = await _repository.GetAsync(id);

        entity.Update(
            input.CustomerId,
            input.Name,
            input.Description,
            input.StartDate,
            input.Budget);

        entity.ChangeStatus(input.Status);
        entity.SetEndDate(input.EndDate);

        Project updated = await _repository.UpdateAsync(entity, autoSave: true);
        return Map(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    private static ProjectDto Map(Project entity)
    {
        return new ProjectDto
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            Name = entity.Name,
            Description = entity.Description,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Budget = entity.Budget,
            Status = entity.Status,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId,
            DeletionTime = entity.DeletionTime,
            DeleterId = entity.DeleterId,
            IsDeleted = entity.IsDeleted
        };
    }
}
