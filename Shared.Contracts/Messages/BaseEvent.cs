namespace Shared.Contracts.Messages;

public abstract record BaseEvent
{
	public Guid CorrelationId { get; init; } = Guid.NewGuid();
	public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}