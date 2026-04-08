using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Entities;

namespace OrderManagement.API.Data;

public class OrderManagementDbContext : DbContext
{
	public OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options)
		: base(options)
	{
	}

	public DbSet<Order> Orders => Set<Order>();
	public DbSet<OrderItem> OrderItems => Set<OrderItem>();
	public DbSet<InventoryRecord> InventoryRecords => Set<InventoryRecord>();
	public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();
	public DbSet<ShipmentRecord> ShipmentRecords => Set<ShipmentRecord>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Order>()
			.HasMany(o => o.Items)
			.WithOne(i => i.Order)
			.HasForeignKey(i => i.OrderId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<Order>()
			.Property(o => o.TotalAmount)
			.HasColumnType("decimal(18,2)");

		modelBuilder.Entity<OrderItem>()
			.Property(i => i.UnitPrice)
			.HasColumnType("decimal(18,2)");
	}
}