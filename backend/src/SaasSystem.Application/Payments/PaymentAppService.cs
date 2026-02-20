using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using SaasSystem.Domain.Entities;
using SaasSystem.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace SaasSystem.Payments;

[Authorize(SaasSystemPermissions.Payments.Default)]
public class PaymentAppService : IPaymentAppService
{
    private readonly IRepository<Payment, Guid> _repository;

    public PaymentAppService(IRepository<Payment, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<PaymentDto>> GetListAsync(PaymentListRequestDto input)
    {
        IQueryable<Payment> queryable = await _repository.GetQueryableAsync();

        if (input.InvoiceId.HasValue)
        {
            queryable = queryable.Where(x => x.InvoiceId == input.InvoiceId);
        }

        long totalCount = queryable.LongCount();

        List<PaymentDto> items = queryable
            .OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? nameof(Payment.PaidAt) + " desc" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .Select(x => new PaymentDto
            {
                Id = x.Id,
                InvoiceId = x.InvoiceId,
                Amount = x.Amount,
                PaidAt = x.PaidAt,
                Method = x.Method,
                ReferenceNumber = x.ReferenceNumber,
                CreationTime = x.CreationTime,
                CreatorId = x.CreatorId,
                LastModificationTime = x.LastModificationTime,
                LastModifierId = x.LastModifierId,
                DeletionTime = x.DeletionTime,
                DeleterId = x.DeleterId,
                IsDeleted = x.IsDeleted
            })
            .ToList();

        return new PagedResultDto<PaymentDto>(totalCount, items);
    }
}
