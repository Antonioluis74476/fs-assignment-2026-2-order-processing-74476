using System.Net.Http.Json;
using System.Text.Json;
using SportsStore.Models.Integration;

namespace SportsStore.Infrastructure.OrderManagement;

public class OrderManagementApiClient : IOrderManagementApiClient
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<OrderManagementApiClient> _logger;

	public OrderManagementApiClient(
		HttpClient httpClient,
		ILogger<OrderManagementApiClient> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
	}

	public async Task<CheckoutOrderResponseDto?> CheckoutAsync(CheckoutOrderRequestDto request)
	{
		var response = await _httpClient.PostAsJsonAsync("/api/orders/checkout", request);

		if (!response.IsSuccessStatusCode)
		{
			var error = await response.Content.ReadAsStringAsync();
			_logger.LogError("Checkout API call failed. Status: {StatusCode}, Body: {Body}",
				response.StatusCode, error);
			return null;
		}

		var json = await response.Content.ReadAsStringAsync();

		var result = JsonSerializer.Deserialize<CheckoutOrderResponseDto>(
			json,
			new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		return result;
	}

	public async Task<OrderStatusResponseDto?> GetOrderStatusAsync(int orderId)
	{
		var response = await _httpClient.GetAsync($"/api/orders/{orderId}/status");

		if (!response.IsSuccessStatusCode)
		{
			_logger.LogWarning("GetOrderStatus API call failed for OrderId {OrderId}. Status: {StatusCode}",
				orderId, response.StatusCode);
			return null;
		}

		return await response.Content.ReadFromJsonAsync<OrderStatusResponseDto>();
	}
}