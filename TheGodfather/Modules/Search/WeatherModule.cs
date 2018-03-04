#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Search
{
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class WeatherModule : TheGodfatherServiceModule<WeatherService>
    {

        public WeatherModule(WeatherService weather) : base(weather) { }


        #region COMMAND_WEATHER
        [Command("weather")]
        [Description("Return weather information for given query.")]
        [Aliases("w")]
        [UsageExample("!weather london")]
        public async Task RandomAsync(CommandContext ctx,
                                     [RemainingText, Description("Query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("You need to specify a query (city usually).");

            var em = await _Service.GetEmbeddedWeatherDataAsync(query)
                .ConfigureAwait(false);
            if (em == null)
                throw new CommandFailedException("Cannot find weather data for given query.");

            await ctx.RespondAsync(embed: em)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
