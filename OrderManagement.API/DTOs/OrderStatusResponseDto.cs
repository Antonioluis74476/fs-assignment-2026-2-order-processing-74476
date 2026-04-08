using Shared.Contracts.Enums;

namespace OrderManagement.API.DTOs;

public class OrderStatusResponseDto
{
	public int OrderId { get; set; }
	public OrderStatus Status { get; set; }
	public string CustomerId { get; set; } = string.Empty;
	public Guid CorrelationId { get; set; }
	public string? PaymentStatus { get; set; }
}