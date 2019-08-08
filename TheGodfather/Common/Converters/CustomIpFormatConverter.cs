using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace TheGodfather.Common.Converters
{
    public class CustomIPFormatConverter : IArgumentConverter<CustomIPFormat>
    {
        public Task<Optional<CustomIPFormat>> ConvertAsync(string value, CommandContext ctx)
        {
            return CustomIPFormat.TryParse(value, out CustomIPFormat ip)
                ? Task.FromResult(new Optional<CustomIPFormat>(ip))
                : Task.FromResult(new Optional<CustomIPFormat>());
        }
    }
}