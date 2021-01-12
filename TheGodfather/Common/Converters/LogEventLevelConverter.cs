using System.Text.RegularExpressions;
using Serilog.Events;

namespace TheGodfather.Common.Converters
{
    public class LogEventLevelConverter : BaseArgumentConverter<LogEventLevel>
    {
        private static readonly Regex _dbgRegex;
        private static readonly Regex _errRegex;
        private static readonly Regex _ftlRegex;
        private static readonly Regex _infRegex;
        private static readonly Regex _vrbRegex;
        private static readonly Regex _wrnRegex;


        static LogEventLevelConverter()
        {
            _dbgRegex = new Regex(@"^d(e?bu?g)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _errRegex = new Regex(@"^e(rr(or)?)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _ftlRegex = new Regex(@"^f(a?tal?)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _infRegex = new Regex(@"^i(nf(ormation)?)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _vrbRegex = new Regex(@"^v(e?rb(ose)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _wrnRegex = new Regex(@"^w(a?rn(ing)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }


        public override bool TryConvert(string value, out LogEventLevel result)
        {
            result = LogEventLevel.Information;
            bool parses = true;

            if (_dbgRegex.IsMatch(value))
                result = LogEventLevel.Debug;
            else if (_errRegex.IsMatch(value))
                result = LogEventLevel.Error;
            else if (_ftlRegex.IsMatch(value))
                result = LogEventLevel.Fatal;
            else if (_infRegex.IsMatch(value))
                result = LogEventLevel.Information;
            else if (_vrbRegex.IsMatch(value))
                result = LogEventLevel.Verbose;
            else if (_wrnRegex.IsMatch(value))
                result = LogEventLevel.Warning;
            else
                parses = false;

            return parses;
        }
    }
}