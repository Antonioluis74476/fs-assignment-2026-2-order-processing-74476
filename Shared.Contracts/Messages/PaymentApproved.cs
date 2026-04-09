namespace Shared.Contracts.Messages;

public record PaymentApproved : BaseEvent
{
	public int OrderId { get; init; }
	public string CustomerId { get; init; } = string.Empty;
	public string PaymentReference { get; init; } = string.Empty;
}