using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace TheGodfather.Modules.Games.Extensions
{
    public static class DiscordMessageExtensions
    {
        public static async Task<DiscordMessage> ModifyOrResendAsync(this DiscordMessage? msg, DiscordChannel chn, DiscordEmbed emb)
        {
            try {
                if (msg is { })
                    msg = await msg.ModifyAsync(embed: emb);
            } catch {
                msg = null;
            }

            if (msg is null)
                msg = await chn.SendMessageAsync(embed: emb);

            return msg;
        }
    }
}
