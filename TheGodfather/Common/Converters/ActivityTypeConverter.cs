using System.Text.RegularExpressions;
using DSharpPlus.Entities;

namespace TheGodfather.Common.Converters
{
    public sealed class ActivityTypeConverter : BaseArgumentConverter<ActivityType>
    {
        private static readonly Regex _listeningRegex;
        private static readonly Regex _playingRegex;
        private static readonly Regex _streamingRegex;
        private static readonly Regex _watchingRegex;
        private static readonly Regex _competingRegex;


        static ActivityTypeConverter()
        {
            _listeningRegex = new Regex(@"^l+(i+s+t+e+n+([sz]+|i+n+g+)?)?(to)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _playingRegex = new Regex(@"^p+(l+a+y+([sz]+|i+n+g+)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _streamingRegex = new Regex(@"^s+(t+r+e+a+m+(e*[sz]+|i+n+g+)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _watchingRegex = new Regex(@"^w+(a+t+c+h+(e*[sz]+|i+n+g+)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _competingRegex = new Regex(@"^c+o+m+p+e+t+i+n+g(\s+i+n+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }


        public override bool TryConvert(string value, out ActivityType result)
        {
            result = ActivityType.Playing;
            bool parses = true;

            if (_playingRegex.IsMatch(value))
                result = ActivityType.Playing;
            else if (_watchingRegex.IsMatch(value))
                result = ActivityType.Watching;
            else if (_listeningRegex.IsMatch(value))
                result = ActivityType.ListeningTo;
            else if (_streamingRegex.IsMatch(value))
                result = ActivityType.Streaming;
            else if (_competingRegex.IsMatch(value))
                result = ActivityType.Competing;
            else
                parses = false;

            return parses;
        }
    }
}