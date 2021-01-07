using System.Text.RegularExpressions;
using TheGodfather.Modules.Music.Common;

namespace TheGodfather.Common.Converters
{
    public sealed class RepeatModeConverter : BaseArgumentConverter<RepeatMode>
    {
        private static readonly Regex _noneRegex;
        private static readonly Regex _singleRegex;
        private static readonly Regex _allRegex;


        static RepeatModeConverter()
        {
            _noneRegex = new Regex(@"^n(one)|0?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _singleRegex = new Regex(@"^o(ne)?|1$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _allRegex = new Regex(@"^a(ll)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }


        public override bool TryConvert(string value, out RepeatMode result)
        {
            result = RepeatMode.None;
            bool parses = true;

            if (_singleRegex.IsMatch(value))
                result = RepeatMode.Single;
            else if (_allRegex.IsMatch(value))
                result = RepeatMode.All;
            else if (_noneRegex.IsMatch(value))
                result = RepeatMode.None;
            else
                parses = false;

            return parses;
        }
    }
}