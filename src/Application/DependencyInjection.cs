using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    private static readonly Type[] HandlerContracts =
    [
        typeof(IDomainEventHandler<>),
        typeof(IIntegrationEventHandler<>),
    ];

    public static IServiceCollection AddApplication(this IServiceCollection services, params System.Reflection.Assembly[] handlerAssemblies)
    {
        var assemblies = handlerAssemblies.Append(typeof(DependencyInjection).Assembly).Distinct();

        foreach (var type in assemblies.SelectMany(a => a.GetTypes()).Where(t => t is { IsClass: true, IsAbstract: false }))
        {
            foreach (var contract in type.GetInterfaces().Where(i => i.IsGenericType && HandlerContracts.Contains(i.GetGenericTypeDefinition())))
            {
                services.AddScoped(contract, type);
            }
        }

        return services;
    }
}
