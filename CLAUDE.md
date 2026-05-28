# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```powershell
# Build
dotnet build OrderHub/OrderHub.Api/OrderHub.Api.csproj /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary

# Run (HTTP: http://localhost:5205, HTTPS: https://localhost:7244)
dotnet run --project OrderHub/OrderHub.Api/OrderHub.Api.csproj
```

OpenAPI schema is available at `/openapi/v1.json` in the Development environment.

Sample HTTP requests are in `OrderHub/OrderHub.Api/OrderHub.Api.http`.

## Architecture

ASP.NET Core Minimal APIs on .NET 10, organized as **Vertical Slices** inside a Clean Architecture shell.

```
OrderHub/OrderHub.Api/
├── Domain/              # Core entities (Product)
├── Features/            # Feature slices (one folder per operation)
│   └── Products/
│       └── CreateProduct/
├── Infrastructure/
│   └── Persistence/     # IProductStore + InMemoryProductStore
├── SharedKernel/        # Shared utilities (currently empty)
└── Program.cs           # Service registration + route mapping
```

### Key patterns

- **Endpoints** live in `Features/<Domain>/<Operation>/` and are registered as extension methods on `IEndpointRouteBuilder`, then called from `Program.cs`.
- **Repository abstraction** — all persistence goes through `IProductStore`; the current backing store is `InMemoryProductStore` (singleton, not thread-safe, dev only).
- **No controller classes** — each feature exposes a static `Map*` extension method that wires the route directly.

### Adding a new endpoint

1. Create `Features/<Domain>/<Operation>/<Name>Endpoint.cs` following the `CreateProductEndpoint` pattern.
2. Register the route in `Program.cs` by calling the new `Map*` extension method.
3. If a new persistence method is needed, add it to `IProductStore` and implement it in `InMemoryProductStore`.

## Issue tracking

Issues are tracked in Jira with the `SEPA` project prefix. Feature branches follow the convention `feature/<ISSUE-KEY>-<short-description>`.
