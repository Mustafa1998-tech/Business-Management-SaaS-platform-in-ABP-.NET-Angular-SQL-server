using Microsoft.AspNetCore.Mvc;
using SaasSystem.Invoices;
using Volo.Abp.Application.Dtos;

namespace SaasSystem.Controllers;

[ApiController]
[Route("api/app/invoices")]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceAppService _service;

    public InvoiceController(IInvoiceAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<PagedResultDto<InvoiceDto>> GetListAsync([FromQuery] InvoiceListRequestDto input)
    {
        return await _service.GetListAsync(input);
    }
}
