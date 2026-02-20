using SaasSystem.Configuration;
using Serilog;
using Volo.Abp;

namespace SaasSystem;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        DotEnvLoader.LoadFromCurrentDirectory();

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();
            builder.Host.UseAutofac();

            await builder.AddApplicationAsync<SaasSystemHttpApiHostModule>();

            WebApplication app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();

            return 0;
        }
        catch (HostAbortedException)
        {
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
