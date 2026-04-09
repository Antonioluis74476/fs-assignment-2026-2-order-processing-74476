using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Contracts.Enums;
using Shared.Contracts.Messages;
using ShippingService.Consumers;
using ShippingService.Data;
using ShippingService.Entities;
using Xunit;

namespace DistributedServices.Tests.Consumers;

public class ShippingConsumerTests
{
	[Fact]
	public async Task PaymentApprovedConsumer_SetsStatusToCompleted_AndCreatesShipment()
	{
		var options = new DbContextOptionsBuilder<ShippingDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		await using var db = new ShippingDbContext(options);

		db.Orders.Add(new Order
		{
			Id = 1,
			CustomerId = "cust-003",
			CustomerName = "Luis Shipping",
			TotalAmount = 300m,
			Status = OrderStatus.PaymentApproved
		});

		await db.SaveChangesAsync();

		var loggerMock = new Mock<ILogger<PaymentApprovedConsumer>>();
		var consumer = new PaymentApprovedConsumer(db, loggerMock.Object);

		var contextMock = new Mock<ConsumeContext<PaymentApproved>>();
		contextMock.SetupGet(x => x.Message).Returns(new PaymentApproved
		{
			OrderId = 1,
			CustomerId = "cust-003",
			PaymentReference = "PAY-123",
			CorrelationId = Guid.NewGuid()
		});
		contextMock.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
		contextMock.Setup(x => x.Publish(It.IsAny<ShippingCreated>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		await consumer.Consume(contextMock.Object);

		var order = await db.Orders.FirstAsync();
		Assert.Equal(OrderStatus.Completed, order.Status);
		Assert.Single(db.ShipmentRecords);
		Assert.True(db.ShipmentRecords.First().Success);
	}
}