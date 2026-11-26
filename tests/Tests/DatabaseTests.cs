using Application;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class DatabaseTests
{
    private sealed record Happened : IDomainEvent;

    private sealed class Thing : AggregateRoot
    {
        public Thing() => Raise(new Happened());
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options, IDomainEventDispatcher dispatcher)
        : AppDbContext(options, dispatcher)
    {
        public DbSet<Thing> Things => Set<Thing>();
    }

    private sealed class HandlerCounter
    {
        public int Count { get; set; }
    }

    private sealed class CountingHandler(HandlerCounter counter) : IDomainEventHandler<Happened>
    {
        public Task HandleAsync(Happened domainEvent, CancellationToken cancellationToken)
        {
            counter.Count++;
            return Task.CompletedTask;
        }
    }

    private sealed class NoopDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    [Fact]
    public async Task In_memory_sqlite_scalar_query_returns_one()
    {
        using var db = new SqliteTestDb();
        await using var context = new TestDbContext(db.Options<TestDbContext>(), new NoopDispatcher());

        var result = await context.Database.SqlQueryRaw<int>("SELECT 1 AS Value").SingleAsync();

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Save_dispatches_domain_events_once_and_clears_them()
    {
        using var db = new SqliteTestDb();
        var counter = new HandlerCounter();
        var services = new ServiceCollection();
        services.AddSingleton(counter);
        services.AddApplication(typeof(DatabaseTests).Assembly);
        services.AddInfrastructure("Data Source=:memory:");
        services.AddDbContext<TestDbContext>(o => o.UseSqlite(db.Connection));
        await using var provider = services.BuildServiceProvider(validateScopes: true);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();
        var thing = new Thing();
        context.Things.Add(thing);

        await context.SaveChangesAsync();

        Assert.Equal(1, counter.Count);
        Assert.Empty(thing.DomainEvents);
    }
}
