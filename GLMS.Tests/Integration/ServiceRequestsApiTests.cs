using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GLMS.Web.Models;

namespace GLMS.Tests.Integration;

public class ServiceRequestsApiTests : IClassFixture<GlmsApiFactory>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ServiceRequestsApiTests(GlmsApiFactory factory)
    {
        _client = factory.CreateSeededClient();
    }

    [Fact]
    public async Task GetAllServiceRequests_Returns200_WithNonNullList()
    {
        var response = await _client.GetAsync("/api/servicerequests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(body));

        var requests = JsonSerializer.Deserialize<List<ServiceRequest>>(body, JsonOpts);
        Assert.NotNull(requests);
    }

    [Fact]
    public async Task GetAllServiceRequests_Returns200_WithSeededRequest()
    {
        var response = await _client.GetAsync("/api/servicerequests");
        response.EnsureSuccessStatusCode();

        var requests = await response.Content.ReadFromJsonAsync<List<ServiceRequest>>(JsonOpts);

        Assert.NotNull(requests);
        Assert.NotEmpty(requests);
        Assert.Contains(requests, r => r.Description.Contains("Durban"));
    }

    [Fact]
    public async Task GetServiceRequestById_ExistingId_Returns200()
    {
        var response = await _client.GetAsync("/api/servicerequests/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var request = await response.Content.ReadFromJsonAsync<ServiceRequest>(JsonOpts);
        Assert.NotNull(request);
        Assert.Equal(1, request.Id);
        Assert.Equal(500m, request.CostUSD);
    }

    [Fact]
    public async Task GetServiceRequestById_NonExistingId_Returns404()
    {
        var response = await _client.GetAsync("/api/servicerequests/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetZarRate_Returns200_WithNumericRate()
    {
        var response = await _client.GetAsync("/api/servicerequests/rate");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ZarRateResponse>(JsonOpts);
        Assert.NotNull(body);
        Assert.True(body.Rate > 0, "ZAR rate must be a positive number.");
    }

    [Fact]
    public async Task CreateServiceRequest_ActiveContract_Returns201()
    {
        var payload = new
        {
            ContractId = 1,   // Active contract seeded in GlmsApiFactory
            Description = "Air freight Cape Town to Dubai",
            CostUSD = 1200m,
            Status = "Pending"
        };

        var response = await _client.PostAsJsonAsync("/api/servicerequests", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<ServiceRequest>(JsonOpts);
        Assert.NotNull(created);
        Assert.True(created.CostZAR > 0, "CostZAR must be auto-calculated and greater than zero.");
    }

    [Fact]
    public async Task CreateServiceRequest_ExpiredContract_Returns400()
    {
        var payload = new
        {
            ContractId = 2,   // Expired contract seeded in GlmsApiFactory
            Description = "Should be rejected",
            CostUSD = 500m,
            Status = "Pending"
        };

        var response = await _client.PostAsJsonAsync("/api/servicerequests", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Expired", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateServiceRequest_NonExistentContract_Returns400()
    {
        var payload = new
        {
            ContractId = 9999,
            Description = "Contract does not exist",
            CostUSD = 100m,
            Status = "Pending"
        };

        var response = await _client.PostAsJsonAsync("/api/servicerequests", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteServiceRequest_ExistingId_Returns204()
    {
        // Create one to delete
        var payload = new
        {
            ContractId = 1,
            Description = "Temporary request for delete test",
            CostUSD = 50m,
            Status = "Pending"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/servicerequests", payload);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<ServiceRequest>(JsonOpts);
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/servicerequests/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteServiceRequest_NonExistingId_Returns404()
    {
        var response = await _client.DeleteAsync("/api/servicerequests/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record ZarRateResponse(decimal Rate);
}
