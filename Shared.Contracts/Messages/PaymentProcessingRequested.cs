namespace Shared.Contracts.Messages;

public record PaymentProcessingRequested : BaseEvent
{
	public int OrderId { get; init; }
	public string CustomerId { get; init; } = string.Empty;
	public decimal Amount { get; init; }
}