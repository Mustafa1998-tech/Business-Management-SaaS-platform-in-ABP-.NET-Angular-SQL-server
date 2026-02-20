using SaasSystem.Domain;
using Microsoft.Extensions.DependencyInjection;
using SaasSystem.EntityFrameworkCore.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace SaasSystem.EntityFrameworkCore;

[DependsOn(
    typeof(SaasSystemDomainModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule))]
public class SaasSystemEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(Volo.Abp.Modularity.ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<SaasSystemDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.ReplaceDbContext<IIdentityDbContext>();
            options.ReplaceDbContext<IOpenIddictDbContext>();
            options.ReplaceDbContext<IPermissionManagementDbContext>();
            options.ReplaceDbContext<ITenantManagementDbContext>();
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseSqlServer();
        });
    }
}
