using DSharpPlus.Entities;

namespace TheGodfather.Modules.Games.Extensions;

public static class DiscordMessageExtensions
{
    public static async Task<DiscordMessage> ModifyOrResendAsync(this DiscordMessage? msg, DiscordChannel chn, DiscordEmbed emb)
    {
        try {
            if (msg is not null)
                msg = await msg.ModifyAsync(emb);
        } catch {
            msg = null;
        }

        if (msg is null)
            msg = await chn.SendMessageAsync(emb);

        return msg;
    }
}