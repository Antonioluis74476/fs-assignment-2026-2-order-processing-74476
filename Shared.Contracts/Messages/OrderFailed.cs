namespace Shared.Contracts.Messages;

public record OrderFailed : BaseEvent
{
	public int OrderId { get; init; }
	public string CustomerId { get; init; } = string.Empty;
	public string Reason { get; init; } = string.Empty;
}