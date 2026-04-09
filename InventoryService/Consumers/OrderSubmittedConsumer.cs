using InventoryService.Data;
using InventoryService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Enums;
using Shared.Contracts.Messages;

namespace InventoryService.Consumers;

public class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
{
	private readonly InventoryDbContext _db;
	private readonly ILogger<OrderSubmittedConsumer> _logger;

	public OrderSubmittedConsumer(
		InventoryDbContext db,
		ILogger<OrderSubmittedConsumer> logger)
	{
		_db = db;
		_logger = logger;
	}

	public async Task Consume(ConsumeContext<OrderSubmitted> context)
	{
		var message = context.Message;

		var order = await _db.Orders
			.Include(o => o.Items)
			.FirstOrDefaultAsync(o => o.Id == message.OrderId, context.CancellationToken);

		if (order is null)
		{
			_logger.LogWarning(
				"Inventory service could not find order. OrderId: {OrderId}, EventType: {EventType}, ServiceName: {ServiceName}, CorrelationId: {CorrelationId}",
				message.OrderId,
				nameof(OrderSubmitted),
				"InventoryService",
				message.CorrelationId);

			return;
		}

		order.Status = OrderStatus.InventoryPending;
		order.UpdatedAtUtc = DateTime.UtcNow;
		await _db.SaveChangesAsync(context.CancellationToken);

		// Simple simulation for now:
		// fail if any item quantity > 20, otherwise succeed
		var failedItem = order.Items.FirstOrDefault(i => i.Quantity > 20);

		if (failedItem is not null)
		{
			order.Status = OrderStatus.InventoryFailed;
			order.UpdatedAtUtc = DateTime.UtcNow;

			_db.InventoryRecords.Add(new InventoryRecord
			{
				OrderId = order.Id,
				Success = false,
				Message = $"Insufficient inventory for product {failedItem.ProductName}"
			});

			await _db.SaveChangesAsync(context.CancellationToken);

			await context.Publish(new InventoryFailed
			{
				OrderId = order.Id,
				CustomerId = order.CustomerId,
				Reason = $"Insufficient inventory for product {failedItem.ProductName}",
				CorrelationId = message.CorrelationId
			});

			_logger.LogWarning(
				"Inventory failed. OrderId: {OrderId}, CustomerId: {CustomerId}, EventType: {EventType}, ServiceName: {ServiceName}, CorrelationId: {CorrelationId}",
				order.Id,
				order.CustomerId,
				nameof(InventoryFailed),
				"InventoryService",
				message.CorrelationId);

			return;
		}

		order.Status = OrderStatus.InventoryConfirmed;
		order.UpdatedAtUtc = DateTime.UtcNow;

		_db.InventoryRecords.Add(new InventoryRecord
		{
			OrderId = order.Id,
			Success = true,
			Message = "Inventory confirmed"
		});

		await _db.SaveChangesAsync(context.CancellationToken);

		await context.Publish(new InventoryConfirmed
		{
			OrderId = order.Id,
			CustomerId = order.CustomerId,
			CorrelationId = message.CorrelationId
		});

		_logger.LogInformation(
			"Inventory confirmed. OrderId: {OrderId}, CustomerId: {CustomerId}, EventType: {EventType}, ServiceName: {ServiceName}, CorrelationId: {CorrelationId}",
			order.Id,
			order.CustomerId,
			nameof(InventoryConfirmed),
			"InventoryService",
			message.CorrelationId);
	}
}