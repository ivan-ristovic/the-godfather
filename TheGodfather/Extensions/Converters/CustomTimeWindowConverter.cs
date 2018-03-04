#region USING_DIRECTIVES
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

using Imgur.API.Enums;
#endregion

namespace TheGodfather.Extensions.Converters
{
    public class CustomTimeWindowConverter : IArgumentConverter<TimeWindow>
    {
        public async Task<Optional<TimeWindow>> ConvertAsync(string value, CommandContext ctx)
        {
            await Task.Delay(0);

            TimeWindow t = TimeWindow.Day;
            bool parses = true;
            switch (value.ToLowerInvariant()) {
                case "day":
                case "d":
                    t = TimeWindow.Day;
                    break;
                case "week":
                case "7d":
                case "w":
                    t = TimeWindow.Week;
                    break;
                case "month":
                case "m":
                    t = TimeWindow.Month;
                    break;
                case "year":
                case "y":
                    t = TimeWindow.Year;
                    break;
                case "all":
                case "a":
                    t = TimeWindow.All;
                    break;
                default:
                    parses = false;
                    break;
            }

            if (parses)
                return new Optional<TimeWindow>(t);
            else
                return new Optional<TimeWindow>();
        }
    }
}