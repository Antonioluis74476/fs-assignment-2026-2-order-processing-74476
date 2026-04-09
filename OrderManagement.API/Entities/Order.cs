using Shared.Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderManagement.API.Entities;

public class Order
{
	public int Id { get; set; }

	[Required]
	public string CustomerId { get; set; } = string.Empty;

	[Required]
	public string CustomerName { get; set; } = string.Empty;

	[Required]
	public string Line1 { get; set; } = string.Empty;

	public string? Line2 { get; set; }
	public string? Line3 { get; set; }

	[Required]
	public string City { get; set; } = string.Empty;

	[Required]
	public string State { get; set; } = string.Empty;

	public string? Zip { get; set; }

	[Required]
	public string Country { get; set; } = string.Empty;

	public bool GiftWrap { get; set; }

	public decimal TotalAmount { get; set; }

	public string? PaymentIntentId { get; set; }
	public string? PaymentStatus { get; set; }

	public OrderStatus Status { get; set; } = OrderStatus.Submitted;

	public Guid CorrelationId { get; set; } = Guid.NewGuid();

	public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAtUtc { get; set; }

	public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}