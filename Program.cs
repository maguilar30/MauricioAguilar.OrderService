using MauricioAguilar.OrderService.Exceptions;
using MauricioAguilar.OrderService.Middleware;
using MauricioAguilar.OrderService.Repositories;
using MauricioAguilar.OrderService.Services;
using MauricioAguilar.OrderService.Messaging;
using MauricioAguilar.Product.Service.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddLogging();
//builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton<IActiveMqPublisher, ActiveMqPublisher>();
builder.Services.AddHostedService<OutboxPublisherWorker>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
