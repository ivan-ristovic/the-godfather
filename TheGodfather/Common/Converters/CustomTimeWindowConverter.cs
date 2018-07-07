#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Imgur.API.Enums;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Common.Converters
{
    public class CustomTimeWindowConverter : IArgumentConverter<TimeWindow>
    {
        public Task<Optional<TimeWindow>> ConvertAsync(string value, CommandContext ctx)
        {
            TimeWindow result = TimeWindow.Day;
            bool parses = true;
            switch (value.ToLowerInvariant()) {
                case "day":
                case "d":
                case "24h":
                    result = TimeWindow.Day;
                    break;
                case "week":
                case "7d":
                case "w":
                    result = TimeWindow.Week;
                    break;
                case "month":
                case "m":
                    result = TimeWindow.Month;
                    break;
                case "year":
                case "y":
                    result = TimeWindow.Year;
                    break;
                case "all":
                case "a":
                    result = TimeWindow.All;
                    break;
                default:
                    parses = false;
                    break;
            }

            if (parses)
                return Task.FromResult(new Optional<TimeWindow>(result));
            else
                return Task.FromResult(new Optional<TimeWindow>());
        }
    }
}