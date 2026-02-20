using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SaasSystem.Customers;

public interface ICustomerAppService : IApplicationService
{
    Task<CustomerDto> GetAsync(Guid id);
    Task<PagedResultDto<CustomerDto>> GetListAsync(CustomerListRequestDto input);
    Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input);
    Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input);
    Task DeleteAsync(Guid id);
    Task<PagedResultDto<CustomerDto>> GetRecentAsync(int take);
}
