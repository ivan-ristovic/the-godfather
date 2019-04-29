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
            bool result;
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
                case "on":
                case "enable":
                case "1":
                    result = true;
                    break;
                case "f":
                case "n":
                case "nah":
                case "nope":
                case "nada":
                case "no":
                case "off":
                case "disable":
                case "0":
                    result = false;
                    break;
                default:
                    parses = bool.TryParse(value, out result);
                    break;
            }

            return parses ? result : (bool?)null;
        }


        public Task<Optional<bool>> ConvertAsync(string value, CommandContext ctx)
            => Task.FromResult(new Optional<bool>(TryConvert(value).GetValueOrDefault()));
    }
}