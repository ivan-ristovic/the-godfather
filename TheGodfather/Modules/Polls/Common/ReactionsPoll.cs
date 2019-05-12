#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Polls.Common
{
    public class ReactionsPoll : Poll
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


        private DiscordMessage msgHandle;
        private IReadOnlyCollection<PollEmoji> results;


        public ReactionsPoll(InteractivityExtension interactivity, DiscordChannel channel, DiscordMember sender, string question)
            : base(interactivity, channel, sender, question)
        {

        }


        public override async Task RunAsync(TimeSpan timespan)
        {
            this.endTime = DateTime.Now + timespan;
            this.IsRunning = true;

            this.msgHandle = await this.channel.SendMessageAsync(embed: this.ToDiscordEmbed());

            this.results = await this.interactivity.DoPollAsync(this.msgHandle, StaticDiscordEmoji.Numbers.Take(this.Options.Count).ToArray(), PollBehaviour.Default, timespan);

            await this.channel.SendMessageAsync(embed: this.ResultsToDiscordEmbed());

            this.IsRunning = false;
        }

        public override DiscordEmbed ToDiscordEmbed()
        {
            var emb = new DiscordEmbedBuilder {
                Title = Question,
                Description = "Vote by clicking on the reactions!",
                Color = DiscordColor.Orange
            };

            for (int i = 0; i < this.Options.Count; i++)
                if (!string.IsNullOrWhiteSpace(this.Options[i]))
                    emb.AddField($"{i + 1}", this.Options[i], inline: true);

            if (this.endTime != null) {
                if (this.TimeUntilEnd.TotalSeconds > 1)
                    emb.WithFooter($"Poll ends {this.endTime.ToUtcTimestamp()} (in {this.TimeUntilEnd:hh\\:mm\\:ss})", this.Initiator.AvatarUrl);
                else
                    emb.WithFooter($"Poll ended.", this.Initiator.AvatarUrl);
            }

            return emb.Build();
        }

        public override DiscordEmbed ResultsToDiscordEmbed()
        {
            var emb = new DiscordEmbedBuilder {
                Title = this.Question + " (results)",
                Color = DiscordColor.Orange
            };

            emb.WithFooter($"Poll by {this.Initiator.DisplayName}", this.Initiator.AvatarUrl);

            if (!this.results.Any())
                return emb.WithDescription("Nobody voted!").Build();

            foreach (PollEmoji pe in this.results) 
                emb.AddField(this.Options[_emojiid[pe.Emoji.Name]], pe.Voted.Count.ToString(), inline: true);

            return emb.Build();
        }
    }
}
