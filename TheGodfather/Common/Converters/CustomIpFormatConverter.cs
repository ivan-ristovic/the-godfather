#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Common.Converters
{
    public class CustomIPFormatConverter : IArgumentConverter<CustomIpFormat>
    {
        public Task<Optional<CustomIpFormat>> ConvertAsync(string value, CommandContext ctx)
        {
            if (!CustomIpFormat.TryParse(value, out CustomIpFormat ip))
                return Task.FromResult(new Optional<CustomIpFormat>());
            return Task.FromResult(new Optional<CustomIpFormat>(ip));
        }
    }
}