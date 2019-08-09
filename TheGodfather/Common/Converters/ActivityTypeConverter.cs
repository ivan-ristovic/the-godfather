using System.Text.RegularExpressions;
using DSharpPlus.Entities;

namespace TheGodfather.Common.Converters
{
    public sealed class ActivityTypeConverter : BaseArgumentConverter<ActivityType>
    {
        private static readonly Regex _lRegex = new Regex(@"^l+(i+s+t+e+n+([sz]+|i+n+g+)?)?(to)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _pRegex = new Regex(@"^p+(l+a+y+([sz]+|i+n+g+)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _sRegex = new Regex(@"^s+(t+r+e+a+m+(e*[sz]+|i+n+g+)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _wRegex = new Regex(@"^w+(a+t+c+h+(e*[sz]+|i+n+g+)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);


        public override bool TryConvert(string value, out ActivityType result)
        {
            result = ActivityType.Playing;
            bool parses = true;

            if (_pRegex.IsMatch(value))
                result = ActivityType.Playing;
            else if (_wRegex.IsMatch(value))
                result = ActivityType.Watching;
            else if (_lRegex.IsMatch(value))
                result = ActivityType.ListeningTo;
            else if (_sRegex.IsMatch(value))
                result = ActivityType.Streaming;
            else
                parses = false;

            return parses;
        }
    }
}