using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;

namespace TheGodfather.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedServices(this IServiceCollection serviceCollection, Assembly? assembly = null)
        {
            IEnumerable<Type> services = GetServiceTypes(assembly)
                .Except(serviceCollection.Select(s => s.ServiceType));

            foreach (Type service in services) {
                // TODO remove
                if (service.GetConstructors().Any(c => c.GetParameters().Any(p => p.ParameterType == typeof(TheGodfatherShard))))
                    continue;
                // END remove
                serviceCollection.AddSingleton(service);
                Log.Verbose("Added service: {Service}", service.FullName);
            }

            return serviceCollection;
        }

        // TODO refactor services so that they do not take gf shard as argument
        [Obsolete]
        public static IServiceCollection AddShardServices(this IServiceCollection serviceCollection, TheGodfatherShard shard, Assembly? assembly = null)
        {
            return serviceCollection
                .AddSingleton(new AntifloodService(shard))
                .AddSingleton(new AntiInstantLeaveService(shard))
                .AddSingleton(new AntispamService(shard))
                .AddSingleton(new LinkfilterService(shard))
                .AddSingleton(new RatelimitService(shard))
                .AddSingleton(s => new SchedulingService(shard, s.GetRequiredService<AsyncExecutionService>()))
                ;
        }

        public static ServiceProvider Initialize(this ServiceProvider provider, Assembly? assembly = null)
        {
            provider.ConfigureAwait(false);
            IEnumerable<Type> serviceTypes = GetServiceTypes(assembly);
            foreach (Type serviceType in serviceTypes)
                _ = provider.GetService(serviceType);
            return provider;
        }


        private static IEnumerable<Type> GetServiceTypes(Assembly? assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();
            Type gfService = typeof(ITheGodfatherService);
            return assembly.GetTypes().Where(t => gfService.IsAssignableFrom(t) && !t.IsAbstract);
        }
    }
}
