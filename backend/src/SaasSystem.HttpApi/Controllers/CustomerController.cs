using Microsoft.AspNetCore.Mvc;
using SaasSystem.Customers;
using Volo.Abp.Application.Dtos;

namespace SaasSystem.Controllers;

[ApiController]
[Route("api/app/customers")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerAppService _customerAppService;

    public CustomerController(ICustomerAppService customerAppService)
    {
        _customerAppService = customerAppService;
    }

    [HttpGet]
    public async Task<PagedResultDto<CustomerDto>> GetListAsync([FromQuery] CustomerListRequestDto input)
    {
        return await _customerAppService.GetListAsync(input);
    }

    [HttpGet("{id:guid}")]
    public async Task<CustomerDto> GetAsync(Guid id)
    {
        return await _customerAppService.GetAsync(id);
    }

    [HttpPost]
    public async Task<CustomerDto> CreateAsync([FromBody] CreateUpdateCustomerDto input)
    {
        return await _customerAppService.CreateAsync(input);
    }

    [HttpPut("{id:guid}")]
    public async Task<CustomerDto> UpdateAsync(Guid id, [FromBody] CreateUpdateCustomerDto input)
    {
        return await _customerAppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id:guid}")]
    public async Task DeleteAsync(Guid id)
    {
        await _customerAppService.DeleteAsync(id);
    }

    [HttpGet("recent")]
    public async Task<PagedResultDto<CustomerDto>> GetRecentAsync([FromQuery] int take = 5)
    {
        return await _customerAppService.GetRecentAsync(take);
    }
}
