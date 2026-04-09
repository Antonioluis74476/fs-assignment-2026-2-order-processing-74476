using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.API.Controllers;
using OrderManagement.API.Data;
using OrderManagement.API.DTOs;
using Shared.Contracts.Messages;
using Xunit;

namespace DistributedServices.Tests.Controllers;

public class OrdersControllerTests
{
	[Fact]
	public async Task Checkout_ReturnsAccepted_AndCreatesOrder()
	{
		var options = new DbContextOptionsBuilder<OrderManagementDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		await using var db = new OrderManagementDbContext(options);

		var publishMock = new Mock<IPublishEndpoint>();
		publishMock
			.Setup(p => p.Publish(It.IsAny<OrderSubmitted>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		var loggerMock = new Mock<ILogger<OrdersController>>();

		var controller = new OrdersController(db, publishMock.Object, loggerMock.Object);

		var request = new CheckoutOrderRequestDto
		{
			CustomerId = "cust-test",
			CustomerName = "Luis Test",
			Line1 = "123 Main Street",
			City = "Dublin",
			State = "Leinster",
			Country = "Ireland",
			GiftWrap = false,
			Items =
			[
				new CheckoutOrderItemDto
				{
					ProductId = 1,
					ProductName = "Kayak",
					UnitPrice = 100m,
					Quantity = 2
				}
			]
		};

		var result = await controller.Checkout(request);

		var acceptedResult = Assert.IsType<AcceptedResult>(result);

		Assert.Single(db.Orders);
		Assert.Equal("Luis Test", db.Orders.First().CustomerName);

		publishMock.Verify(
			p => p.Publish(It.IsAny<OrderSubmitted>(), It.IsAny<CancellationToken>()),
			Times.Once);

		Assert.NotNull(acceptedResult.Value);
	}
}