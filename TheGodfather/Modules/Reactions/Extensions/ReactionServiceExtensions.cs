using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using TheGodfather.Modules.Reactions.Services;

namespace TheGodfather.Modules.Reactions.Extensions
{
    public static class ReactionServiceExtensions
    {
        public static Task<int> AddEmojiReactionEAsync(this ReactionsService service, ulong gid, DiscordEmoji emoji, IEnumerable<string> triggers, bool regex)
            => service.AddEmojiReactionAsync(gid, emoji.GetDiscordName(), triggers, regex);

        public static Task<int> RemoveEmojiReactionsEAsync(this ReactionsService service, ulong gid, DiscordEmoji emoji)
            => service.RemoveEmojiReactionsAsync(gid, emoji.GetDiscordName());
    }
}
