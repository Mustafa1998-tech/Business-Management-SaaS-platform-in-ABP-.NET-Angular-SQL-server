using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using SaasSystem.Domain.Entities;
using SaasSystem.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SaasSystem.Invoices;

[Authorize(SaasSystemPermissions.Invoices.Default)]
public class InvoiceAppService : ApplicationService, IInvoiceAppService
{
    private readonly IRepository<Invoice, Guid> _repository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public InvoiceAppService(
        IRepository<Invoice, Guid> repository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    public async Task<InvoiceDto> GetAsync(Guid id)
    {
        Invoice entity = await _repository.GetAsync(id);
        return Map(entity);
    }

    public async Task<PagedResultDto<InvoiceDto>> GetListAsync(InvoiceListRequestDto input)
    {
        IQueryable<Invoice> queryable = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x => x.InvoiceNumber.Contains(input.Filter));
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

        List<InvoiceDto> items = queryable
            .OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? nameof(Invoice.IssueDate) + " desc" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .Select(Map)
            .ToList();

        return new PagedResultDto<InvoiceDto>(totalCount, items);
    }

    public async Task<InvoiceDto> CreateAsync(CreateUpdateInvoiceDto input)
    {
        Invoice entity = new(
            _guidGenerator.Create(),
            _currentTenant.Id,
            input.CustomerId,
            input.ProjectId,
            input.InvoiceNumber,
            input.IssueDate,
            input.DueDate,
            input.SubTotal,
            input.TaxAmount);

        entity.ChangeStatus(input.Status);

        Invoice created = await _repository.InsertAsync(entity, autoSave: true);
        return Map(created);
    }

    public async Task<InvoiceDto> UpdateAsync(Guid id, CreateUpdateInvoiceDto input)
    {
        Invoice entity = await _repository.GetAsync(id);

        entity.Update(
            input.CustomerId,
            input.ProjectId,
            input.InvoiceNumber,
            input.IssueDate,
            input.DueDate,
            input.SubTotal,
            input.TaxAmount);

        entity.ChangeStatus(input.Status);

        Invoice updated = await _repository.UpdateAsync(entity, autoSave: true);
        return Map(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    private static InvoiceDto Map(Invoice entity)
    {
        return new InvoiceDto
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            ProjectId = entity.ProjectId,
            InvoiceNumber = entity.InvoiceNumber,
            IssueDate = entity.IssueDate,
            DueDate = entity.DueDate,
            TotalAmount = entity.TotalAmount,
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
