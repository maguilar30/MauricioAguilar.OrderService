using Apache.NMS;
using Apache.NMS.ActiveMQ;
using MauricioAguilar.OrderService.Exceptions;
using MauricioAguilar.OrderService.Messaging;
using MauricioAguilar.OrderService.Middleware;
using MauricioAguilar.OrderService.Repositories;
using MauricioAguilar.OrderService.Services;
using MauricioAguilar.OrderService.Data;
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

var brokerUri = builder.Configuration["ActiveMQ:BrokerUri"];
var user = builder.Configuration["ActiveMQ:UserName"];
var pass = builder.Configuration["ActiveMQ:Password"];

IConnectionFactory factory = new ConnectionFactory(brokerUri);
IConnection connection = factory.CreateConnection(user, pass);
connection.Start();


builder.Services.AddSingleton<IConnection>(connection);
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton<IActiveMqPublisher, ActiveMqPublisher>();
builder.Services.AddHostedService<InboxOrderConsumer>();
builder.Services.AddHostedService<OutboxPublisherWorker>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.Lifetime.ApplicationStopping.Register(() => {
    connection.Close();
    connection.Dispose();
});

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
