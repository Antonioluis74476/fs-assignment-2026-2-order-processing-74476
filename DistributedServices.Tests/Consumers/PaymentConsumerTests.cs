using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentService.Consumers;
using PaymentService.Data;
using PaymentService.Entities;
using Shared.Contracts.Enums;
using Shared.Contracts.Messages;
using Xunit;

namespace DistributedServices.Tests.Consumers;

public class PaymentConsumerTests
{
	[Fact]
	public async Task InventoryConfirmedConsumer_SetsStatusToPaymentApproved_WhenBelowThreshold()
	{
		var options = new DbContextOptionsBuilder<PaymentDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		await using var db = new PaymentDbContext(options);

		db.Orders.Add(new Order
		{
			Id = 1,
			CustomerId = "cust-002",
			CustomerName = "Luis Payment",
			TotalAmount = 500m,
			Status = OrderStatus.InventoryConfirmed
		});

		await db.SaveChangesAsync();

		var loggerMock = new Mock<ILogger<InventoryConfirmedConsumer>>();
		var consumer = new InventoryConfirmedConsumer(db, loggerMock.Object);

		var contextMock = new Mock<ConsumeContext<InventoryConfirmed>>();
		contextMock.SetupGet(x => x.Message).Returns(new InventoryConfirmed
		{
			OrderId = 1,
			CustomerId = "cust-002",
			CorrelationId = Guid.NewGuid()
		});
		contextMock.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
		contextMock.Setup(x => x.Publish(It.IsAny<PaymentApproved>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		await consumer.Consume(contextMock.Object);

		var order = await db.Orders.FirstAsync();
		Assert.Equal(OrderStatus.PaymentApproved, order.Status);
		Assert.Equal("Paid", order.PaymentStatus);
		Assert.Single(db.PaymentRecords);
		Assert.True(db.PaymentRecords.First().Success);
	}
}