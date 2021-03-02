using System.Linq;
using DSharpPlus.Entities;
using TheGodfather.Modules.Polls.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Polls.Extensions
{
    public static class PollExtensions
    {
        public static DiscordEmbed ToDiscordEmbed(this Poll poll, LocalizationService lcs)
        {
            var emb = new LocalizedEmbedBuilder(lcs, poll.Channel.GuildId);
            emb.WithTitle(poll.Question);
            emb.WithLocalizedDescription("str-vote-text");
            emb.WithColor(DiscordColor.Orange);

            for (int i = 0; i < poll.Options.Count; i++) {
                if (!string.IsNullOrWhiteSpace(poll.Options[i]))
                    emb.AddField($"{i + 1} : {poll.Options[i]}", $"{poll.Results.Count(kvp => kvp.Value == i)}");
            }

            if (poll.EndTime is { }) {
                string localizedTime = lcs.GetLocalizedTimeString(poll.Channel.GuildId, poll.EndTime);
                if (poll.TimeUntilEnd.TotalSeconds > 1)
                    emb.WithLocalizedFooter("fmt-poll-end", poll.Initiator.AvatarUrl, localizedTime, $"{poll.TimeUntilEnd:hh\\:mm\\:ss}");
                else
                    emb.WithLocalizedFooter("fmt-poll-end", poll.Initiator.AvatarUrl);
            }

            return emb.Build();
        }

        public static DiscordEmbed ResultsToDiscordEmbed(this Poll poll, LocalizationService lcs)
        {
            var emb = new LocalizedEmbedBuilder(lcs, poll.Channel.GuildId);
            emb.WithLocalizedTitle("fmt-poll-res", poll.Question);
            emb.WithColor(DiscordColor.Orange);

            for (int i = 0; i < poll.Options.Count; i++)
                emb.AddField(poll.Options[i], poll.Results.Count(kvp => kvp.Value == i).ToString(), inline: true);

            emb.WithLocalizedFooter("fmt-poll-by", poll.Initiator.AvatarUrl, poll.Initiator.DisplayName);

            if (poll.Results is null || !poll.Results.Any())
                return emb.WithLocalizedDescription("str-poll-none").Build();

            return emb.Build();
        }

    }
}
