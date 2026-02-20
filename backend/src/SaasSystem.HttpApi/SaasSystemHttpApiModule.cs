using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.TenantManagement;

namespace SaasSystem;

[DependsOn(
    typeof(SaasSystemApplicationContractsModule),
    typeof(Volo.Abp.AspNetCore.Mvc.AbpAspNetCoreMvcModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpTenantManagementHttpApiModule))]
public class SaasSystemHttpApiModule : AbpModule
{
}
