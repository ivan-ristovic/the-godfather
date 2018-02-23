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
            if (string.Compare(value, "day", false) == 0 || string.Compare(value, "d", false) == 0)
                t = TimeWindow.Day;
            else if (string.Compare(value, "week", false) == 0 || string.Compare(value, "w", false) == 0)
                t = TimeWindow.Week;
            else if (string.Compare(value, "month", false) == 0 || string.Compare(value, "m", false) == 0)
                t = TimeWindow.Month;
            else if (string.Compare(value, "year", false) == 0 || string.Compare(value, "y", false) == 0)
                t = TimeWindow.Year;
            else if (string.Compare(value, "all", false) == 0 || string.Compare(value, "a", false) == 0)
                t = TimeWindow.All;
            else
                return new Optional<TimeWindow>();

            return new Optional<TimeWindow>(t);
        }
    }
}