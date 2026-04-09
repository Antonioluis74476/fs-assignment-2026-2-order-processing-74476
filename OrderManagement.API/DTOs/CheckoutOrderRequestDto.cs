using System.ComponentModel.DataAnnotations;

namespace OrderManagement.API.DTOs;

public class CheckoutOrderRequestDto
{
	[Required]
	public string CustomerId { get; set; } = string.Empty;

	[Required]
	public string CustomerName { get; set; } = string.Empty;

	[Required]
	public string Line1 { get; set; } = string.Empty;

	public string? Line2 { get; set; }
	public string? Line3 { get; set; }

	[Required]
	public string City { get; set; } = string.Empty;

	[Required]
	public string State { get; set; } = string.Empty;

	public string? Zip { get; set; }

	[Required]
	public string Country { get; set; } = string.Empty;

	public bool GiftWrap { get; set; }

	public List<CheckoutOrderItemDto> Items { get; set; } = new();
}