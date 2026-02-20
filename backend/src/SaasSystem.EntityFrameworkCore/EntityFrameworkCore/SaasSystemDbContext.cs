using Microsoft.EntityFrameworkCore;
using SaasSystem.Domain.Entities;
using SaasSystem.Domain.MultiTenancy;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.OpenIddict.Applications;
using Volo.Abp.OpenIddict.Authorizations;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.OpenIddict.Scopes;
using Volo.Abp.OpenIddict.Tokens;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace SaasSystem.EntityFrameworkCore.EntityFrameworkCore;

[ConnectionStringName("Default")]
public class SaasSystemDbContext :
    AbpDbContext<SaasSystemDbContext>,
    IIdentityDbContext,
    IOpenIddictDbContext,
    IPermissionManagementDbContext,
    ITenantManagementDbContext
{
    private readonly ICurrentTenant _currentTenant;

    public DbSet<IdentityUser> Users => Set<IdentityUser>();
    public DbSet<IdentityRole> Roles => Set<IdentityRole>();
    public DbSet<IdentityClaimType> ClaimTypes => Set<IdentityClaimType>();
    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
    public DbSet<IdentitySecurityLog> SecurityLogs => Set<IdentitySecurityLog>();
    public DbSet<IdentityLinkUser> LinkUsers => Set<IdentityLinkUser>();
    public DbSet<IdentityUserDelegation> UserDelegations => Set<IdentityUserDelegation>();
    public DbSet<IdentitySession> Sessions => Set<IdentitySession>();

    public DbSet<OpenIddictApplication> Applications => Set<OpenIddictApplication>();
    public DbSet<OpenIddictAuthorization> Authorizations => Set<OpenIddictAuthorization>();
    public DbSet<OpenIddictScope> Scopes => Set<OpenIddictScope>();
    public DbSet<OpenIddictToken> Tokens => Set<OpenIddictToken>();

    public DbSet<PermissionGroupDefinitionRecord> PermissionGroups => Set<PermissionGroupDefinitionRecord>();
    public DbSet<PermissionDefinitionRecord> Permissions => Set<PermissionDefinitionRecord>();
    public DbSet<PermissionGrant> PermissionGrants => Set<PermissionGrant>();

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantConnectionString> TenantConnectionStrings => Set<TenantConnectionString>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<WorkTask> Tasks => Set<WorkTask>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<TenantProfile> TenantProfiles => Set<TenantProfile>();

    public SaasSystemDbContext(DbContextOptions<SaasSystemDbContext> options, ICurrentTenant currentTenant)
        : base(options)
    {
        _currentTenant = currentTenant;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigurePermissionManagement();
        builder.ConfigureTenantManagement();

        builder.Entity<Customer>(b =>
        {
            b.ToTable(SaasSystemConsts.DbTablePrefix + "Customers", SaasSystemConsts.DbSchema);
            b.Property(x => x.Name).IsRequired().HasMaxLength(SaasSystemConsts.NameMaxLength);
            b.Property(x => x.Email).IsRequired().HasMaxLength(SaasSystemConsts.EmailMaxLength);
            b.Property(x => x.Phone).IsRequired().HasMaxLength(SaasSystemConsts.PhoneMaxLength);
            b.Property(x => x.Address).IsRequired().HasMaxLength(SaasSystemConsts.DescriptionMaxLength);
            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
            b.HasQueryFilter(x => !_currentTenant.Id.HasValue || x.TenantId == _currentTenant.Id);
        });

        builder.Entity<Project>(b =>
        {
            b.ToTable(SaasSystemConsts.DbTablePrefix + "Projects", SaasSystemConsts.DbSchema);
            b.Property(x => x.Name).IsRequired().HasMaxLength(SaasSystemConsts.NameMaxLength);
            b.Property(x => x.Description).IsRequired().HasMaxLength(SaasSystemConsts.DescriptionMaxLength);
            b.Property(x => x.Budget).HasPrecision(18, 2);
            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.CustomerId);
            b.HasOne(x => x.Customer).WithMany(x => x.Projects).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            b.HasQueryFilter(x => !_currentTenant.Id.HasValue || x.TenantId == _currentTenant.Id);
        });

        builder.Entity<WorkTask>(b =>
        {
            b.ToTable(SaasSystemConsts.DbTablePrefix + "Tasks", SaasSystemConsts.DbSchema);
            b.Property(x => x.Title).IsRequired().HasMaxLength(SaasSystemConsts.NameMaxLength);
            b.Property(x => x.Description).IsRequired().HasMaxLength(SaasSystemConsts.DescriptionMaxLength);
            b.Property(x => x.EstimatedHours).HasPrecision(10, 2);
            b.Property(x => x.SpentHours).HasPrecision(10, 2);
            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => new { x.ProjectId, x.Status, x.SortOrder });
            b.HasOne(x => x.Project).WithMany(x => x.Tasks).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            b.HasQueryFilter(x => !_currentTenant.Id.HasValue || x.TenantId == _currentTenant.Id);
        });

        builder.Entity<Invoice>(b =>
        {
            b.ToTable(SaasSystemConsts.DbTablePrefix + "Invoices", SaasSystemConsts.DbSchema);
            b.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(SaasSystemConsts.InvoiceNumberMaxLength);
            b.Property(x => x.SubTotal).HasPrecision(18, 2);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2);
            b.Property(x => x.TotalAmount).HasPrecision(18, 2);
            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.CustomerId);
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => x.InvoiceNumber).IsUnique();
            b.HasOne(x => x.Customer).WithMany(x => x.Invoices).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
            b.HasQueryFilter(x => !_currentTenant.Id.HasValue || x.TenantId == _currentTenant.Id);
        });

        builder.Entity<Payment>(b =>
        {
            b.ToTable(SaasSystemConsts.DbTablePrefix + "Payments", SaasSystemConsts.DbSchema);
            b.Property(x => x.ReferenceNumber).IsRequired().HasMaxLength(SaasSystemConsts.NameMaxLength);
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.InvoiceId);
            b.HasOne(x => x.Invoice).WithMany(x => x.Payments).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            b.HasQueryFilter(x => !_currentTenant.Id.HasValue || x.TenantId == _currentTenant.Id);
        });

        builder.Entity<TenantProfile>(b =>
        {
            b.ToTable(SaasSystemConsts.DbTablePrefix + "TenantProfiles", SaasSystemConsts.DbSchema);
            b.Property(x => x.Name).IsRequired().HasMaxLength(SaasSystemConsts.NameMaxLength);
            b.Property(x => x.Edition).IsRequired().HasMaxLength(SaasSystemConsts.NameMaxLength);
            b.HasIndex(x => x.TenantId);
            b.HasQueryFilter(x => !_currentTenant.Id.HasValue || x.TenantId == _currentTenant.Id);
        });
    }
}
