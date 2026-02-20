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
}
