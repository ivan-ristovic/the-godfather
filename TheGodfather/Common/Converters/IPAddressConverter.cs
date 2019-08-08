using System.Net;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace TheGodfather.Common.Converters
{
    public class IPAddressConverter : IArgumentConverter<IPAddress>
    {
        public Task<Optional<IPAddress>> ConvertAsync(string value, CommandContext ctx)
        {
            return IPAddress.TryParse(value, out IPAddress ip)
                ? Task.FromResult(new Optional<IPAddress>(ip))
                : Task.FromResult(new Optional<IPAddress>());
        }
    }
}