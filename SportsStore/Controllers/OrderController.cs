using Microsoft.AspNetCore.Mvc;
using SportsStore.Infrastructure.OrderManagement;
using SportsStore.Models;
using SportsStore.Models.Integration;

namespace SportsStore.Controllers
{
	public class OrderController : Controller
	{
		private readonly Cart _cart;
		private readonly ILogger<OrderController> _logger;
		private readonly IOrderManagementApiClient _orderManagementApiClient;

		public OrderController(
			Cart cartService,
			ILogger<OrderController> logger,
			IOrderManagementApiClient orderManagementApiClient)
		{
			_cart = cartService;
			_logger = logger;
			_orderManagementApiClient = orderManagementApiClient;
		}

		public ViewResult Checkout()
		{
			_logger.LogInformation("Checkout page accessed");
			return View(new Order());
		}

		[HttpPost]
		public async Task<IActionResult> Checkout(Order order)
		{
			_logger.LogInformation(
				"Checkout submitted for customer {Name}, {Line1}, {City}",
				order.Name, order.Line1, order.City);

			if (!_cart.Lines.Any())
			{
				_logger.LogWarning("Checkout attempted with empty cart by {Name}", order.Name);
				ModelState.AddModelError("", "Sorry, your cart is empty!");
			}

			if (!ModelState.IsValid)
			{
				_logger.LogWarning("Order checkout failed validation for {Name}", order.Name);
				return View(order);
			}

			try
			{
				var request = new CheckoutOrderRequestDto
				{
					CustomerId = User?.Identity?.Name ?? "guest-customer",
					CustomerName = order.Name ?? "Unknown Customer",
					Line1 = order.Line1 ?? string.Empty,
					Line2 = order.Line2,
					Line3 = order.Line3,
					City = order.City ?? string.Empty,
					State = order.State ?? string.Empty,
					Zip = order.Zip,
					Country = order.Country ?? string.Empty,
					GiftWrap = order.GiftWrap,
					Items = _cart.Lines.Select(l => new CheckoutOrderItemDto
					{
						ProductId = l.Product.ProductID ?? 0,
						ProductName = l.Product.Name,
						UnitPrice = l.Product.Price,
						Quantity = l.Quantity
					}).ToList()
				};

				var result = await _orderManagementApiClient.CheckoutAsync(request);

				if (result is null)
				{
					_logger.LogError("Distributed checkout failed for customer {Name}", order.Name);
					ModelState.AddModelError("", "There was an error submitting your order. Please try again.");
					return View(order);
				}

				_logger.LogInformation(
					"Distributed order submitted successfully. OrderId: {OrderId}, Status: {Status}",
					result.OrderId, result.Status);

				_cart.Clear();

				return RedirectToAction(nameof(Track), new { orderId = result.OrderId });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to submit distributed order for {Name}", order.Name);
				ModelState.AddModelError("", "There was an error processing your order. Please try again.");
				return View(order);
			}
		}

		[HttpGet]
		public async Task<IActionResult> Track(int orderId)
		{
			var status = await _orderManagementApiClient.GetOrderStatusAsync(orderId);

			if (status is null)
			{
				ViewBag.OrderId = orderId;
				ViewBag.Status = "Unknown";
				ViewBag.PaymentStatus = "Unknown";
				return View();
			}

			ViewBag.OrderId = status.OrderId;
			ViewBag.Status = status.Status;
			ViewBag.PaymentStatus = status.PaymentStatus;
			ViewBag.CorrelationId = status.CorrelationId;

			return View();
		}
	}
}