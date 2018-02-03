using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace TheGodfather.Extensions
{
    public class CustomColorConverter : IArgumentConverter<DiscordColor>
    {
        public async Task<Optional<DiscordColor>> ConvertAsync(string value, CommandContext ctx)
        {
            await Task.Delay(0);

            DiscordColor color;
            try {
                color = new DiscordColor(value);
            } catch {
                return new Optional<DiscordColor>();
            }

            return new Optional<DiscordColor>(color);
        }
    }
}