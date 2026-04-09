using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SportsStore.Controllers;
using SportsStore.Infrastructure.OrderManagement;
using SportsStore.Models;
using SportsStore.Models.Integration;
using Xunit;

namespace SportsStore.Tests
{
	public class OrderControllerTests
	{
		private static ILogger<OrderController> GetLogger()
			=> new Mock<ILogger<OrderController>>().Object;

		[Fact]
		public async Task Cannot_Checkout_Empty_Cart()
		{
			var cart = new Cart();
			var apiClientMock = new Mock<IOrderManagementApiClient>();

			var target = new OrderController(
				cart,
				GetLogger(),
				apiClientMock.Object);

			var order = new Order();

			var actionResult = await target.Checkout(order);
			var result = actionResult as ViewResult;

			apiClientMock.Verify(
				m => m.CheckoutAsync(It.IsAny<CheckoutOrderRequestDto>()),
				Times.Never);

			Assert.NotNull(result);
			Assert.True(string.IsNullOrEmpty(result?.ViewName));
			Assert.False(result!.ViewData.ModelState.IsValid);
		}

		[Fact]
		public async Task Cannot_Checkout_Invalid_ShippingDetails()
		{
			var cart = new Cart();
			cart.AddItem(new Product
			{
				ProductID = 1,
				Name = "Test Product",
				Price = 100m,
				Description = "Test",
				Category = "Test"
			}, 1);

			var apiClientMock = new Mock<IOrderManagementApiClient>();

			var target = new OrderController(
				cart,
				GetLogger(),
				apiClientMock.Object);

			target.ModelState.AddModelError("error", "error");

			var actionResult = await target.Checkout(new Order());
			var result = actionResult as ViewResult;

			apiClientMock.Verify(
				m => m.CheckoutAsync(It.IsAny<CheckoutOrderRequestDto>()),
				Times.Never);

			Assert.NotNull(result);
			Assert.True(string.IsNullOrEmpty(result?.ViewName));
			Assert.False(result!.ViewData.ModelState.IsValid);
		}

		[Fact]
		public async Task Can_Checkout_And_Submit_Order()
		{
			var cart = new Cart();
			cart.AddItem(new Product
			{
				ProductID = 1,
				Name = "Kayak",
				Price = 275m,
				Description = "A boat",
				Category = "Watersports"
			}, 2);

			var apiClientMock = new Mock<IOrderManagementApiClient>();
			apiClientMock
				.Setup(m => m.CheckoutAsync(It.IsAny<CheckoutOrderRequestDto>()))
				.ReturnsAsync(new CheckoutOrderResponseDto
				{
					OrderId = 123,
					Status = "Submitted",
					CorrelationId = Guid.NewGuid()
				});

			var target = new OrderController(
				cart,
				GetLogger(),
				apiClientMock.Object);

			var order = new Order
			{
				Name = "Luis Test",
				Line1 = "123 Main Street",
				City = "Dublin",
				State = "Leinster",
				Country = "Ireland"
			};

			var actionResult = await target.Checkout(order);
			var result = actionResult as RedirectToActionResult;

			apiClientMock.Verify(
				m => m.CheckoutAsync(It.IsAny<CheckoutOrderRequestDto>()),
				Times.Once);

			Assert.NotNull(result);
			Assert.Equal("Track", result!.ActionName);
			Assert.Null(result.ControllerName);
			Assert.Equal(123, result.RouteValues?["orderId"]);
		}
	}
}