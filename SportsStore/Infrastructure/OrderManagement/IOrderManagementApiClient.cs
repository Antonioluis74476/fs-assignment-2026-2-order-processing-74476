using SportsStore.Models.Integration;

namespace SportsStore.Infrastructure.OrderManagement;

public interface IOrderManagementApiClient
{
	Task<CheckoutOrderResponseDto?> CheckoutAsync(CheckoutOrderRequestDto request);
	Task<OrderStatusResponseDto?> GetOrderStatusAsync(int orderId);
}