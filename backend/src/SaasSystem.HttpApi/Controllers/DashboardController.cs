using Microsoft.AspNetCore.Mvc;
using SaasSystem.Dashboard;

namespace SaasSystem.Controllers;

[ApiController]
[Route("api/app/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardAppService _dashboardAppService;

    public DashboardController(IDashboardAppService dashboardAppService)
    {
        _dashboardAppService = dashboardAppService;
    }

    [HttpGet("stats")]
    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        return await _dashboardAppService.GetStatsAsync();
    }
}
