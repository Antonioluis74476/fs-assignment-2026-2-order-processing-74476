using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Enums;
using Shared.Contracts.Messages;
using ShippingService.Data;
using ShippingService.Entities;

namespace ShippingService.Consumers;

public class PaymentApprovedConsumer : IConsumer<PaymentApproved>
{
	private readonly ShippingDbContext _db;
	private readonly ILogger<PaymentApprovedConsumer> _logger;

	public PaymentApprovedConsumer(
		ShippingDbContext db,
		ILogger<PaymentApprovedConsumer> logger)
	{
		_db = db;
		_logger = logger;
	}

	public async Task Consume(ConsumeContext<PaymentApproved> context)
	{
		var message = context.Message;

		var order = await _db.Orders
			.FirstOrDefaultAsync(o => o.Id == message.OrderId, context.CancellationToken);

		if (order is null)
		{
			_logger.LogWarning(
				"Order not found in ShippingService. OrderId: {OrderId}, EventType: {EventType}, ServiceName: {ServiceName}, CorrelationId: {CorrelationId}",
				message.OrderId,
				nameof(PaymentApproved),
				"ShippingService",
				message.CorrelationId);

			return;
		}

		var shipmentReference = $"SHP-{DateTime.UtcNow:yyyyMMddHHmmss}-{order.Id}";
		var estimatedDispatchUtc = DateTime.UtcNow.AddDays(2);

		_db.ShipmentRecords.Add(new ShipmentRecord
		{
			OrderId = order.Id,
			Success = true,
			ShipmentReference = shipmentReference,
			EstimatedDispatchUtc = estimatedDispatchUtc,
			Message = "Shipment created",
			CreatedAtUtc = DateTime.UtcNow
		});

		order.Status = OrderStatus.Completed;
		order.UpdatedAtUtc = DateTime.UtcNow;

		await _db.SaveChangesAsync(context.CancellationToken);

		await context.Publish(new ShippingCreated
		{
			OrderId = order.Id,
			CustomerId = order.CustomerId,
			ShipmentReference = shipmentReference,
			EstimatedDispatchUtc = estimatedDispatchUtc,
			CorrelationId = message.CorrelationId
		});

		_logger.LogInformation(
			"Shipping created. OrderId: {OrderId}, CustomerId: {CustomerId}, EventType: {EventType}, ServiceName: {ServiceName}, CorrelationId: {CorrelationId}, ShipmentReference: {ShipmentReference}",
			order.Id,
			order.CustomerId,
			nameof(ShippingCreated),
			"ShippingService",
			message.CorrelationId,
			shipmentReference);
	}
}