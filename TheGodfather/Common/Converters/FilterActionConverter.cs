using System;
using System.Text.RegularExpressions;
using TheGodfather.Database.Models;

namespace TheGodfather.Common.Converters
{
    public class FilterActionConverter : BaseArgumentConverter<Filter.Action>
    {
        private static readonly Regex _dRegex;
        private static readonly Regex _sRegex;


        static FilterActionConverter()
        {
            _dRegex = new Regex(@"^d(el(ete)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _sRegex = new Regex(@"^(s(anitize|poiler)?|e(dit)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }


        public override bool TryConvert(string value, out Filter.Action result)
        {
            result = Filter.Action.Kick;
            bool parses = true;

            if (_dRegex.IsMatch(value))
                result = Filter.Action.DeleteMessage;
            else if (_sRegex.IsMatch(value))
                result = Filter.Action.Sanitize;
            else if (new PunishmentActionConverter().TryConvert(value, out Punishment.Action punishment))
                parses = Enum.TryParse(punishment.ToString(), out result);
            else
                parses = false;

            return parses;
        }
    }
}