using Application;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

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

    private sealed class CountingHandler : IDomainEventHandler<Happened>
    {
        public int Count { get; private set; }

        public Task HandleAsync(Happened domainEvent, CancellationToken cancellationToken)
        {
            Count++;
            return Task.CompletedTask;
        }
    }

    private sealed class Dispatcher(CountingHandler handler) : IDomainEventDispatcher
    {
        public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            foreach (var e in domainEvents.OfType<Happened>())
            {
                await handler.HandleAsync(e, cancellationToken);
            }
        }
    }

    [Fact]
    public async Task In_memory_sqlite_scalar_query_returns_one()
    {
        using var db = new SqliteTestDb();
        await using var context = new TestDbContext(db.Options<TestDbContext>(), new Dispatcher(new CountingHandler()));

        var result = await context.Database.SqlQueryRaw<int>("SELECT 1 AS Value").SingleAsync();

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Save_dispatches_domain_events_once_and_clears_them()
    {
        using var db = new SqliteTestDb();
        var handler = new CountingHandler();
        await using var context = new TestDbContext(db.Options<TestDbContext>(), new Dispatcher(handler));
        await context.Database.EnsureCreatedAsync();
        var thing = new Thing();
        context.Things.Add(thing);

        await context.SaveChangesAsync();

        Assert.Equal(1, handler.Count);
        Assert.Empty(thing.DomainEvents);
    }
}
