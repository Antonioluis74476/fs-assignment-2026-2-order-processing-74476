namespace ShippingService.Entities;

public class ShipmentRecord
{
	public int Id { get; set; }
	public int OrderId { get; set; }

	public bool Success { get; set; }
	public string? ShipmentReference { get; set; }
	public DateTime? EstimatedDispatchUtc { get; set; }
	public string? Message { get; set; }

	public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}