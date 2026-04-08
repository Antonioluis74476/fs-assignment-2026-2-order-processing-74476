namespace Shared.Contracts.Messages;

public record OrderSubmitted : BaseEvent
{
	public int OrderId { get; init; }
	public string CustomerId { get; init; } = string.Empty;
	public decimal TotalAmount { get; init; }
}