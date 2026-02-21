using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace SaasSystem.EntityFrameworkCore.Seed;

public class SaasSystemIdentityDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string DefaultPasswordEnvironmentKey = "SAASSYSTEM_DEFAULT_PASSWORD";
    private const string RolePermissionProviderName = "R";
    private const string AdminRoleName = "admin";
    private const string UserRoleName = "user";
    private const string AdminUserName = "admin";
    private const string StandardUserName = "user";
    private const string AdminEmail = "admin@saassystem.local";
    private const string StandardUserEmail = "user@saassystem.local";

    private static readonly string[] AdminPermissions =
    {
        "SaasSystem.Dashboard",
        "SaasSystem.Users",
        "SaasSystem.Orders",
        "SaasSystem.Settings",
        "SaasSystem.Customers",
        "SaasSystem.Customers.Create",
        "SaasSystem.Customers.Update",
        "SaasSystem.Customers.Delete",
        "SaasSystem.Projects",
        "SaasSystem.Tasks",
        "SaasSystem.Tasks.Move",
        "SaasSystem.Invoices",
        "SaasSystem.Payments",
        "SaasSystem.Reports",
        "SaasSystem.Administration"
    };

    private static readonly string[] UserPermissions =
    {
        "SaasSystem.Dashboard",
        "SaasSystem.Customers",
        "SaasSystem.Projects",
        "SaasSystem.Tasks",
        "SaasSystem.Invoices",
        "SaasSystem.Payments"
    };

    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly IdentityUserManager _userManager;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ILogger<SaasSystemIdentityDataSeedContributor> _logger;

    public SaasSystemIdentityDataSeedContributor(
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator,
        IIdentityUserRepository identityUserRepository,
        IdentityRoleManager roleManager,
        IdentityUserManager userManager,
        IPermissionDataSeeder permissionDataSeeder,
        IUnitOfWorkManager unitOfWorkManager,
        ILogger<SaasSystemIdentityDataSeedContributor> logger)
    {
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _identityUserRepository = identityUserRepository;
        _roleManager = roleManager;
        _userManager = userManager;
        _permissionDataSeeder = permissionDataSeeder;
        _unitOfWorkManager = unitOfWorkManager;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        IEnumerable<Guid?> targetTenants = context.TenantId.HasValue
            ? new[] { context.TenantId }
            : SaasSystemSeedTenants.WithHost();

        foreach (Guid? tenantId in targetTenants)
        {
            using (_currentTenant.Change(tenantId))
            using (IUnitOfWork uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                await SeedTenantAsync(tenantId);
                await uow.CompleteAsync();
            }
        }
    }

    private async Task SeedTenantAsync(Guid? tenantId)
    {
        await EnsureRoleAsync(AdminRoleName, tenantId);
        await EnsureRoleAsync(UserRoleName, tenantId);

        await _permissionDataSeeder.SeedAsync(
            RolePermissionProviderName,
            AdminRoleName,
            AdminPermissions,
            tenantId);

        await _permissionDataSeeder.SeedAsync(
            RolePermissionProviderName,
            UserRoleName,
            UserPermissions,
            tenantId);

        IdentityUser adminUser = await EnsureUserAsync(AdminUserName, AdminEmail, tenantId);
        IdentityUser userUser = await EnsureUserAsync(StandardUserName, StandardUserEmail, tenantId);

        await EnsureRoleAssignmentAsync(adminUser, AdminRoleName);
        await EnsureRoleAssignmentAsync(userUser, UserRoleName);
    }

    private async Task EnsureRoleAsync(string roleName, Guid? tenantId)
    {
        if (await _roleManager.FindByNameAsync(roleName) is not null)
        {
            return;
        }

        IdentityRole role = new(_guidGenerator.Create(), roleName, tenantId);
        IdentityResult result = await _roleManager.CreateAsync(role);
        ThrowIfFailed(result, $"Failed to create role '{roleName}' for tenant '{tenantId?.ToString() ?? "host"}'.");
    }

    private async Task<IdentityUser> EnsureUserAsync(string userName, string email, Guid? tenantId)
    {
        List<IdentityUser> existingUsers = (await _identityUserRepository.GetListAsync())
            .Where(x => x.UserName == userName)
            .ToList();

        if (existingUsers.Count > 0)
        {
            foreach (IdentityUser existingUser in existingUsers)
            {
                await EnsurePasswordMatchesEnvironmentAsync(existingUser, tenantId);
            }

            return existingUsers.FirstOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase))
                ?? existingUsers[0];
        }

        string password = ResolveDefaultPassword();
        IdentityUser user = new(_guidGenerator.Create(), userName, email, tenantId);
        IdentityResult createResult = await _userManager.CreateAsync(user, password);
        ThrowIfFailed(createResult, $"Failed to create user '{userName}' for tenant '{tenantId?.ToString() ?? "host"}'.");
        return user;
    }

    private async Task EnsurePasswordMatchesEnvironmentAsync(IdentityUser user, Guid? tenantId)
    {
        string? password = Environment.GetEnvironmentVariable(DefaultPasswordEnvironmentKey);
        if (string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (await _userManager.CheckPasswordAsync(user, password))
        {
            return;
        }

        if (await _userManager.HasPasswordAsync(user))
        {
            IdentityResult removeResult = await _userManager.RemovePasswordAsync(user);
            ThrowIfFailed(
                removeResult,
                $"Failed to remove existing password for user '{user.UserName}' in tenant '{tenantId?.ToString() ?? "host"}'.");
        }

        IdentityResult addResult = await _userManager.AddPasswordAsync(user, password);
        ThrowIfFailed(
            addResult,
            $"Failed to align password for user '{user.UserName}' in tenant '{tenantId?.ToString() ?? "host"}'.");
    }

    private async Task EnsureRoleAssignmentAsync(IdentityUser user, string roleName)
    {
        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            return;
        }

        IdentityResult addToRoleResult = await _userManager.AddToRoleAsync(user, roleName);
        ThrowIfFailed(addToRoleResult, $"Failed to assign role '{roleName}' to user '{user.UserName}'.");
    }

    private static void ThrowIfFailed(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new AbpException($"{message} Errors: {string.Join("; ", result.Errors.Select(x => x.Description))}");
    }

    private string ResolveDefaultPassword()
    {
        string? password = Environment.GetEnvironmentVariable(DefaultPasswordEnvironmentKey);
        if (!string.IsNullOrWhiteSpace(password))
        {
            return password;
        }

        _logger.LogError(
            "Identity seed skipped because {EnvironmentKey} is not set.",
            DefaultPasswordEnvironmentKey);

        throw new AbpException(
            $"Set environment variable '{DefaultPasswordEnvironmentKey}' before starting the host to seed default users.");
    }
}
