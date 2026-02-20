using SaasSystem.Domain.Entities;
using Volo.Abp.Identity;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.Identity;
using Volo.Abp.PermissionManagement.OpenIddict;
using Volo.Abp.TenantManagement;

namespace SaasSystem.Domain;

[DependsOn(
    typeof(SaasSystemDomainSharedModule),
    typeof(AbpDddDomainModule),
    typeof(AbpIdentityDomainModule),
    typeof(AbpOpenIddictDomainModule),
    typeof(AbpPermissionManagementDomainModule),
    typeof(AbpPermissionManagementDomainIdentityModule),
    typeof(AbpPermissionManagementDomainOpenIddictModule),
    typeof(AbpTenantManagementDomainModule))]
public class SaasSystemDomainModule : AbpModule
{
}
