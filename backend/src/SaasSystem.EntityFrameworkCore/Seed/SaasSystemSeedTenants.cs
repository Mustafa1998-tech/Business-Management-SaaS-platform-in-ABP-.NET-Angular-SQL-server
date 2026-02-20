namespace SaasSystem.EntityFrameworkCore.Seed;

public static class SaasSystemSeedTenants
{
    public static readonly Guid[] TenantIds =
    {
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Guid.Parse("33333333-3333-3333-3333-333333333333")
    };

    public static IEnumerable<Guid?> WithHost()
    {
        yield return null;

        foreach (Guid tenantId in TenantIds)
        {
            yield return tenantId;
        }
    }
}
