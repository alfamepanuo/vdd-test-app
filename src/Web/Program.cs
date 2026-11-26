using Application;
using Infrastructure;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Web.Components;
using Web.Features.Health;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services);
        if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            var telemetry = new TelemetryConfiguration { ConnectionString = appInsightsConnectionString };
            configuration.WriteTo.ApplicationInsights(telemetry, TelemetryConverter.Traces);
        }
    });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(
        builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db");
    builder.Services.AddRazorComponents().AddInteractiveServerComponents();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapHealth();
    app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program
{
}
