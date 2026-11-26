namespace Tests;

public class WebTests
{
    [Fact]
    public async Task Health_returns_ok()
    {
        using var factory = new TestWebFactory();
        var response = await factory.CreateClient().GetAsync("/api/health");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("""{"status":"ok"}""", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Home_page_renders_heading()
    {
        using var factory = new TestWebFactory();
        var html = await factory.CreateClient().GetStringAsync("/");

        Assert.Contains("<h1>Translations admin</h1>", html);
    }

    [Fact]
    public async Task Host_starts_with_empty_application_insights_connection_string()
    {
        using var factory = new TestWebFactory(appInsights: "");
        var response = await factory.CreateClient().GetAsync("/api/health");

        Assert.True(response.IsSuccessStatusCode);
    }
}
