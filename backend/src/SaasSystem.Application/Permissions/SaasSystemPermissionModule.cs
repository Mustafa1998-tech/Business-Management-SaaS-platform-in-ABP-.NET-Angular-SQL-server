using SaasSystem.Permissions;
using Volo.Abp.Modularity;

namespace SaasSystem.Permissions;

[DependsOn(typeof(SaasSystemApplicationContractsModule))]
public class SaasSystemPermissionModule : AbpModule
{
}
