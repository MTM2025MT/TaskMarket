
# 🤝# TaskMarket API

A REST API for a local-services marketplace. Customers post tasks and job offers, contractors apply, and both sides hire, chat in real time, pay and leave reviews.

Built with ASP.NET Core 8, Entity Framework Core and SQL Server.

## Features

- **Accounts:** registration for customers and contractors, email verification, and JWT login with two roles (`Customer`, `Contractor`).
- **Tasks:** customers create tasks, attach media and move them through statuses.
- **Job offers:** customers publish offers, contractors apply, and the customer accepts one application.
- **Hiring requests:** customers send a hiring request to a contractor, and the contractor accepts or rejects it.
- **Real-time chat:** a SignalR hub (`/chatHub`) delivers messages to everyone in a task's chat, and REST endpoints serve the history and read status.
- **Payments:** a two-step intent / confirm flow. It is simulated; no real payment provider is connected.
- **Reviews and categories:** customers review contractors after a task, and the API lists contractor reviews and service categories.
- **Top-contractors cache:** the most-requested list is served from memory and refreshed every hour in the background (see [Design notes](#design-notes)).
- **Request dashboard:** `/dashboard` shows the last 100 requests with their status code and duration.

## Tech stack

| Area | Technology |
|---|---|
| Runtime | .NET 8, ASP.NET Core Web API |
| Database | SQL Server, Entity Framework Core 8.0.22 (Code-First, 4 migrations) |
| Auth | ASP.NET Core Identity 8.0.22, JWT Bearer 8.0.26 |
| Real-time | ASP.NET Core SignalR |
| Errors | ErrorOr 2.0.1 |
| Demo data | Bogus 35.6.5 |
| API docs | Swashbuckle (Swagger) 6.6.2 |

## Project structure

```
TaskMarket/
├── TaskMarket/                 # The API: all running code is here
│   ├── Controllers/            # HTTP endpoints
│   ├── Handlers/               # Use-case logic, plus the SignalR ChatHub
│   ├── Repositories/           # Data access (interfaces in IRepositories/)
│   ├── Models/                 # Entities, DbContext seeding
│   ├── BackgroundJobs/         # JobCacheRefreshService
│   ├── Middlewares/            # RequestTimingMiddleware
│   ├── Migrations/             # EF Core migrations
│   └── TaskMarket.http         # Ready-made requests
├── TaskMarketDomain/           # Placeholder for a future layer split (empty)
├── TaskMarketApplication/      # Placeholder for a future layer split (empty)
└── TaskMarketInfrastructure/   # Placeholder for a future layer split (empty)
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Express, Developer or LocalDB), or SQL Server in Docker
- The EF Core CLI: `dotnet tool install --global dotnet-ef`
- *Optional:* a Gmail account with an [app password](https://support.google.com/accounts/answer/185833), only if you want verification emails to be sent

## Getting started

1. **Clone the repository**

   ```bash
   git clone https://github.com/MTM2025MT/TaskMarket.git
   cd TaskMarket
   ```

2. **Set the connection string.** `appsettings.json` points to a local server named `MO_TALAL`. Change `ConnectionStrings:DefaultConnection` to your own server, for example:

   ```
   Data Source=localhost;Initial Catalog=TaskMarket;Integrated Security=True;Encrypt=False
   ```

3. **Set the secrets** (see [Configuration](#configuration)). The app will not start without `JWT:Secret`.

   ```bash
   cd TaskMarket
   dotnet user-secrets set "JWT:Secret" "<a random string of at least 32 characters>"
   ```

4. **Create the database**

   ```bash
   dotnet ef database update
   ```

5. **Run the API**

   ```bash
   dotnet run --launch-profile https
   ```

   Swagger UI opens at **https://localhost:7264/swagger**.

### Demo data (optional)

`Models/DatabaseSeeder.cs` fills the database with fake data using Bogus: 10 categories, 50 customers, 20 contractors, plus tasks, offers, messages, payments and reviews. To use it, uncomment the seeding block in `Program.cs` (the three lines after `// Uncomment to apply migrations and seed fake data`) and run the app once.

Every seeded user has the password `P@ssw0rd123!`, for example `customer_0@example.com`. Seeded users are already verified, so they can log in straight away.

## Configuration

Secrets are never stored in `appsettings.json`. Use [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) locally, or environment variables elsewhere (`JWT__Secret`, and so on).

| Setting | Required | Where | Description |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | Yes | `appsettings.json` | SQL Server connection string |
| `JWT:Secret` | Yes | user secrets | Key that signs the JWTs (32+ characters) |
| `JWT:Issuer`, `JWT:Audience` | Yes | `appsettings.json` | Already set to `TaskMarket` defaults |
| `MyInformation:Email` | No | user secrets | Gmail address that sends verification emails |
| `MyInformation:Password` | No | user secrets | Gmail **app password** for that address |
| `App:BaseUrl` | No | `appsettings.json` | Public URL used in the verification link |

Without the email settings, registration still works, but no email is sent. Use the seeded users to log in.

## Using the API

1. Call `POST /api/Accounts/Login` with an email and password. Copy the `token` from the response.
2. In Swagger, click **Authorize** and paste the token. In `TaskMarket.http`, set `@token`.
3. Call the protected endpoints.

### Endpoints

| Area | Endpoints |
|---|---|
| Accounts | `POST /api/Accounts/register` · `POST /api/Accounts/register-contractor` · `GET /api/Accounts/verify-email` · `POST /api/Accounts/Login` |
| Contractors | `GET /api/Contractor/GetBestContractors` · `GET /api/Contractor/{id}` · `GET /api/contractors/{contractorId}/reviews` |
| Tasks | `POST /api/tasks` · `GET /api/tasks` · `GET /api/tasks/{taskId}` · `GET /api/tasks/GetTasksByCustomerId/{customerId}` · `PUT /api/tasks/{taskId}/status` · `POST /api/tasks/{taskId}/media` · `GET/POST /api/tasks/{taskId}/messages` · `POST /api/tasks/{taskId}/reviews` |
| Job offers | `POST /api/offers` · `GET /api/offers` · `GET /api/offers/GetOffersByCustomerId/{customerId}` · `POST/GET /api/offers/{offerId}/applications` · `POST /api/offers/{offerId}/applications/{applicationId}/accept` |
| Hiring requests | `POST /api/hiring-requests` · `GET /api/hiring-requests` · `PUT /api/hiring-requests/{requestId}/status` |
| Payments | `POST /api/payments/intent` · `POST /api/payments/confirm` · `GET /api/payments` |
| Messages | `PUT /api/messages/{messageId}/read` · `GET /api/messages/GetAllMesseges/{userid}` |
| Categories | `GET /api/categories` |
| Monitoring | `GET /dashboard` · `GET /dashboard/data` |

Creating tasks, offers, hiring requests and payments needs the `Customer` role. Applying to offers and answering hiring requests needs the `Contractor` role.

### Real-time chat

Connect to `/chatHub` with the JWT in the query string, because browsers cannot send headers over WebSockets:

```js
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7264/chatHub?access_token=" + token)
  .build();

connection.on("ReceiveMessage", (msg) => console.log(msg));

await connection.start();
await connection.invoke("JoinTaskChat", taskId);
await connection.invoke("SendMessage", userId, "Hello", taskId);
```

CORS allows `http://localhost:5173` and `http://localhost:5174` (React dev servers).

<!--
## Screenshots

![Swagger UI](docs/swagger.png)
![Request dashboard](docs/dashboard.png)
-->

## Tests

There are no automated tests yet. `TaskMarket/TaskMarket.http` contains ready-made requests you can run from Visual Studio or VS Code (REST Client).

## Design notes

- **Thin controllers, handler classes.** Controllers map HTTP to a handler (for example `HiringRequestsController` → `HiringRequestHandler`), and handlers return `ErrorOr<T>`. Controllers turn that into the right status code, so errors look the same on every endpoint.
- **Cache-aside with background refresh.** `GET /api/Contractor/GetBestContractors` reads the top 50 contractors by rating from `IMemoryCache` and falls back to the database. `JobCacheRefreshService` (a `BackgroundService` with a `PeriodicTimer`) reloads the list at startup and then every hour. Measured locally, the endpoint went from about 5–7 ms to under 1.1 ms (~6x).
- **Safe deletes.** `OnModelCreating` uses `DeleteBehavior.Restrict` on messages, payments and reviews. This avoids SQL Server's "multiple cascade paths" error and stops one delete from removing another user's data.
- **Only valid accounts get in.** Every JWT carries an `account_valid` claim, set at login from whether the account is active, verified and not deleted. The API rejects tokens where it is false, so an unverified account can log in but cannot call protected endpoints.
- **Money as `decimal(18,2)`**, so amounts are stored without floating-point errors.
