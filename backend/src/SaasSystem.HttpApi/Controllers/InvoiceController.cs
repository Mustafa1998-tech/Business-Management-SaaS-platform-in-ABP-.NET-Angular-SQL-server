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

    [HttpGet("{id:guid}")]
    public async Task<InvoiceDto> GetAsync(Guid id)
    {
        return await _service.GetAsync(id);
    }

    [HttpPost]
    public async Task<InvoiceDto> CreateAsync([FromBody] CreateUpdateInvoiceDto input)
    {
        return await _service.CreateAsync(input);
    }

    [HttpPut("{id:guid}")]
    public async Task<InvoiceDto> UpdateAsync(Guid id, [FromBody] CreateUpdateInvoiceDto input)
    {
        return await _service.UpdateAsync(id, input);
    }

    [HttpDelete("{id:guid}")]
    public async Task DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
    }
}
