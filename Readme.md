# Order Service

An ASP.NET Core Web API for creating and querying customer orders. The service persists orders in SQL Server and integrates with Apache ActiveMQ using an inbox/outbox messaging pattern.

## Features

- Create an order with customer, item, shipping and billing addresses, currency, tax, shipping cost, and discounts.
- Retrieve all orders or retrieve an order by its numeric ID.
- Persist orders and outgoing messages in SQL Server using Entity Framework Core.
- Publish pending outbox messages to ActiveMQ and consume order-creation commands from an ActiveMQ queue.
- Use inbox message IDs to avoid processing the same incoming command more than once.
- Log HTTP requests and return structured Problem Details responses for unhandled errors.

## Technology

- .NET 10 / ASP.NET Core
- Entity Framework Core 10 with SQL Server
- Apache ActiveMQ via Apache.NMS

## Requirements

- .NET 10 SDK
- SQL Server instance
- Apache ActiveMQ broker
- Entity Framework Core CLI (`dotnet-ef`) to apply database migrations

The API requires both SQL Server and ActiveMQ configuration at startup. The sample values in `appsettings.json` are placeholders and must be replaced with valid settings.

## Configuration

Set the following values using .NET User Secrets, environment variables, or another secure configuration provider. Do not commit real credentials to source control.

| Setting | Description |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string for the order database. |
| `ActiveMQ:BrokerUri` | ActiveMQ transport URI, for example `activemq:tcp://localhost:61616`. |
| `ActiveMQ:UserName` | ActiveMQ username, if authentication is enabled. |
| `ActiveMQ:Password` | ActiveMQ password, if authentication is enabled. |
| `ActiveMQ:Topic` | Topic used by the outbox publisher; sample value: `VirtualTopic.OrderManagement`. |
| `ActiveMQ:Queue` | Queue consumed by the inbox worker; sample value: `Consumer.Orders.VirtualTopic.OrderManagement`. |

From the project directory, configure local secrets in PowerShell (replace the SQL Server values as appropriate):

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=OrderService;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
dotnet user-secrets set "ActiveMQ:BrokerUri" "activemq:tcp://localhost:61616"
dotnet user-secrets set "ActiveMQ:UserName" "YOUR_ACTIVEMQ_USER"
dotnet user-secrets set "ActiveMQ:Password" "YOUR_ACTIVEMQ_PASSWORD"
```

The topic and queue can remain at their sample values if those destinations match your broker configuration. User Secrets are for local development; use your hosting platform's secret store for deployed environments.

## Run locally

From the directory containing `MauricioAguilar.OrderService.csproj`:

```powershell
dotnet restore
dotnet ef database update
dotnet run --launch-profile http
```

The HTTP launch profile listens at `http://localhost:5214`. The project includes EF Core migrations; apply them to the configured database before making API requests. The service does not apply migrations automatically at startup.

## API

The controller route is `api/Order`.

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/api/Order` | Return the collection of orders. |
| `GET` | `/api/Order/{id}` | Retrieve an order by numeric ID. |
| `POST` | `/api/Order` | Create an order; returns `201 Created` with the created order. |

The order item collection is currently excluded from JSON responses.

### Create an order

```http
POST http://localhost:5214/api/Order
Content-Type: application/json
```

```json
{
  "customerId": 123,
  "items": [
	{
	  "productId": 456,
	  "productName": "Example product",
	  "sku": "SKU-456",
	  "quantity": 2,
	  "unitPrice": 19.99,
	  "discount": 2.00
	}
  ],
  "shippingAddress": {
	"fullName": "Taylor Customer",
	"addressLine1": "100 Main Street",
	"addressLine2": null,
	"city": "Seattle",
	"state": "WA",
	"postalCode": "98101",
	"country": "US"
  },
  "billingAddress": {
	"fullName": "Taylor Customer",
	"addressLine1": "100 Main Street",
	"addressLine2": null,
	"city": "Seattle",
	"state": "WA",
	"postalCode": "98101",
	"country": "US"
  },
  "currency": "USD",
  "tax": 1.50,
  "shippingCost": 5.00
}
```

`Total` is calculated by the service and does not need to be supplied for each item. The current calculation subtracts each item's discount when calculating its line total, then subtracts the aggregate discount again when calculating the order total; account for this behavior when validating totals.

Example requests:

```powershell
curl.exe http://localhost:5214/api/Order
curl.exe http://localhost:5214/api/Order/1
```

## Messaging and persistence

For API-created orders, the order and an outbox record are saved in the same database transaction. The background outbox worker checks for unpublished messages about every three seconds and publishes them to the configured ActiveMQ topic using persistent delivery. Failed publications are recorded and retried by later worker iterations.

The inbox worker listens on the configured queue for JSON `CreateOrderCommand` messages. It uses the command ID to make processing idempotent, stores the resulting order and outgoing event in a database transaction, and acknowledges the broker message after that transaction succeeds. If database processing fails, the broker message is rolled back for redelivery.

Orders are stored under the `ord` database schema, together with order items and inbox/outbox records. Order statuses defined by the application include `Pending`, `InventoryReserved`, `Confirmed`, `Processing`, `Shipped`, `Delivered`, `Cancelled`, `PaymentFailed`, `RefundPending`, and `Refunded`.

## Project layout

- `Controllers/` — HTTP order endpoints.
- `Services/` — order creation and retrieval logic.
- `Repositories/` and `Data/` — persistence operations and EF Core context.
- `Entities/` and `DTOs/` — domain models and API/message contracts.
- `Messaging/` — ActiveMQ publisher, inbox consumer, and outbox worker.
- `Migrations/` — SQL Server schema migrations.
