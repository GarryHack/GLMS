using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GLMS.Web.Models;

namespace GLMS.Tests.Integration;

public class ContractsApiTests : IClassFixture<GlmsApiFactory>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ContractsApiTests(GlmsApiFactory factory)
    {
        _client = factory.CreateSeededClient();
    }

    [Fact]
    public async Task GetAllContracts_Returns200_WithNonNullList()
    {
        var response = await _client.GetAsync("/api/contracts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(body));

        var contracts = JsonSerializer.Deserialize<List<Contract>>(body, JsonOpts);
        Assert.NotNull(contracts);
    }

    [Fact]
    public async Task GetAllContracts_Returns200_WithSeededContracts()
    {
        var response = await _client.GetAsync("/api/contracts");
        response.EnsureSuccessStatusCode();

        var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>(JsonOpts);

        Assert.NotNull(contracts);
        Assert.NotEmpty(contracts);
    }

    [Fact]
    public async Task GetContracts_FilterByActiveStatus_ReturnsOnlyActiveContracts()
    {
        var response = await _client.GetAsync("/api/contracts?status=Active");
        response.EnsureSuccessStatusCode();

        var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>(JsonOpts);

        Assert.NotNull(contracts);
        Assert.All(contracts, c => Assert.Equal(ContractStatus.Active, c.Status));
    }

    [Fact]
    public async Task GetContracts_FilterByExpiredStatus_ReturnsOnlyExpiredContracts()
    {
        var response = await _client.GetAsync("/api/contracts?status=Expired");
        response.EnsureSuccessStatusCode();

        var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>(JsonOpts);

        Assert.NotNull(contracts);
        Assert.All(contracts, c => Assert.Equal(ContractStatus.Expired, c.Status));
    }

    [Fact]
    public async Task GetContracts_FilterByDateRange_ReturnsMatchingContracts()
    {
        var response = await _client.GetAsync("/api/contracts?fromDate=2025-01-01&toDate=2026-12-31");
        response.EnsureSuccessStatusCode();

        var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>(JsonOpts);

        Assert.NotNull(contracts);
        Assert.All(contracts, c =>
        {
            Assert.True(c.StartDate >= new DateTime(2025, 1, 1));
            Assert.True(c.EndDate <= new DateTime(2026, 12, 31));
        });
    }

    [Fact]
    public async Task GetContractById_ExistingId_Returns200()
    {
        var response = await _client.GetAsync("/api/contracts/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var contract = await response.Content.ReadFromJsonAsync<Contract>(JsonOpts);
        Assert.NotNull(contract);
        Assert.Equal(1, contract.Id);
        Assert.Equal("Premium", contract.ServiceLevel);
    }

    [Fact]
    public async Task GetContractById_NonExistingId_Returns404()
    {
        var response = await _client.GetAsync("/api/contracts/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateContract_ValidMultipartPayload_Returns201()
    {
        using var form = new MultipartFormDataContent
        {
            { new StringContent("0"),            "Id" },
            { new StringContent("1"),            "ClientId" },
            { new StringContent("2026-01-01"),   "StartDate" },
            { new StringContent("2027-12-31"),   "EndDate" },
            { new StringContent("Active"),       "Status" },
            { new StringContent("Gold"),         "ServiceLevel" }
        };

        var response = await _client.PostAsync("/api/contracts", form);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PatchContractStatus_ExistingId_Returns204()
    {
        var payload = new { Status = "OnHold" };

        var response = await _client.PatchAsJsonAsync("/api/contracts/3/status", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task PatchContractStatus_NonExistingId_Returns404()
    {
        var payload = new { Status = "Expired" };

        var response = await _client.PatchAsJsonAsync("/api/contracts/9999/status", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteContract_ExistingId_Returns204()
    {
        // Create a contract to delete
        using var form = new MultipartFormDataContent
        {
            { new StringContent("0"),           "Id" },
            { new StringContent("1"),           "ClientId" },
            { new StringContent("2026-06-01"),  "StartDate" },
            { new StringContent("2026-12-31"),  "EndDate" },
            { new StringContent("Draft"),       "Status" },
            { new StringContent("Economy"),     "ServiceLevel" }
        };

        var createResponse = await _client.PostAsync("/api/contracts", form);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<Contract>(JsonOpts);
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/contracts/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteContract_NonExistingId_Returns404()
    {
        var response = await _client.DeleteAsync("/api/contracts/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
