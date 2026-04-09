using Shared.Contracts.Enums;

namespace PaymentService.Entities;

public class Order
{
	public int Id { get; set; }
	public string CustomerId { get; set; } = string.Empty;
	public string CustomerName { get; set; } = string.Empty;
	public decimal TotalAmount { get; set; }
	public string? PaymentStatus { get; set; }
	public OrderStatus Status { get; set; }
	public Guid CorrelationId { get; set; }
	public DateTime CreatedAtUtc { get; set; }
	public DateTime? UpdatedAtUtc { get; set; }
}