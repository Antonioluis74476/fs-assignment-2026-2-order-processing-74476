namespace OrderManagement.API.DTOs;

public class CheckoutOrderItemDto
{
	public int ProductId { get; set; }
	public string ProductName { get; set; } = string.Empty;
	public decimal UnitPrice { get; set; }
	public int Quantity { get; set; }
}