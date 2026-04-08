namespace OrderManagement.API.Entities;

public class InventoryRecord
{
	public int Id { get; set; }
	public int OrderId { get; set; }

	public bool Success { get; set; }
	public string? Message { get; set; }

	public DateTime CheckedAtUtc { get; set; } = DateTime.UtcNow;
}