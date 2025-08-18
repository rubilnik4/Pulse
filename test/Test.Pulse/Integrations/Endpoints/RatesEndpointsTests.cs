using System.Net;
using System.Net.Http.Json;
using Pulse.Api.Contracts.Responses;
using Shouldly;
using Test.Pulse.Integrations.Startup;

namespace Test.Pulse.Integrations.Endpoints;

public class RatesEndpointsTests
{
    private CustomWebApplicationFactory _factory = null!;
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = await CustomWebApplicationFactory.StartNewAsync();
    }
    
    [Test]
    public async Task GetRates_WithUsdEur_Should_Return_200_And_Items()
    {
        using var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/rates?codes=USD,EUR");
        
        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var payload = await resp.Content.ReadFromJsonAsync<RatesResponse>();
        payload.ShouldNotBeNull();
        
        payload.Items.Count.ShouldBe(2);
        var usd = payload.Items.Single(i => i.Code == "USD");
        usd.Code.ShouldBe("USD");
    }
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeAsync();
    }
}