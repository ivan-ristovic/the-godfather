using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.EventHandling;
using TheGodfather.Modules.Polls.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Polls.Extensions
{
    public static class ReactionsPollExtensions
    {
        private static readonly ImmutableDictionary<string, int> _emojiid = new Dictionary<string, int> {
            { "1\u20e3" , 0 }, { ":one:" , 0 },
            { "2\u20e3" , 1 }, { ":two:" , 1 },
            { "3\u20e3" , 2 }, { ":three:" , 2 },
            { "4\u20e3" , 3 }, { ":four:" , 3 },
            { "5\u20e3" , 4 }, { ":five:" , 4 },
            { "6\u20e3" , 5 }, { ":six:" , 5 },
            { "7\u20e3" , 6 }, { ":seven:" , 6 },
            { "8\u20e3" , 7 }, { ":eight:" , 7 },
            { "9\u20e3" , 8 }, { ":nine:" , 8 },
            { "\U0001f51f" , 9 }, { ":keycap_ten:" , 9 },
        }.ToImmutableDictionary();


        public static DiscordEmbed ToEmbed(this ReactionsPoll poll, LocalizationService lcs)
        {
            var emb = new LocalizedEmbedBuilder(lcs, poll.Channel.GuildId);
            emb.WithTitle(poll.Question);
            emb.WithLocalizedDescription("str-vote-react");
            emb.WithColor(DiscordColor.Orange);

            for (int i = 0; i < poll.Options.Count; i++) {
                if (!string.IsNullOrWhiteSpace(poll.Options[i]))
                    emb.AddField($"{i + 1}", poll.Options[i], inline: true);
            }

            if (poll.EndTime is { }) {
                string localizedTime = lcs.GetLocalizedTimeString(poll.Channel.GuildId, poll.EndTime);
                if (poll.TimeUntilEnd.TotalSeconds > 1)
                    emb.WithLocalizedFooter("fmt-poll-end", poll.Initiator.AvatarUrl, localizedTime, $"{localizedTime:hh\\:mm\\:ss}");
                else
                    emb.WithLocalizedFooter("fmt-poll-end", poll.Initiator.AvatarUrl);
            }

            return emb.Build();
        }

        public static DiscordEmbed ResultsToDiscordEmbed(this ReactionsPoll poll, LocalizationService lcs)
        {
            var emb = new LocalizedEmbedBuilder(lcs, poll.Channel.GuildId);
            emb.WithLocalizedTitle("fmt-poll-res", poll.Question);
            emb.WithColor(DiscordColor.Orange);

            emb.WithLocalizedFooter("fmt-poll-by", poll.Initiator.AvatarUrl, poll.Initiator.DisplayName);

            if (poll.Results is null || !poll.Results.Any())
                return emb.WithLocalizedDescription("str-poll-none").Build();

            foreach (PollEmoji pe in poll.Results)
                emb.AddField(poll.Options[_emojiid[pe.Emoji.Name]], pe.Voted.Count.ToString(), inline: true);

            return emb.Build();
        }
    }
}
