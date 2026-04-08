namespace Shared.Contracts.Messages;

public record ShippingCreated : BaseEvent
{
	public int OrderId { get; init; }
	public string CustomerId { get; init; } = string.Empty;
	public string ShipmentReference { get; init; } = string.Empty;
	public DateTime EstimatedDispatchUtc { get; init; }
}