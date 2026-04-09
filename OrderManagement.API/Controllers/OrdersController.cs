using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using OrderManagement.API.DTOs;
using OrderManagement.API.Entities;
using Shared.Contracts.Enums;
using Shared.Contracts.Messages;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
	private readonly OrderManagementDbContext _db;
	private readonly IPublishEndpoint _publishEndpoint;
	private readonly ILogger<OrdersController> _logger;

	public OrdersController(
		OrderManagementDbContext db,
		IPublishEndpoint publishEndpoint,
		ILogger<OrdersController> logger)
	{
		_db = db;
		_publishEndpoint = publishEndpoint;
		_logger = logger;
	}

	[HttpPost("checkout")]
	public async Task<IActionResult> Checkout([FromBody] CheckoutOrderRequestDto request)
	{
		if (!ModelState.IsValid)
			return ValidationProblem(ModelState);

		if (request.Items is null || request.Items.Count == 0)
			return BadRequest("Order must contain at least one item.");

		var correlationId = Guid.NewGuid();

		var order = new Order
		{
			CustomerId = request.CustomerId,
			CustomerName = request.CustomerName,
			Line1 = request.Line1,
			Line2 = request.Line2,
			Line3 = request.Line3,
			City = request.City,
			State = request.State,
			Zip = request.Zip,
			Country = request.Country,
			GiftWrap = request.GiftWrap,
			Status = OrderStatus.Submitted,
			CorrelationId = correlationId,
			CreatedAtUtc = DateTime.UtcNow,
			TotalAmount = request.Items.Sum(i => i.UnitPrice * i.Quantity),
			Items = request.Items.Select(i => new OrderItem
			{
				ProductId = i.ProductId,
				ProductName = i.ProductName,
				UnitPrice = i.UnitPrice,
				Quantity = i.Quantity
			}).ToList()
		};

		_db.Orders.Add(order);
		await _db.SaveChangesAsync();

		await _publishEndpoint.Publish(new OrderSubmitted
		{
			OrderId = order.Id,
			CustomerId = order.CustomerId,
			TotalAmount = order.TotalAmount,
			CorrelationId = correlationId
		});

		_logger.LogInformation(
			"Order submitted. OrderId: {OrderId}, CustomerId: {CustomerId}, EventType: {EventType}, ServiceName: {ServiceName}, CorrelationId: {CorrelationId}",
			order.Id,
			order.CustomerId,
			nameof(OrderSubmitted),
			"OrderManagement.API",
			correlationId);

		return Accepted(new
		{
			orderId = order.Id,
			status = order.Status.ToString(),
			correlationId = order.CorrelationId
		});
	}

	[HttpGet]
	public async Task<IActionResult> GetOrders()
	{
		var orders = await _db.Orders
			.Include(o => o.Items)
			.OrderByDescending(o => o.CreatedAtUtc)
			.Select(o => new
			{
				o.Id,
				o.CustomerId,
				o.CustomerName,
				Status = o.Status.ToString(),
				o.TotalAmount,
				o.PaymentStatus,
				o.CreatedAtUtc,
				ItemCount = o.Items.Count
			})
			.ToListAsync();

		return Ok(orders);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetOrderById(int id)
	{
		var order = await _db.Orders
			.Include(o => o.Items)
			.FirstOrDefaultAsync(o => o.Id == id);

		if (order is null)
			return NotFound();

		return Ok(new
		{
			order.Id,
			order.CustomerId,
			order.CustomerName,
			Status = order.Status.ToString(),
			order.TotalAmount,
			order.PaymentStatus,
			order.CorrelationId,
			order.CreatedAtUtc,
			Items = order.Items.Select(i => new
			{
				i.ProductId,
				i.ProductName,
				i.UnitPrice,
				i.Quantity
			})
		});
	}

	[HttpGet("{id}/status")]
	public async Task<ActionResult<OrderStatusResponseDto>> GetOrderStatus(int id)
	{
		var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);

		if (order is null)
			return NotFound();

		return Ok(new OrderStatusResponseDto
		{
			OrderId = order.Id,
			Status = order.Status,
			CustomerId = order.CustomerId,
			CorrelationId = order.CorrelationId,
			PaymentStatus = order.PaymentStatus
		});
	}
}