using FluentAssertions;

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
        HttpResponseMessage response = await _client.GetAsync("/api/app/reports/invoices-pdf");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
