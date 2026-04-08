using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using Serilog;

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
	.MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
	.MinimumLevel.Override("Microsoft.Hosting", Serilog.Events.LogEventLevel.Warning)
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
	.WriteTo.Seq("http://localhost:5341")
	.CreateLogger();

try
{
	Log.Information("OrderManagement.API starting up");

	var builder = WebApplication.CreateBuilder(args);

	builder.Host.UseSerilog((ctx, lc) => lc
		.ReadFrom.Configuration(ctx.Configuration)
		.Enrich.FromLogContext()
		.WriteTo.Console()
		.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
		.WriteTo.Seq("http://localhost:5341"));

	builder.Services.AddControllers();
	builder.Services.AddEndpointsApiExplorer();
	builder.Services.AddSwaggerGen();

	builder.Services.AddDbContext<OrderManagementDbContext>(options =>
		options.UseSqlServer(
			builder.Configuration.GetConnectionString("OrderManagementConnection")));

	builder.Services.AddMassTransit(x =>
	{
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
		});
	});

	builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
	builder.Services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());

	var app = builder.Build();

	if (app.Environment.IsDevelopment())
	{
		app.UseSwagger();
		app.UseSwaggerUI();
	}

	app.UseHttpsRedirection();
	app.UseAuthorization();
	app.MapControllers();

	Log.Information("OrderManagement.API started successfully");
	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "OrderManagement.API terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
}