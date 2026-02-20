using SaasSystem.Configuration;
using SaasSystem.EntityFrameworkCore;
using SaasSystem.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace SaasSystem;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SaasSystemApplicationModule),
    typeof(SaasSystemEntityFrameworkCoreModule)
)]
public class SaasSystemDbMigratorModule : AbpModule
{
}

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        DotEnvLoader.LoadFromCurrentDirectory();

        using var application = await AbpApplicationFactory.CreateAsync<SaasSystemDbMigratorModule>(options =>
        {
            options.UseAutofac();
        });

        await application.InitializeAsync();

        try
        {
            using var scope = application.ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SaasSystemDbContext>();
            var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();

            await dbContext.Database.MigrateAsync();
            await seeder.SeedAsync();

            Console.WriteLine("Database migration and seed completed.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
        finally
        {
            await application.ShutdownAsync();
        }
    }
}
