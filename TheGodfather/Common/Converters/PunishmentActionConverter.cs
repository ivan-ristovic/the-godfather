using System.Text.RegularExpressions;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Common.Converters
{
    public class PunishmentActionConverter : BaseArgumentConverter<PunishmentAction>
    {
        private static readonly Regex _pmRegex = new Regex(@"^(p(erm(a(nent(al+y?)?)?)?)?)?(m+(u+t+e*d*)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _tmRegex = new Regex(@"^t(e?mp(ora(l|ry))?)?(m+(u+t+e*d*)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _pbRegex = new Regex(@"^(p(erm(a(nent(al+y?)?)?)?)?)?(b+([ae]+n+e*d*)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _tbRegex = new Regex(@"^t(e?mp(ora(l|ry))?)?(b+([ae]+n+e*d*)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _kRegex = new Regex(@"^k+(i+c*k+e*d*)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override bool TryConvert(string value, out PunishmentAction result)
        {
            result = PunishmentAction.Kick;
            bool parses = true;

            if (_kRegex.IsMatch(value))
                result = PunishmentAction.Kick;
            else if (_tbRegex.IsMatch(value))
                result = PunishmentAction.TemporaryBan;
            else if (_tmRegex.IsMatch(value))
                result = PunishmentAction.TemporaryMute;
            else if (_pbRegex.IsMatch(value))
                result = PunishmentAction.PermanentBan;
            else if (_pmRegex.IsMatch(value))
                result = PunishmentAction.PermanentMute;
            else
                parses = false;

            return parses;
        }
    }
}