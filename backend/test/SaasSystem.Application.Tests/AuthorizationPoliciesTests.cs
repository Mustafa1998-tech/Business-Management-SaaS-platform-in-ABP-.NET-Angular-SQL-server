using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using SaasSystem.Dashboard;
using SaasSystem.Invoices;
using SaasSystem.Payments;
using SaasSystem.Permissions;
using SaasSystem.Projects;
using SaasSystem.Reports;

namespace SaasSystem.Application.Tests;

public class AuthorizationPoliciesTests
{
    [Fact]
    public void Dashboard_Should_Require_Dashboard_Policy()
    {
        GetClassPolicy<DashboardAppService>().Should().Be(SaasSystemPermissions.Dashboard.Default);
    }

    [Fact]
    public void Projects_Should_Require_Projects_Policy()
    {
        GetClassPolicy<ProjectAppService>().Should().Be(SaasSystemPermissions.Projects.Default);
    }

    [Fact]
    public void Invoices_Should_Require_Invoices_Policy()
    {
        GetClassPolicy<InvoiceAppService>().Should().Be(SaasSystemPermissions.Invoices.Default);
    }

    [Fact]
    public void Payments_Should_Require_Payments_Policy()
    {
        GetClassPolicy<PaymentAppService>().Should().Be(SaasSystemPermissions.Payments.Default);
    }

    [Fact]
    public void Reports_Should_Require_Reports_Policy()
    {
        GetClassPolicy<ReportsAppService>().Should().Be(SaasSystemPermissions.Reports.Default);
    }

    [Fact]
    public void New_Page_Policies_Should_Be_Defined_As_Constants()
    {
        SaasSystemPermissions.Cars.Default.Should().Be("SaasSystem.Cars");
        SaasSystemPermissions.Users.Default.Should().Be("SaasSystem.Users");
        SaasSystemPermissions.Orders.Default.Should().Be("SaasSystem.Orders");
        SaasSystemPermissions.Settings.Default.Should().Be("SaasSystem.Settings");
    }

    private static string? GetClassPolicy<T>()
    {
        return typeof(T)
            .GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault()
            ?.Policy;
    }
}
