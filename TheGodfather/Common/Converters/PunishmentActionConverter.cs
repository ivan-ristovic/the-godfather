using System.Text.RegularExpressions;
using TheGodfather.Database.Models;

namespace TheGodfather.Common.Converters
{
    public class PunishmentActionConverter : BaseArgumentConverter<Punishment.Action>
    {
        private static readonly Regex _pmRegex;
        private static readonly Regex _tmRegex;
        private static readonly Regex _pbRegex;
        private static readonly Regex _tbRegex;
        private static readonly Regex _kRegex;


        static PunishmentActionConverter()
        {
            _pmRegex = new Regex(@"^(p(erm(a(nent(al+y?)?)?)?)?)?(m+(u+t+e*d*)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _tmRegex = new Regex(@"^t(e?mp(ora(l|ry))?)?(m+(u+t+e*d*)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _pbRegex = new Regex(@"^(p(erm(a(nent(al+y?)?)?)?)?)?(b+([ae]+n+e*d*)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _tbRegex = new Regex(@"^t(e?mp(ora(l|ry))?)?(b+([ae]+n+e*d*)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _kRegex = new Regex(@"^k+(i+c*k+e*d*)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }


        public override bool TryConvert(string value, out Punishment.Action result)
        {
            result = Punishment.Action.Kick;
            bool parses = true;

            if (_kRegex.IsMatch(value))
                result = Punishment.Action.Kick;
            else if (_tbRegex.IsMatch(value))
                result = Punishment.Action.TemporaryBan;
            else if (_tmRegex.IsMatch(value))
                result = Punishment.Action.TemporaryMute;
            else if (_pbRegex.IsMatch(value))
                result = Punishment.Action.PermanentBan;
            else if (_pmRegex.IsMatch(value))
                result = Punishment.Action.PermanentMute;
            else
                parses = false;

            return parses;
        }
    }
}