#region USING_DIRECTIVES
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("weather"), Module(ModuleType.Searches)]
    [Description("Weather search commands. If invoked without subcommands, returns weather information for given query.")]
    [Aliases("w")]
    [UsageExample("!weather london")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class WeatherModule : TheGodfatherServiceModule<WeatherService>
    {

        public WeatherModule(WeatherService weather) : base(weather) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("You need to specify a query (city usually).");

            var em = await _Service.GetEmbeddedCurrentWeatherDataAsync(query)
                .ConfigureAwait(false);
            if (em == null)
                throw new CommandFailedException("Cannot find weather data for given query.");

            await ctx.RespondAsync(embed: em)
                .ConfigureAwait(false);
        }


        #region COMMAND_WEATHER_FORECAST
        [Command("forecast"), Priority(1)]
        [Module(ModuleType.Searches)]
        [Description("Get weather forecast for the following days (def: 7).")]
        [Aliases("f")]
        [UsageExample("!weather forecast london")]
        [UsageExample("!weather forecast 5 london")]
        public async Task ForecastAsync(CommandContext ctx,
                                       [Description("Amount of days to fetch the forecast for.")] int amount,
                                       [RemainingText, Description("Query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("You need to specify a query (city usually).");

            if (amount < 1)
                throw new InvalidCommandUsageException("Amount of days cannot be less than one.");

            var ems = await _Service.GetEmbeddedWeatherForecastAsync(query, amount)
                .ConfigureAwait(false);
            if (ems == null || !ems.Any())
                throw new CommandFailedException("Cannot find weather data for given query.");

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, ems.Select(e => new Page() { Embed = e }))
                .ConfigureAwait(false);
        }

        [Command("forecast"), Priority(0)]
        public async Task ForecastAsync(CommandContext ctx,
                                       [RemainingText, Description("Query.")] string query)
            => await ForecastAsync(ctx, 7, query).ConfigureAwait(false);
        #endregion
    }
}
