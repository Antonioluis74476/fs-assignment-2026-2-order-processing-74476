using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Entities;
using Shared.Contracts.Enums;
using Shared.Contracts.Messages;

namespace PaymentService.Consumers;

public class InventoryConfirmedConsumer : IConsumer<InventoryConfirmed>
{
	private readonly PaymentDbContext _db;
	private readonly ILogger<InventoryConfirmedConsumer> _logger;

	public InventoryConfirmedConsumer(
		PaymentDbContext db,
		ILogger<InventoryConfirmedConsumer> logger)
	{
		_db = db;
		_logger = logger;
	}

	public async Task Consume(ConsumeContext<InventoryConfirmed> context)
	{
		var message = context.Message;

		var order = await _db.Orders
			.FirstOrDefaultAsync(o => o.Id == message.OrderId, context.CancellationToken);

		if (order is null)
		{
			_logger.LogWarning(
				"Payment service could not find order. OrderId: {OrderId}, EventType: {EventType}, ServiceName: {ServiceName}, CorrelationId: {CorrelationId}",
				message.OrderId,
				nameof(InventoryConfirmed),
				"PaymentService",
				message.CorrelationId);

			return;
		}

		order.Status = OrderStatus.PaymentPending;
		order.PaymentStatus = "Pending";
		order.UpdatedAtUtc = DateTime.UtcNow;
		await _db.SaveChangesAsync(context.CancellationToken);

		// Simple simulation:
		// reject if total amount > 1000, approve otherwise
		var approved = order.TotalAmount <= 1000m;

		if (!approved)
		{
			order.Status = OrderStatus.PaymentFailed;
			order.PaymentStatus = "Failed";
			order.UpdatedAtUtc = DateTime.UtcNow;

			_db.PaymentRecords.Add(new PaymentRecord
			{
				OrderId = order.Id,
				Success = false,
				Message = "Payment rejected: amount exceeds demo approval threshold"
			});

			await _db.SaveChangesAsync(context.CancellationToken);

			await context.Publish(new PaymentRejected
			{
				OrderId = order.Id,
				CustomerId = order.CustomerId,
				Reason = "Payment rejected: amount exceeds demo approval threshold",
				CorrelationId = message.CorrelationId
			});

			_logger.LogWarning(
				"Payment rejected. OrderId: {OrderId}, CustomerId: {CustomerId}, EventType: {EventType}, ServiceName: {ServiceName}, CorrelationId: {CorrelationId}",
				order.Id,
				order.CustomerId,
				nameof(PaymentRejected),
				"PaymentService",
				message.CorrelationId);

			return;
		}

		var paymentReference = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{order.Id}";

		order.Status = OrderStatus.PaymentApproved;
		order.PaymentStatus = "Paid";
		order.UpdatedAtUtc = DateTime.UtcNow;

		_db.PaymentRecords.Add(new PaymentRecord
		{
			OrderId = order.Id,
			Success = true,
			PaymentReference = paymentReference,
			Message = "Payment approved"
		});

		await _db.SaveChangesAsync(context.CancellationToken);

		await context.Publish(new PaymentApproved
		{
			OrderId = order.Id,
			CustomerId = order.CustomerId,
			PaymentReference = paymentReference,
			CorrelationId = message.CorrelationId
		});

		_logger.LogInformation(
			"Payment approved. OrderId: {OrderId}, CustomerId: {CustomerId}, EventType: {EventType}, ServiceName: {ServiceName}, CorrelationId: {CorrelationId}",
			order.Id,
			order.CustomerId,
			nameof(PaymentApproved),
			"PaymentService",
			message.CorrelationId);
	}
}