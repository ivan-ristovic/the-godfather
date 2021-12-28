using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TheGodfather.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection serviceCollection, Assembly? assembly = null)
    {
        IEnumerable<Type> services = GetServiceTypes(assembly)
            .Except(serviceCollection.Select(s => s.ServiceType));

        foreach (Type service in services) {
            serviceCollection.AddSingleton(service);
            Log.Verbose("Added service: {Service}", service.FullName);
        }

        return serviceCollection;
    }

    public static ServiceProvider Initialize(this ServiceProvider provider, Assembly? assembly = null)
    {
        provider.ConfigureAwait(false);
        IEnumerable<Type> serviceTypes = GetServiceTypes(assembly);
        foreach (Type serviceType in serviceTypes)
            _ = provider.GetRequiredService(serviceType);
        return provider;
    }

    private static IEnumerable<Type> GetServiceTypes(Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        Type gfService = typeof(ITheGodfatherService);
        return assembly.GetTypes().Where(t => gfService.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
    }
}