using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using SaasSystem.Domain.Entities;
using SaasSystem.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SaasSystem.Payments;

[Authorize(SaasSystemPermissions.Payments.Default)]
public class PaymentAppService : ApplicationService, IPaymentAppService
{
    private readonly IRepository<Payment, Guid> _repository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public PaymentAppService(
        IRepository<Payment, Guid> repository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    public async Task<PaymentDto> GetAsync(Guid id)
    {
        Payment entity = await _repository.GetAsync(id);
        return Map(entity);
    }

    public async Task<PagedResultDto<PaymentDto>> GetListAsync(PaymentListRequestDto input)
    {
        IQueryable<Payment> queryable = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x => x.ReferenceNumber.Contains(input.Filter));
        }

        if (input.InvoiceId.HasValue)
        {
            queryable = queryable.Where(x => x.InvoiceId == input.InvoiceId);
        }

        if (input.Method.HasValue)
        {
            queryable = queryable.Where(x => x.Method == input.Method.Value);
        }

        long totalCount = queryable.LongCount();

        List<PaymentDto> items = queryable
            .OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? nameof(Payment.PaidAt) + " desc" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .Select(Map)
            .ToList();

        return new PagedResultDto<PaymentDto>(totalCount, items);
    }

    public async Task<PaymentDto> CreateAsync(CreateUpdatePaymentDto input)
    {
        Payment entity = new(
            _guidGenerator.Create(),
            _currentTenant.Id,
            input.InvoiceId,
            input.Amount,
            input.PaidAt,
            input.Method,
            input.ReferenceNumber);

        Payment created = await _repository.InsertAsync(entity, autoSave: true);
        return Map(created);
    }

    public async Task<PaymentDto> UpdateAsync(Guid id, CreateUpdatePaymentDto input)
    {
        Payment entity = await _repository.GetAsync(id);
        entity.Update(
            input.InvoiceId,
            input.Amount,
            input.PaidAt,
            input.Method,
            input.ReferenceNumber);

        Payment updated = await _repository.UpdateAsync(entity, autoSave: true);
        return Map(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    private static PaymentDto Map(Payment entity)
    {
        return new PaymentDto
        {
            Id = entity.Id,
            InvoiceId = entity.InvoiceId,
            Amount = entity.Amount,
            PaidAt = entity.PaidAt,
            Method = entity.Method,
            ReferenceNumber = entity.ReferenceNumber,
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
