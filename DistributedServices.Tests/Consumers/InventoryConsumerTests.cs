using InventoryService.Consumers;
using InventoryService.Data;
using InventoryService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Contracts.Enums;
using Shared.Contracts.Messages;
using Xunit;

namespace DistributedServices.Tests.Consumers;

public class InventoryConsumerTests
{
	[Fact]
	public async Task OrderSubmittedConsumer_SetsStatusToInventoryConfirmed_ForValidOrder()
	{
		var options = new DbContextOptionsBuilder<InventoryDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		await using var db = new InventoryDbContext(options);

		db.Orders.Add(new Order
		{
			Id = 1,
			CustomerId = "cust-001",
			CustomerName = "Luis",
			TotalAmount = 200m,
			Status = OrderStatus.Submitted,
			Items =
			[
				new OrderItem
				{
					Id = 1,
					ProductId = 1,
					ProductName = "Kayak",
					Quantity = 2,
					UnitPrice = 100m
				}
			]
		});

		await db.SaveChangesAsync();

		var loggerMock = new Mock<ILogger<OrderSubmittedConsumer>>();
		var consumer = new OrderSubmittedConsumer(db, loggerMock.Object);

		var contextMock = new Mock<ConsumeContext<OrderSubmitted>>();
		contextMock.SetupGet(x => x.Message).Returns(new OrderSubmitted
		{
			OrderId = 1,
			CustomerId = "cust-001",
			TotalAmount = 200m,
			CorrelationId = Guid.NewGuid()
		});
		contextMock.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
		contextMock.Setup(x => x.Publish(It.IsAny<InventoryConfirmed>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		await consumer.Consume(contextMock.Object);

		var order = await db.Orders.FirstAsync();
		Assert.Equal(OrderStatus.InventoryConfirmed, order.Status);
		Assert.Single(db.InventoryRecords);
		Assert.True(db.InventoryRecords.First().Success);
	}
}