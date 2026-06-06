using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using GLMS.Api;
using GLMS.Web.Data;
using GLMS.Web.Models;

namespace GLMS.Tests.Integration;

public class GlmsApiFactory : WebApplicationFactory<ApiMarker>
{
    // Unique per factory instance — prevents cross-class InMemory store sharing
    private readonly string _dbName = "TestDb_" + Guid.NewGuid().ToString("N");
    private bool _seeded;
    private readonly object _seedLock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("TestDatabaseName", _dbName);
    }

    public HttpClient CreateSeededClient()
    {
        var client = CreateClient();
        EnsureSeeded();
        return client;
    }

    private void EnsureSeeded()
    {
        if (_seeded) return;
        lock (_seedLock)
        {
            if (_seeded) return;

            // Services is the live test-server root provider — same store requests use
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            SeedTestData(db);
            _seeded = true;
        }
    }

    private static void SeedTestData(AppDbContext db)
    {
        db.Clients.Add(new Client
        {
            Id = 1,
            Name = "TechMove Logistics",
            ContactDetails = "logistics@techm.co.za",
            Region = "Southern Africa"
        });

        db.Contracts.AddRange(
            new Contract
            {
                Id = 1,
                ClientId = 1,
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                Status = ContractStatus.Active,
                ServiceLevel = "Premium"
            },
            new Contract
            {
                Id = 2,
                ClientId = 1,
                StartDate = new DateTime(2023, 1, 1),
                EndDate = new DateTime(2024, 1, 1),
                Status = ContractStatus.Expired,
                ServiceLevel = "Standard"
            },
            new Contract
            {
                Id = 3,
                ClientId = 1,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2027, 12, 31),
                Status = ContractStatus.Draft,
                ServiceLevel = "Economy"
            }
        );

        db.ServiceRequests.Add(new ServiceRequest
        {
            Id = 1,
            ContractId = 1,
            Description = "Freight shipment from Durban to Johannesburg",
            CostUSD = 500m,
            CostZAR = 9250m,
            Status = ServiceRequestStatus.Pending
        });

        db.SaveChanges();
    }
}
