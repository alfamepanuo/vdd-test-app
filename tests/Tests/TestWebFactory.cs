using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests;

public sealed class TestWebFactory(string? appInsights = null) : WebApplicationFactory<Program>
{
    private readonly string _logDir = Path.Combine(Path.GetTempPath(), "vdd-tests-" + Guid.NewGuid());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Serilog:MinimumLevel:Default", "Fatal");
        builder.UseSetting("Serilog:WriteTo:1:Args:path", Path.Combine(_logDir, "test-.log"));
        builder.UseSetting("ConnectionStrings:Default", "Data Source=:memory:");
        if (appInsights is not null)
        {
            builder.UseSetting("ApplicationInsights:ConnectionString", appInsights);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && Directory.Exists(_logDir))
        {
            Directory.Delete(_logDir, true);
        }
    }
}
