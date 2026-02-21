using Microsoft.AspNetCore.Mvc;
using SaasSystem.Projects;
using Volo.Abp.Application.Dtos;

namespace SaasSystem.Controllers;

[ApiController]
[Route("api/app/projects")]
public class ProjectController : ControllerBase
{
    private readonly IProjectAppService _service;

    public ProjectController(IProjectAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<PagedResultDto<ProjectDto>> GetListAsync([FromQuery] ProjectListRequestDto input)
    {
        return await _service.GetListAsync(input);
    }

    [HttpGet("{id:guid}")]
    public async Task<ProjectDto> GetAsync(Guid id)
    {
        return await _service.GetAsync(id);
    }

    [HttpPost]
    public async Task<ProjectDto> CreateAsync([FromBody] CreateUpdateProjectDto input)
    {
        return await _service.CreateAsync(input);
    }

    [HttpPut("{id:guid}")]
    public async Task<ProjectDto> UpdateAsync(Guid id, [FromBody] CreateUpdateProjectDto input)
    {
        return await _service.UpdateAsync(id, input);
    }

    [HttpDelete("{id:guid}")]
    public async Task DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
    }
}
