# VDD Test App

A .NET 9 starter application combining an ASP.NET Core minimal JSON API with a Blazor interactive-server admin placeholder. The current API includes `GET /api/health`, which returns `{"status":"ok"}`.

## Structure

The solution follows inward-facing project dependencies: `Domain` contains core entities and event contracts; `Application` defines use-case and event-handler abstractions; `Infrastructure` provides SQLite/EF Core and in-process event services; and `Web` is the composition root. Web features are organized as vertical slices, with each feature owning its endpoints and contracts. The Blazor home page is a placeholder for the admin UI.

Domain events raised by aggregates are dispatched to their handlers after a successful unit-of-work save. Integration events are published through an abstraction with an in-process handler implementation as the default.

## Logging and configuration

The web host uses Serilog behind `ILogger`, writing to the console and rolling files under `logs/`. An Application Insights sink is enabled only when `ApplicationInsights:ConnectionString` is configured. Settings in `appsettings.json` can be overridden with environment variables; no credentials are included in the repository.

## Build and tests

Run these commands from the repository root:

```sh
dotnet restore
dotnet build --no-restore
dotnet format --verify-no-changes
dotnet test --no-restore --logger "junit;LogFilePath=$PWD/reports/unit.xml"
```

The xUnit suite covers the health endpoint, the prerendered Blazor page, in-memory SQLite, event dispatch and publishing, and project dependency boundaries. The test command writes its JUnit XML report to `reports/unit.xml`.
