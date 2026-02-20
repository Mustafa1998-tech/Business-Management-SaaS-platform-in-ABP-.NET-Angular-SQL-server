using Microsoft.Extensions.Configuration;

namespace SaasSystem.EntityFrameworkCore.EntityFrameworkCore;

public static class SaasSystemDbContextFactory
{
    public static string ResolveConnectionString()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SaasSystem.HttpApi.Host"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        return configuration.GetConnectionString("Default")
               ?? throw new InvalidOperationException("Connection string 'Default' was not found.");
    }
}
