using DSharpPlus;
using DSharpPlus.Entities;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Misc.Extensions;

public static class StarboardMessageExtensions
{
    public static DiscordEmbed ToStarboardEmbed(this DiscordMessage msg, LocalizationService lcs, DiscordEmoji star, int count)
    {
        var emb = new LocalizedEmbedBuilder(lcs, msg.Channel.Guild.Id);
        emb.WithColor(DiscordColor.Gold);
        emb.WithUrl(msg.JumpLink);
        emb.WithAuthor(msg.Author.ToDiscriminatorString(), iconUrl: msg.Author.AvatarUrl);
        emb.WithDescription(msg.Content?.Truncate(DiscordLimits.EmbedDescriptionLimit - 5, " ..."), false);
        emb.AddLocalizedField(TranslationKey.str_votes, $"{Formatter.Bold(count.ToString())} {star}", true);
        emb.AddLocalizedField(TranslationKey.str_chn, msg.Channel.Mention, true);

        string jumplink = Formatter.MaskedUrl(lcs.GetString(msg.Channel.Guild.Id, TranslationKey.str_jumplink), msg.JumpLink);
        emb.AddLocalizedField(TranslationKey.str_link, jumplink, true);

        string? url = msg.Attachments
                .Select(a => a.Url)
                .FirstOrDefault(u => u.EndsWith(".jpg") || u.EndsWith(".png") || u.EndsWith(".jpeg") || u.EndsWith(".gif"))
            ;
        if (url is not null)
            emb.WithImageUrl(url);

        emb.WithLocalizedTimestamp(msg.CreationTimestamp);
        return emb.Build();
    }
}