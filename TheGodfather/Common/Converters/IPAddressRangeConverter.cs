using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace TheGodfather.Common.Converters
{
    public class IPAddressRangeConverter : IArgumentConverter<IPAddressRange>
    {
        public Task<Optional<IPAddressRange>> ConvertAsync(string value, CommandContext ctx)
        {
            return IPAddressRange.TryParse(value, out IPAddressRange ip)
                ? Task.FromResult(new Optional<IPAddressRange>(ip))
                : Task.FromResult(new Optional<IPAddressRange>());
        }
    }
}