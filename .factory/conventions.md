# Conventions

## Tech stack
C# on .NET 9. One ASP.NET Core web project hosts a minimal JSON API and a Blazor (interactive server) admin UI; a separate xUnit test project. One solution file at the repository root.

## Database
SQLite through Entity Framework Core. Schema changes are EF Core migrations applied at startup.

## Test isolation
Every test creates its own in-memory SQLite database (a connection kept open for the test's lifetime); no shared database file. Tests use xUnit and the JUnit XML logger; the report goes to reports/unit.xml.

## API style
JSON REST under /api, plural nouns, standard HTTP status codes.

## Input validation
Validate request bodies with data annotations or explicit checks in the endpoint; invalid input returns 400.

## Error format
Errors are RFC 7807 problem details (ProblemDetails JSON).

## Identifiers
Integer autoincrement primary keys for rows; translations are addressed by id.

## Dates and times
DateTimeOffset in UTC, serialized as ISO 8601.

## Configuration
appsettings.json with environment variable overrides (for example the database path); no secrets in files.

## Logging
Microsoft.Extensions.Logging (ILogger); quiet in tests.

## Dependencies policy
Prefer the .NET platform libraries; add a NuGet package only when the platform cannot do the job reasonably.
