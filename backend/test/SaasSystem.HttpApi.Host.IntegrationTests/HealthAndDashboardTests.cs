using FluentAssertions;
using Volo.Abp.Authorization;

namespace SaasSystem.HttpApi.Host.IntegrationTests;

public class HealthAndDashboardTests : IClassFixture<SaasSystemWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthAndDashboardTests(SaasSystemWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Endpoint_Should_Return_Success()
    {
        HttpResponseMessage response = await _client.GetAsync("/health");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Dashboard_Stats_Should_Require_Authorization()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/app/dashboard/stats");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Reports_Export_Should_Require_Authorization()
    {
        HttpResponseMessage? response = null;
        Exception? exception = await Record.ExceptionAsync(async () =>
        {
            response = await _client.GetAsync("/api/app/reports/invoices-pdf");
        });

        if (exception is null)
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().BeOneOf(
                System.Net.HttpStatusCode.Unauthorized,
                System.Net.HttpStatusCode.Forbidden
            );
            return;
        }

        exception.Should().BeOfType<AbpAuthorizationException>();
    }
}
