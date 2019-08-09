using Imgur.API.Enums;

namespace TheGodfather.Common.Converters
{
    public class ImgurTimeWindowConverter : BaseArgumentConverter<TimeWindow>
    {
        // TODO remove, use TimeSpan and then convert to apropriate TimeWindow
        public override bool TryConvert(string value, out TimeWindow result)
        {
            result = TimeWindow.Day;
            bool parses = true;
            switch (value.ToLowerInvariant()) {
                case "day":
                case "24h":
                case "d":
                    result = TimeWindow.Day;
                    break;
                case "week":
                case "7d":
                case "w":
                    result = TimeWindow.Week;
                    break;
                case "month":
                case "1mo":
                case "1m":
                case "mo":
                case "m":
                    result = TimeWindow.Month;
                    break;
                case "year":
                case "1y":
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

            return parses;
        }
    }
}