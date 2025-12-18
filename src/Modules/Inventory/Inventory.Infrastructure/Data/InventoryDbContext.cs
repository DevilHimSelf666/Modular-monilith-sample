using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Data;

public class InventoryDbContext : DbContext
{
    public const string Schema = "inv";

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<ProcurementReservation> ProcurementReservations => Set<ProcurementReservation>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<Warehouse>(builder =>
        {
            builder.ToTable("Warehouses");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(200);
            builder.Property(x => x.Location).HasMaxLength(256);
            builder.HasMany<StockItem>().WithOne().HasForeignKey(x => x.WarehouseId);
        });

        modelBuilder.Entity<StockItem>(builder =>
        {
            builder.ToTable("StockItems");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Sku).HasMaxLength(128);
            builder.Property(x => x.Quantity);
            builder.Property(x => x.ReservedQuantity);
        });

        modelBuilder.Entity<ProcurementReservation>(builder =>
        {
            builder.ToTable("ProcurementReservations");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.MessageId).IsUnique();
            builder.Property(x => x.InquiryType).HasMaxLength(200);
            builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<InboxMessage>(builder =>
        {
            builder.ToTable("InboxMessages");
            builder.HasKey(x => x.MessageId);
            builder.Property(x => x.ReceivedAtUtc);
            builder.Property(x => x.ProcessedAtUtc);
        });
    }
}
