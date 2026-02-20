using Microsoft.Extensions.Configuration;
using OpenIddict.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SaasSystem.EntityFrameworkCore.Seed;

public class SaasSystemOpenIddictDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string AngularClientFallbackId = "SaasSystem_Angular";
    private const string AngularRootUrlFallback = "http://localhost:4200";
    private const string ApiScopeName = "SaasSystem";

    private readonly ICurrentTenant _currentTenant;
    private readonly IConfiguration _configuration;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;

    public SaasSystemOpenIddictDataSeedContributor(
        ICurrentTenant currentTenant,
        IConfiguration configuration,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager)
    {
        _currentTenant = currentTenant;
        _configuration = configuration;
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId.HasValue)
        {
            return;
        }

        using (_currentTenant.Change(null))
        {
            await EnsureScopeAsync();
            await EnsureAngularClientAsync();
        }
    }

    private async Task EnsureScopeAsync()
    {
        if (await _scopeManager.FindByNameAsync(ApiScopeName) is not null)
        {
            return;
        }

        OpenIddictScopeDescriptor descriptor = new()
        {
            Name = ApiScopeName,
            DisplayName = "SaaS System API"
        };
        descriptor.Resources.Add(ApiScopeName);

        await _scopeManager.CreateAsync(descriptor);
    }

    private async Task EnsureAngularClientAsync()
    {
        string clientId = _configuration["OpenIddict:Applications:Angular:ClientId"] ?? AngularClientFallbackId;
        string rootUrl = _configuration["OpenIddict:Applications:Angular:RootUrl"] ?? AngularRootUrlFallback;

        OpenIddictApplicationDescriptor descriptor = new()
        {
            ClientId = clientId,
            DisplayName = "SaaS System Angular",
            ClientType = OpenIddictConstants.ClientTypes.Public,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit
        };

        descriptor.Permissions.UnionWith(
        [
            OpenIddictConstants.Permissions.Endpoints.Authorization,
            OpenIddictConstants.Permissions.Endpoints.EndSession,
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.Endpoints.Revocation,
            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
            OpenIddictConstants.Permissions.GrantTypes.Password,
            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
            OpenIddictConstants.Permissions.ResponseTypes.Code,
            OpenIddictConstants.Permissions.ResponseTypes.Token,
            OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Phone,
            OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Roles,
            OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Address,
            OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OfflineAccess,
            OpenIddictConstants.Permissions.Prefixes.Scope + ApiScopeName
        ]);

        if (Uri.TryCreate(rootUrl, UriKind.Absolute, out Uri? rootUri))
        {
            descriptor.RedirectUris.Add(rootUri);
            descriptor.PostLogoutRedirectUris.Add(rootUri);
        }

        object? application = await _applicationManager.FindByClientIdAsync(clientId);
        if (application is null)
        {
            await _applicationManager.CreateAsync(descriptor);
            return;
        }

        await _applicationManager.UpdateAsync(application, descriptor);
    }
}
