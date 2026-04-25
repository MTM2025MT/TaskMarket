### 2. `TaskMarket` README Template

Copy and paste this directly into the `README.md` file for your TaskMarket repository:

```markdown
# 🤝 TaskMarket - Core API

> A modular Web API monolith designed to manage complex contractor hiring workflows, payments, and messaging with strict relational safeguards and an explicit use-case handler architecture.

This project is implemented as a decoupled ASP.NET Core Web API. HTTP controllers orchestrate use cases through dedicated handlers and repositories, while Entity Framework Core manages relational persistence and identity-linked domain entities. The result is a highly testable API that deploys as a single unit while preserving clean architectural boundaries.

## 🛠️ Technology Stack
* **Framework:** ASP.NET Core Web API (.NET 8)
* **Language:** C# 12.0 (Modern C# styling, nullable enabled)
* **Database:** SQL Server
* **ORM:** Entity Framework Core 8
* **Authentication:** ASP.NET Core Identity
* **Error Modeling:** `ErrorOr` (Result matching pattern)

## ⚙️ Core System Mechanics & Architecture

The architecture reflects a pragmatic engineering style focused on testability and data integrity:

* **Handler/Use-Case Driven Execution:** * Controllers act strictly as the API boundary. They validate input and delegate business execution to dedicated handlers (e.g., `HiringRequestsController` delegates to `HiringRequestHandler`), mapping result states to HTTP responses via the `ErrorOr` pattern.
  * This creates a stable seam between the HTTP transport layer and business logic, fully adhering to the Single Responsibility Principle (SRP).
* **Strict Relational Safeguards:** * Domain entities are heavily modeled with explicit data constraints (e.g., `decimal(18,2)` for monetary values) to prevent schema drift and precision bugs.
  * `OnModelCreating` deliberately enforces `DeleteBehavior.Restrict` for multi-user-linked entities (messages, payments, reviews). This critically prevents SQL Server multiple cascade path issues and unintended data loss in complex relational graphs.
* **Identity & Domain Integration:** * A unified account model drives cross-cutting features (contractor/customer profiles, messaging, payments). This supports extensible account-driven workflows without fragmenting the underlying authentication sources.
* **Optimized Serialization:** * Configured `System.Text.Json` with `ReferenceHandler.IgnoreCycles` to actively prevent runtime serialization loops from bidirectional navigation properties—a vital operational safeguard for deep EF Core graphs.

## 💻 Running Locally

1. Clone the repository:
   ```bash
   git clone [https://github.com/MTM2025MT/TaskMarket.git](https://github.com/MTM2025MT/TaskMarket.git)
Navigate to the project directory:

Bash
cd TaskMarket
Update appsettings.json with your local SQL Server connection string.

Apply Entity Framework migrations:

Bash
dotnet ef database update
Run the application:

Bash
dotnet run
