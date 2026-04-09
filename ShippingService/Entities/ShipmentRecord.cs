namespace ShippingService.Entities;

public class ShipmentRecord
{
	public int Id { get; set; }
	public int OrderId { get; set; }

	public string ShipmentReference { get; set; } = string.Empty;
	public string Carrier { get; set; } = "DemoCarrier";
	public DateTime EstimatedDispatchUtc { get; set; }
	public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}