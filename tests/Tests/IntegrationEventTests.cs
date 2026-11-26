using Application;
using Domain;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class IntegrationEventTests
{
    private sealed record Ping : IIntegrationEvent;

    private sealed class PingHandler : IIntegrationEventHandler<Ping>
    {
        public static int Received;

        public Task HandleAsync(Ping integrationEvent, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Received);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Published_event_reaches_registered_handler()
    {
        var services = new ServiceCollection();
        services.AddApplication(typeof(IntegrationEventTests).Assembly);
        services.AddInfrastructure("Data Source=:memory:");
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var before = PingHandler.Received;

        await scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>().PublishAsync(new Ping());

        Assert.Equal(before + 1, PingHandler.Received);
    }
}
