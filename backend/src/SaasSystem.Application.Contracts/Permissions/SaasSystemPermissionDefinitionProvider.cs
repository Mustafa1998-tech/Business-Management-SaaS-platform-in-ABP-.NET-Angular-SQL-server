using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace SaasSystem.Permissions;

public class SaasSystemPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(SaasSystemPermissions.GroupName, L("Permission:SaasSystem"));

        group.AddPermission(SaasSystemPermissions.Dashboard.Default, L("Permission:Dashboard"));
        group.AddPermission(SaasSystemPermissions.Users.Default, L("Permission:Users"));
        group.AddPermission(SaasSystemPermissions.Orders.Default, L("Permission:Orders"));
        group.AddPermission(SaasSystemPermissions.Settings.Default, L("Permission:Settings"));

        var customers = group.AddPermission(SaasSystemPermissions.Customers.Default, L("Permission:Customers"));
        customers.AddChild(SaasSystemPermissions.Customers.Create, L("Permission:Create"));
        customers.AddChild(SaasSystemPermissions.Customers.Update, L("Permission:Edit"));
        customers.AddChild(SaasSystemPermissions.Customers.Delete, L("Permission:Delete"));

        group.AddPermission(SaasSystemPermissions.Projects.Default, L("Permission:Projects"));

        var tasks = group.AddPermission(SaasSystemPermissions.Tasks.Default, L("Permission:Tasks"));
        tasks.AddChild(SaasSystemPermissions.Tasks.Move, L("Permission:MoveTask"));

        group.AddPermission(SaasSystemPermissions.Invoices.Default, L("Permission:Invoices"));
        group.AddPermission(SaasSystemPermissions.Payments.Default, L("Permission:Payments"));
        group.AddPermission(SaasSystemPermissions.Reports.Default, L("Permission:Reports"));
        group.AddPermission(SaasSystemPermissions.Administration.Default, L("Permission:Administration"), multiTenancySide: MultiTenancySides.Host);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SaasSystemResource>(name);
    }
}
