using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;

namespace SaasSystem;

[DependsOn(
    typeof(SaasSystemDomainSharedModule),
    typeof(AbpAccountApplicationContractsModule),
    typeof(AbpIdentityApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationContractsModule),
    typeof(AbpTenantManagementApplicationContractsModule))]
public class SaasSystemApplicationContractsModule : AbpModule
{
}
