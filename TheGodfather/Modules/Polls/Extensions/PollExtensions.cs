using DSharpPlus.Entities;
using TheGodfather.Modules.Polls.Common;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Polls.Extensions;

public static class PollExtensions
{
    public static DiscordEmbed ToDiscordEmbed(this Poll poll, LocalizationService lcs)
    {
        var emb = new LocalizedEmbedBuilder(lcs, poll.Channel.GuildId);
        emb.WithTitle(poll.Question);
        emb.WithLocalizedDescription(TranslationKey.str_vote_text);
        emb.WithColor(DiscordColor.Orange);

        for (int i = 0; i < poll.Options.Count; i++)
            if (!string.IsNullOrWhiteSpace(poll.Options[i]))
                emb.AddField($"{i + 1} : {poll.Options[i]}", $"{poll.Results.Count(kvp => kvp.Value == i)}", true);

        if (poll.EndTime is not null) {
            string localizedTime = lcs.GetLocalizedTimeString(poll.Channel.GuildId, poll.EndTime);
            if (poll.TimeUntilEnd.TotalSeconds > 1)
                emb.WithLocalizedFooter(TranslationKey.fmt_poll_end(localizedTime, $"{poll.TimeUntilEnd:hh\\:mm\\:ss}"), poll.Initiator.AvatarUrl);
            else
                emb.WithLocalizedFooter(TranslationKey.fmt_poll_ended, poll.Initiator.AvatarUrl);
        }

        return emb.Build();
    }

    public static DiscordEmbed ResultsToDiscordEmbed(this Poll poll, LocalizationService lcs)
    {
        var emb = new LocalizedEmbedBuilder(lcs, poll.Channel.GuildId);
        emb.WithLocalizedTitle(TranslationKey.fmt_poll_res(poll.Question));
        emb.WithColor(DiscordColor.Orange);

        for (int i = 0; i < poll.Options.Count; i++)
            emb.AddField(poll.Options[i], poll.Results.Count(kvp => kvp.Value == i).ToString(), true);

        emb.WithLocalizedFooter(TranslationKey.fmt_poll_by(poll.Initiator.DisplayName), poll.Initiator.AvatarUrl);

        if (!poll.Results.Any())
            return emb.WithLocalizedDescription(TranslationKey.str_poll_none).Build();

        return emb.Build();
    }

}