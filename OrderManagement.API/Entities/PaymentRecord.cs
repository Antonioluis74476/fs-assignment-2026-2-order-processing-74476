namespace OrderManagement.API.Entities;

public class PaymentRecord
{
	public int Id { get; set; }
	public int OrderId { get; set; }

	public bool Success { get; set; }
	public string? PaymentReference { get; set; }
	public string? Message { get; set; }

	public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}