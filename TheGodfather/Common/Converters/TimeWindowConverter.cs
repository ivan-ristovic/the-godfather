using System.Text.RegularExpressions;
using Imgur.API.Enums;

namespace TheGodfather.Common.Converters
{
    public sealed class TimeWindowConverter : BaseArgumentConverter<TimeWindow>
    {
        private static readonly Regex _allRegex;
        private static readonly Regex _dayRegex;
        private static readonly Regex _weekRegex;
        private static readonly Regex _monthRegex;
        private static readonly Regex _yearRegex;


        static TimeWindowConverter()
        {
            _allRegex = new Regex(@"^a(ll)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _dayRegex = new Regex(@"^d(ay)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _weekRegex = new Regex(@"^w(eek)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _monthRegex = new Regex(@"^m(onth)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _yearRegex = new Regex(@"^y(ear)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }


        public override bool TryConvert(string value, out TimeWindow result)
        {
            result = TimeWindow.All;
            bool parses = true;

            if (_dayRegex.IsMatch(value))
                result = TimeWindow.Day;
            else if (_weekRegex.IsMatch(value))
                result = TimeWindow.Week;
            else if (_monthRegex.IsMatch(value))
                result = TimeWindow.Month;
            else if (_yearRegex.IsMatch(value))
                result = TimeWindow.Year;
            else if (_allRegex.IsMatch(value))
                result = TimeWindow.All;
            else
                parses = false;

            return parses;
        }
    }
}