using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Consumers;
using PaymentService.Data;
using Serilog;

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
	.MinimumLevel.Override("Microsoft.Hosting", Serilog.Events.LogEventLevel.Warning)
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
	.WriteTo.Seq("http://localhost:5341")
	.CreateLogger();

try
{
	Log.Information("PaymentService starting up");

	var builder = Host.CreateApplicationBuilder(args);

	builder.Services.AddSerilog((services, lc) => lc
		.ReadFrom.Configuration(builder.Configuration)
		.Enrich.FromLogContext()
		.WriteTo.Console()
		.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
		.WriteTo.Seq("http://localhost:5341"));

	builder.Services.AddDbContext<PaymentDbContext>(options =>
		options.UseSqlServer(
			builder.Configuration.GetConnectionString("OrderManagementConnection")));

	builder.Services.AddMassTransit(x =>
	{
		x.AddConsumer<InventoryConfirmedConsumer>();

		x.UsingRabbitMq((context, cfg) =>
		{
			cfg.Host(
				builder.Configuration["RabbitMq:Host"] ?? "localhost",
				"/",
				h =>
				{
					h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
					h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
				});

			cfg.ConfigureEndpoints(context);
		});
	});

	var host = builder.Build();

	Log.Information("PaymentService started successfully");
	host.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "PaymentService terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
}