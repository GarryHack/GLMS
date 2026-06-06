using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GLMS.Web.Models;

namespace GLMS.Tests.Integration;

public class ClientsApiTests : IClassFixture<GlmsApiFactory>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ClientsApiTests(GlmsApiFactory factory)
    {
        _client = factory.CreateSeededClient();
    }

    [Fact]
    public async Task GetAllClients_Returns200_WithNonNullList()
    {
        var response = await _client.GetAsync("/api/clients");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(body));

        var clients = JsonSerializer.Deserialize<List<Client>>(body, JsonOpts);
        Assert.NotNull(clients);
    }

    [Fact]
    public async Task GetAllClients_Returns200_WithSeededClient()
    {
        var response = await _client.GetAsync("/api/clients");
        response.EnsureSuccessStatusCode();

        var clients = await response.Content.ReadFromJsonAsync<List<Client>>(JsonOpts);

        Assert.NotNull(clients);
        Assert.NotEmpty(clients);
        Assert.Contains(clients, c => c.Id == 1);
    }

    [Fact]
    public async Task GetClientById_ExistingId_Returns200()
    {
        var response = await _client.GetAsync("/api/clients/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var client = await response.Content.ReadFromJsonAsync<Client>(JsonOpts);
        Assert.NotNull(client);
        Assert.Equal(1, client.Id);
        Assert.False(string.IsNullOrWhiteSpace(client.Name));
    }

    [Fact]
    public async Task GetClientById_NonExistingId_Returns404()
    {
        var response = await _client.GetAsync("/api/clients/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateClient_ValidPayload_Returns201()
    {
        var newClient = new
        {
            Name = "FastFreight Ltd",
            ContactDetails = "info@fastfreight.co.za",
            Region = "West Africa"
        };

        var response = await _client.PostAsJsonAsync("/api/clients", newClient);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateClient_MissingRequiredFields_Returns400()
    {
        var payload = new { Name = "" };

        var response = await _client.PostAsJsonAsync("/api/clients", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateClient_ExistingId_Returns204()
    {
        var updated = new
        {
            Id = 1,
            Name = "TechMove Logistics Updated",
            ContactDetails = "updated@techm.co.za",
            Region = "Southern Africa"
        };

        var response = await _client.PutAsJsonAsync("/api/clients/1", updated);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_ExistingId_Returns204()
    {
        // Create a client to delete so other tests are not affected
        var newClient = new
        {
            Name = "ToDelete Corp",
            ContactDetails = "delete@corp.com",
            Region = "East Africa"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/clients", newClient);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<Client>(JsonOpts);
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/clients/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_NonExistingId_Returns404()
    {
        var response = await _client.DeleteAsync("/api/clients/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
