using Microsoft.EntityFrameworkCore;
using ShippingService.Entities;

namespace ShippingService.Data;

public class ShippingDbContext : DbContext
{
	public ShippingDbContext(DbContextOptions<ShippingDbContext> options)
		: base(options)
	{
	}

	public DbSet<Order> Orders => Set<Order>();
	public DbSet<ShipmentRecord> ShipmentRecords => Set<ShipmentRecord>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Order>().ToTable("Orders");
		modelBuilder.Entity<ShipmentRecord>().ToTable("ShipmentRecords");

		modelBuilder.Entity<Order>()
			.Property(o => o.TotalAmount)
			.HasColumnType("decimal(18,2)");
	}
}