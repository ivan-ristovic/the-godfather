#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

using System.Net;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Common.Converters
{
    public class CustomIPAddressConverter : IArgumentConverter<IPAddress>
    {
        public Task<Optional<IPAddress>> ConvertAsync(string value, CommandContext ctx)
        {
            if (!IPAddress.TryParse(value, out IPAddress ip))
                return Task.FromResult(new Optional<IPAddress>());
            return Task.FromResult(new Optional<IPAddress>(ip));
        }
    }
}