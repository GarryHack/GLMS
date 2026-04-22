using Microsoft.EntityFrameworkCore;
using GLMS.Web.Models;

namespace GLMS.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasOne(c => c.Client)
                  .WithMany(cl => cl.Contracts)
                  .HasForeignKey(c => c.ClientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.Property(sr => sr.CostUSD).HasColumnType("decimal(18,2)");
            entity.Property(sr => sr.CostZAR).HasColumnType("decimal(18,2)");

            entity.HasOne(sr => sr.Contract)
                  .WithMany(c => c.ServiceRequests)
                  .HasForeignKey(sr => sr.ContractId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
