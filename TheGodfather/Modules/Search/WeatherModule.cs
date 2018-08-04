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
    [UsageExamples("!weather london")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class WeatherModule : TheGodfatherServiceModule<WeatherService>
    {

        public WeatherModule(WeatherService weather) : base(weather) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Query.")] string query)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("You need to specify a query (city usually).");

            var em = await this.Service.GetEmbeddedCurrentWeatherDataAsync(query)
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
        [UsageExamples("!weather forecast london",
                       "!weather forecast 5 london")]
        public async Task ForecastAsync(CommandContext ctx,
                                       [Description("Amount of days to fetch the forecast for.")] int amount,
                                       [RemainingText, Description("Query.")] string query)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("You need to specify a query (city usually).");

            if (amount < 1)
                throw new InvalidCommandUsageException("Amount of days cannot be less than one.");

            var ems = await this.Service.GetEmbeddedWeatherForecastAsync(query, amount)
                .ConfigureAwait(false);
            if (ems == null || !ems.Any())
                throw new CommandFailedException("Cannot find weather data for given query.");

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, ems.Select(e => new Page() { Embed = e }))
                .ConfigureAwait(false);
        }

        [Command("forecast"), Priority(0)]
        public Task ForecastAsync(CommandContext ctx,
                                 [RemainingText, Description("Query.")] string query)
            => ForecastAsync(ctx, 7, query);
        #endregion
    }
}
