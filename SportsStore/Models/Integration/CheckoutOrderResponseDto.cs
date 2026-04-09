namespace SportsStore.Models.Integration;

public class CheckoutOrderResponseDto
{
	public int OrderId { get; set; }
	public string Status { get; set; } = string.Empty;
	public Guid CorrelationId { get; set; }
}