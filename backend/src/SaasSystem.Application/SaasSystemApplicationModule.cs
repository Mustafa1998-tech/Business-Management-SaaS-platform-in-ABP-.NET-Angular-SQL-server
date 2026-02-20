using SaasSystem.Domain;
using Volo.Abp.Account;
using Volo.Abp.Modularity;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;

namespace SaasSystem;

[DependsOn(
    typeof(SaasSystemApplicationContractsModule),
    typeof(SaasSystemDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(Volo.Abp.AutoMapper.AbpAutoMapperModule),
    typeof(Volo.Abp.Caching.AbpCachingModule))]
public class SaasSystemApplicationModule : AbpModule
{
    public override void ConfigureServices(Volo.Abp.Modularity.ServiceConfigurationContext context)
    {
        Configure<Volo.Abp.AutoMapper.AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<SaasSystemApplicationModule>(validate: true);
        });
    }
}
