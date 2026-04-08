namespace Shared.Contracts.Messages;

public record ShippingRequested : BaseEvent
{
	public int OrderId { get; init; }
	public string CustomerId { get; init; } = string.Empty;
}