#region USING_DIRECTIVES
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Extensions
{
    public class CustomBoolConverter : IArgumentConverter<bool>
    {
        public async Task<Optional<bool>> ConvertAsync(string value, CommandContext ctx)
        {
            await Task.Delay(0);
            bool result = false;
            bool parses = true;
            switch (value.ToLower()) {
                case "t":
                case "y":
                case "ye":
                case "ya":
                case "yup":
                case "yee":
                case "yes":
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
                return new Optional<bool>(result);
            else
                return new Optional<bool>();
        }
    }
}