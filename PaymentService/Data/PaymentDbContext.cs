using Microsoft.EntityFrameworkCore;
using PaymentService.Entities;

namespace PaymentService.Data;

public class PaymentDbContext : DbContext
{
	public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
		: base(options)
	{
	}

	public DbSet<Order> Orders => Set<Order>();
	public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Order>().ToTable("Orders");
		modelBuilder.Entity<PaymentRecord>().ToTable("PaymentRecords");

		modelBuilder.Entity<Order>()
			.Property(o => o.TotalAmount)
			.HasColumnType("decimal(18,2)");
	}
}