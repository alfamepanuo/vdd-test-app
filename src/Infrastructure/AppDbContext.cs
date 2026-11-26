using Application;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class AppDbContext : DbContext, IUnitOfWork
{
    private readonly IDomainEventDispatcher _dispatcher;

    public AppDbContext(DbContextOptions<AppDbContext> options, IDomainEventDispatcher dispatcher)
        : this((DbContextOptions)options, dispatcher)
    {
    }

    protected AppDbContext(DbContextOptions options, IDomainEventDispatcher dispatcher)
        : base(options)
    {
        _dispatcher = dispatcher;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var aggregates = PendingAggregates();
        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();

        var result = base.SaveChanges(acceptAllChangesOnSuccess);

        _dispatcher.DispatchAsync(events, CancellationToken.None).GetAwaiter().GetResult();
        aggregates.ForEach(a => a.ClearDomainEvents());

        return result;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var aggregates = PendingAggregates();
        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();

        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        await _dispatcher.DispatchAsync(events, cancellationToken);
        aggregates.ForEach(a => a.ClearDomainEvents());

        return result;
    }

    private List<AggregateRoot> PendingAggregates() =>
        ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();
}
