using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Misc.Services;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Owner.Services;
using TheGodfather.Modules.Reactions.Services;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Services
{
    public static class BotServiceCollectionProvider
    {
        public static IServiceCollection AddSharedServices(IServiceCollection services)
        {
            return services
                .AddSingleton<GuildConfigService>()
                .AddSingleton<BlockingService>()
                .AddSingleton<FilteringService>()
                .AddSingleton<ChannelEventService>()
                .AddSingleton<GiphyService>()
                .AddSingleton<GoodreadsService>()
                .AddSingleton<ImgurService>()
                .AddSingleton<InteractivityService>()
                .AddSingleton<LocalizationService>()
                .AddSingleton<LoggingService>()
                .AddSingleton<OMDbService>()
                .AddSingleton<ReactionsService>()
                .AddSingleton<SteamService>()
                .AddSingleton<UserRanksService>()
                .AddSingleton<WeatherService>()
                .AddSingleton<YtService>()
                ;
        }

        public static IServiceCollection AddShardSpecificServices(IServiceCollection sharedServices, TheGodfatherShard shard)
        {
            return sharedServices
                .AddSingleton(new AntifloodService(shard))
                .AddSingleton(new AntiInstantLeaveService(shard))
                .AddSingleton(new AntispamService(shard))
                .AddSingleton(new LinkfilterService(shard))
                .AddSingleton(new RatelimitService(shard))
                .AddSingleton(s => new SchedulingService(shard, s.GetRequiredService<AsyncExecutionService>()))
                ;
        }
    }
}
