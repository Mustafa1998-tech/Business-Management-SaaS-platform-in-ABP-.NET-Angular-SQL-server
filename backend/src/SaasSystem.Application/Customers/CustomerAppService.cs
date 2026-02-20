using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using SaasSystem.Domain.Entities;
using SaasSystem.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SaasSystem.Customers;

[Authorize(SaasSystemPermissions.Customers.Default)]
public class CustomerAppService : ICustomerAppService
{
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;

    public CustomerAppService(
        IRepository<Customer, Guid> customerRepository,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator)
    {
        _customerRepository = customerRepository;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
    }

    public async Task<CustomerDto> GetAsync(Guid id)
    {
        Customer entity = await _customerRepository.GetAsync(id);
        return Map(entity);
    }

    public async Task<PagedResultDto<CustomerDto>> GetListAsync(CustomerListRequestDto input)
    {
        IQueryable<Customer> queryable = await _customerRepository.GetQueryableAsync();

        queryable = queryable
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Name.Contains(input.Filter!) || x.Email.Contains(input.Filter!))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);

        long totalCount = queryable.LongCount();

        List<CustomerDto> items = queryable
            .OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? nameof(Customer.Name) : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .Select(Map)
            .ToList();

        return new PagedResultDto<CustomerDto>(totalCount, items);
    }

    [Authorize(SaasSystemPermissions.Customers.Create)]
    public async Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input)
    {
        Customer entity = new(
            _guidGenerator.Create(),
            _currentTenant.Id,
            input.Name,
            input.Email,
            input.Phone,
            input.Address);

        entity.SetActive(input.IsActive);

        Customer created = await _customerRepository.InsertAsync(entity, autoSave: true);
        return Map(created);
    }

    [Authorize(SaasSystemPermissions.Customers.Update)]
    public async Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input)
    {
        Customer entity = await _customerRepository.GetAsync(id);

        entity.SetName(input.Name);
        entity.SetEmail(input.Email);
        entity.SetPhone(input.Phone);
        entity.SetAddress(input.Address);
        entity.SetActive(input.IsActive);

        Customer updated = await _customerRepository.UpdateAsync(entity, autoSave: true);
        return Map(updated);
    }

    [Authorize(SaasSystemPermissions.Customers.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _customerRepository.DeleteAsync(id);
    }

    public async Task<PagedResultDto<CustomerDto>> GetRecentAsync(int take)
    {
        IQueryable<Customer> queryable = await _customerRepository.GetQueryableAsync();

        List<CustomerDto> items = queryable
            .OrderByDescending(x => x.CreationTime)
            .Take(take)
            .Select(Map)
            .ToList();

        return new PagedResultDto<CustomerDto>(items.Count, items);
    }

    private static CustomerDto Map(Customer entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Email = entity.Email,
            Phone = entity.Phone,
            Address = entity.Address,
            IsActive = entity.IsActive,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId,
            DeleterId = entity.DeleterId,
            DeletionTime = entity.DeletionTime,
            IsDeleted = entity.IsDeleted
        };
    }
}
