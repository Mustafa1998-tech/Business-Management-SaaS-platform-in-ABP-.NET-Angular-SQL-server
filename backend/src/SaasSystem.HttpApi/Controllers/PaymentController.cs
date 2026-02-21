using Microsoft.AspNetCore.Mvc;
using SaasSystem.Payments;
using Volo.Abp.Application.Dtos;

namespace SaasSystem.Controllers;

[ApiController]
[Route("api/app/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentAppService _service;

    public PaymentController(IPaymentAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<PagedResultDto<PaymentDto>> GetListAsync([FromQuery] PaymentListRequestDto input)
    {
        return await _service.GetListAsync(input);
    }

    [HttpGet("{id:guid}")]
    public async Task<PaymentDto> GetAsync(Guid id)
    {
        return await _service.GetAsync(id);
    }

    [HttpPost]
    public async Task<PaymentDto> CreateAsync([FromBody] CreateUpdatePaymentDto input)
    {
        return await _service.CreateAsync(input);
    }

    [HttpPut("{id:guid}")]
    public async Task<PaymentDto> UpdateAsync(Guid id, [FromBody] CreateUpdatePaymentDto input)
    {
        return await _service.UpdateAsync(id, input);
    }

    [HttpDelete("{id:guid}")]
    public async Task DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
    }
}
