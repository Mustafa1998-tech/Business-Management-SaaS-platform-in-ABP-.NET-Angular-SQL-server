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
}
