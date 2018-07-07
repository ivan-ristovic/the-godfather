#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Common.Converters
{
    public class CustomBoolConverter : IArgumentConverter<bool>
    {
        public static bool? TryConvert(string value)
        {
            bool result = false;
            bool parses = true;
            switch (value.ToLowerInvariant()) {
                case "t":
                case "y":
                case "ye":
                case "ya":
                case "yup":
                case "yee":
                case "yes":
                case "yea":
                case "yeah":
                case "1":
                    result = true;
                    break;
                case "f":
                case "n":
                case "nah":
                case "nope":
                case "nada":
                case "no":
                case "0":
                    result = false;
                    break;
                default:
                    parses = bool.TryParse(value, out result);
                    break;
            }
            if (parses)
                return result;
            else
                return null;
        }


        public Task<Optional<bool>> ConvertAsync(string value, CommandContext ctx)
        {
            bool? b = TryConvert(value);
            if (b.HasValue)
                return Task.FromResult(new Optional<bool>(b.Value));
            else
                return Task.FromResult(new Optional<bool>());
        }
    }
}