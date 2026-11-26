namespace Web.Features.Health;

public sealed record HealthResponse(string Status);

public static class HealthEndpoint
{
    public static IEndpointRouteBuilder MapHealth(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", () => Results.Ok(new HealthResponse("ok")));
        return app;
    }
}
