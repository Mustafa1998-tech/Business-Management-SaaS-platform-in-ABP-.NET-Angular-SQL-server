using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Prometheus;
using SaasSystem.EntityFrameworkCore;
using SaasSystem.EntityFrameworkCore.EntityFrameworkCore;
using SaasSystem.EntityFrameworkCore.Seed;
using SaasSystem.Middleware;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;

namespace SaasSystem;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpIdentityAspNetCoreModule),
    typeof(AbpOpenIddictAspNetCoreModule),
    typeof(SaasSystemHttpApiModule),
    typeof(SaasSystemApplicationModule),
    typeof(SaasSystemEntityFrameworkCoreModule),
    typeof(Volo.Abp.AspNetCore.MultiTenancy.AbpAspNetCoreMultiTenancyModule),
    typeof(Volo.Abp.AspNetCore.Serilog.AbpAspNetCoreSerilogModule))]
public class SaasSystemHttpApiHostModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        if (context.Services.GetHostingEnvironment().IsDevelopment())
        {
            PreConfigure<OpenIddictServerBuilder>(builder =>
            {
                builder.UseAspNetCore().DisableTransportSecurityRequirement();
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        IConfiguration configuration = context.Services.GetConfiguration();

        context.Services.AddControllers();
        context.Services.AddEndpointsApiExplorer();
        context.Services.AddSwaggerGen();
        context.Services.AddHealthChecks();

        context.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 120,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));
        });

        context.Services.AddSingleton<SecurityHeadersMiddleware>();
        context.Services.AddSingleton<CorrelationIdMiddleware>();

        context.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });
        context.Services.AddAuthorization();

        context.Services.AddOpenIddict()
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                string[] origins = configuration["App:CorsOrigins"]?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    ?? Array.Empty<string>();

                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        IApplicationBuilder app = context.GetApplicationBuilder();
        IWebHostEnvironment environment = context.GetEnvironment();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors();
        app.UseRateLimiter();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHttpMetrics();

        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAbpRequestLocalization();

        app.UseConfiguredEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health");
            endpoints.MapMetrics();
        });

        if (!environment.IsEnvironment("Test"))
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            SaasSystemDbContext dbContext = scope.ServiceProvider.GetRequiredService<SaasSystemDbContext>();
            await dbContext.Database.MigrateAsync();

            IDataSeeder dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            await dataSeeder.SeedAsync();

            SaasSystemDataSeeder seeder = scope.ServiceProvider.GetRequiredService<SaasSystemDataSeeder>();
            await seeder.SeedAsync();
        }
    }
}
