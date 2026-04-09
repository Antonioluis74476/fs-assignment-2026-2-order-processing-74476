using InventoryService.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Data;

public class InventoryDbContext : DbContext
{
	public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
		: base(options)
	{
	}

	public DbSet<Order> Orders => Set<Order>();
	public DbSet<OrderItem> OrderItems => Set<OrderItem>();
	public DbSet<InventoryRecord> InventoryRecords => Set<InventoryRecord>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Order>().ToTable("Orders");
		modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
		modelBuilder.Entity<InventoryRecord>().ToTable("InventoryRecords");

		modelBuilder.Entity<Order>()
			.HasMany(o => o.Items)
			.WithOne(i => i.Order)
			.HasForeignKey(i => i.OrderId);

		modelBuilder.Entity<OrderItem>()
			.Property(i => i.UnitPrice)
			.HasColumnType("decimal(18,2)");

		modelBuilder.Entity<Order>()
			.Property(o => o.TotalAmount)
			.HasColumnType("decimal(18,2)");
	}
}