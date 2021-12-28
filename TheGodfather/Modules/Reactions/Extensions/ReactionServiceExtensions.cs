using DSharpPlus;
using DSharpPlus.Entities;
using TheGodfather.Modules.Reactions.Services;

namespace TheGodfather.Modules.Reactions.Extensions;

public static class ReactionServiceExtensions
{
    public static Task<int> AddEmojiReactionEmojiAsync(this ReactionsService service, ulong gid, DiscordEmoji emoji, IEnumerable<string> triggers, bool regex)
        => service.AddEmojiReactionAsync(gid, emoji.GetDiscordName(), triggers, regex);

    public static Task<int> RemoveEmojiReactionsEmojiAsync(this ReactionsService service, ulong gid, DiscordEmoji emoji)
        => service.RemoveEmojiReactionsAsync(gid, emoji.GetDiscordName());

    public static async Task HandleEmojiReactionsAsync(this ReactionsService service, DiscordClient shard, DiscordMessage msg)
    {
        if (msg.Channel.GuildId is null)
            return;

        ulong gid = msg.Channel.GuildId.Value;

        EmojiReaction? er = service.FindMatchingEmojiReactions(gid, msg.Content)
            .Shuffle()
            .FirstOrDefault();

        if (er is null)
            return;

        try {
            var emoji = DiscordEmoji.FromName(shard, er.Response);
            await msg.CreateReactionAsync(emoji);
        } catch (ArgumentException) {
            await service.RemoveEmojiReactionsWhereAsync(gid, r => r.HasSameResponseAs(er));
        }
    }

    public static async Task HandleTextReactionsAsync(this ReactionsService service, DiscordMessage msg)
    {
        if (msg.Channel.GuildId is null)
            return;

        TextReaction? tr = service.FindMatchingTextReaction(msg.Channel.GuildId.Value, msg.Content);
        if (tr is { } && tr.CanSend())
            await msg.Channel.SendMessageAsync(tr.Response.Replace("%user%", msg.Author.Mention));
    }
}