using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using SaasSystem.Domain.Entities;
using SaasSystem.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SaasSystem.Invoices;

[Authorize(SaasSystemPermissions.Invoices.Default)]
public class InvoiceAppService : ApplicationService, IInvoiceAppService
{
    private readonly IRepository<Invoice, Guid> _repository;

    public InvoiceAppService(IRepository<Invoice, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<InvoiceDto>> GetListAsync(InvoiceListRequestDto input)
    {
        IQueryable<Invoice> queryable = await _repository.GetQueryableAsync();

        if (input.CustomerId.HasValue)
        {
            queryable = queryable.Where(x => x.CustomerId == input.CustomerId);
        }

        long totalCount = queryable.LongCount();

        List<InvoiceDto> items = queryable
            .OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? nameof(Invoice.IssueDate) + " desc" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .Select(x => new InvoiceDto
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                ProjectId = x.ProjectId,
                InvoiceNumber = x.InvoiceNumber,
                IssueDate = x.IssueDate,
                DueDate = x.DueDate,
                TotalAmount = x.TotalAmount,
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

        return new PagedResultDto<InvoiceDto>(totalCount, items);
    }
}
