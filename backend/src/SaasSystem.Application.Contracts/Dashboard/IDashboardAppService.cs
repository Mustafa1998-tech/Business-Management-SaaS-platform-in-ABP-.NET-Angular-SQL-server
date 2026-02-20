using Volo.Abp.Application.Services;

namespace SaasSystem.Dashboard;

public interface IDashboardAppService : IApplicationService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
