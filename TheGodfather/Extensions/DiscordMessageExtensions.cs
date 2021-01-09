using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace TheGodfather.Extensions
{
    public static class DiscordMessageExtensions
    {
        public static async Task<int> GetReactionsCountAsync(this DiscordMessage msg, DiscordEmoji emoji)
        {
            msg = await msg.Channel.GetMessageAsync(msg.Id);
            return GetReactionsCount(msg, emoji);
        }

        public static int GetReactionsCount(this DiscordMessage msg, DiscordEmoji emoji)
        {
            string emojiName = emoji.GetDiscordName();
            return msg.Reactions.FirstOrDefault(r => r.Emoji.GetDiscordName() == emojiName)?.Count ?? 0;
        }
    }
}
