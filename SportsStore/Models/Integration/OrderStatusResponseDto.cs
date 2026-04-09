namespace SportsStore.Models.Integration;

public class OrderStatusResponseDto
{
	public int OrderId { get; set; }
	public string Status { get; set; } = string.Empty;
	public string CustomerId { get; set; } = string.Empty;
	public Guid CorrelationId { get; set; }
	public string? PaymentStatus { get; set; }
}