namespace SportsStore.Models.Integration;

public class CheckoutOrderItemDto
{
	public long ProductId { get; set; }
	public string ProductName { get; set; } = string.Empty;
	public decimal UnitPrice { get; set; }
	public int Quantity { get; set; }
}