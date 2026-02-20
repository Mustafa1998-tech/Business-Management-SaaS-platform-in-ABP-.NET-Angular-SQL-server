using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;

namespace SaasSystem;

[DependsOn(
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpOpenIddictDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpTenantManagementDomainSharedModule))]
public class SaasSystemDomainSharedModule : AbpModule
{
}
