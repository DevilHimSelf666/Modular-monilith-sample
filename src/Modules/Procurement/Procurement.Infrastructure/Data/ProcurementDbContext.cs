using Microsoft.EntityFrameworkCore;
using Procurement.Domain;

namespace Procurement.Infrastructure.Data;

public class ProcurementDbContext : DbContext
{
    public const string Schema = "proc";

    public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options) : base(options)
    {
    }

    public DbSet<ProcurementRequest> ProcurementRequests => Set<ProcurementRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<ProcurementRequest>(entity =>
        {
            entity.ToTable("ProcurementRequests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.InquiryType).HasMaxLength(200);
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.WorkflowInstanceId).HasMaxLength(128);
        });
    }
}
