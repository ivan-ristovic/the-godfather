#region USING_DIRECTIVES
using System.Text.RegularExpressions;
#endregion

namespace TheGodfather.Common
{
    public class CustomIpFormat
    {
        private static readonly Regex _parseRegex = new Regex(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|\*)(\.|:|$)){1,4}([0-9]{4,5})?$", RegexOptions.Compiled);


        public string Content { get; set; }


        public static bool TryParse(string str, out CustomIpFormat res)
        {
            if (_parseRegex.IsMatch(str)) {
                res = new CustomIpFormat() { Content = str };
                return true;
            } else {
                res = null;
                return false;
            }
        }
    }
}
