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
    [Group("weather")]
    [Description("Weather search commands. If invoked without subcommands, returns weather information for given query.")]
    [Aliases("w")]
    [UsageExample("!weather london")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
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
        [Command("forecast")]
        [Description("Get weather forecast for next 7 days.")]
        [Aliases("f")]
        [UsageExample("!weather forecast london")]
        public async Task ForecastAsync(CommandContext ctx,
                                       [RemainingText, Description("Query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("You need to specify a query (city usually).");

            var ems = await _Service.GetEmbeddedWeatherForecastAsync(query)
                .ConfigureAwait(false);
            if (ems == null || !ems.Any())
                throw new CommandFailedException("Cannot find weather data for given query.");

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, ems.Select(e => new Page() { Embed = e }))
                .ConfigureAwait(false);
        }
        #endregion
    }
}
