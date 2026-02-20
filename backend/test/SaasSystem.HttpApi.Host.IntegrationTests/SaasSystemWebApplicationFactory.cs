using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaasSystem.EntityFrameworkCore.EntityFrameworkCore;

namespace SaasSystem.HttpApi.Host.IntegrationTests;

public class SaasSystemWebApplicationFactory : WebApplicationFactory<SaasSystem.SaasSystemHostMarker>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<SaasSystemDbContext>>();
            services.RemoveAll<SaasSystemDbContext>();

            services.AddDbContext<SaasSystemDbContext>(options =>
            {
                options.UseInMemoryDatabase("SaasSystemIntegrationTests");
            });
        });
    }
}
