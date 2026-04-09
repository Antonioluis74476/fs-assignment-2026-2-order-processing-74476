namespace SportsStore.Models.Integration;

public class CheckoutOrderRequestDto
{
	public string CustomerId { get; set; } = string.Empty;
	public string CustomerName { get; set; } = string.Empty;
	public string Line1 { get; set; } = string.Empty;
	public string? Line2 { get; set; }
	public string? Line3 { get; set; }
	public string City { get; set; } = string.Empty;
	public string State { get; set; } = string.Empty;
	public string? Zip { get; set; }
	public string Country { get; set; } = string.Empty;
	public bool GiftWrap { get; set; }
	public List<CheckoutOrderItemDto> Items { get; set; } = new();
}