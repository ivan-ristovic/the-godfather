using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Games.Common
{
    public sealed class RussianRouletteGame : BaseChannelGame
    {
        public const int MaxParticipants = 10;

        public int ParticipantCount => this.participants.Count;
        public bool Started { get; private set; }
        public IReadOnlyList<DiscordUser> Survivors { get; private set; }

        private readonly ConcurrentHashSet<DiscordUser> participants;


        public RussianRouletteGame(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            this.Started = false;
            this.Survivors = new List<DiscordUser>();
            this.participants = new ConcurrentHashSet<DiscordUser>();
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            this.Started = true;

            var rng = new SecureRandom();
            for (int round = 1; round <= 5 && this.ParticipantCount > 1; round++) {
                DiscordMessage msg = await this.Channel.LocalizedEmbedAsync(lcs, Emojis.Gun, DiscordColor.DarkRed, "fmt-game-rr-starting", round);
                await Task.Delay(TimeSpan.FromSeconds(5));

                var eb = new StringBuilder();
                foreach (DiscordUser participant in this.participants) {
                    if (rng.NextBool(round)) {
                        eb.AppendLine($"{participant.Mention} {Emojis.Dead} {Emojis.Blast} {Emojis.Gun}");
                        this.participants.TryRemove(participant);
                    } else {
                        eb.AppendLine($"{participant.Mention} {Emojis.Relieved} {Emojis.Gun}");
                    }

                    var emb = new LocalizedEmbedBuilder(lcs, this.Channel.GuildId);
                    emb.WithLocalizedTitle("fmt-game-rr-round", round);
                    emb.WithDescription(eb.ToString());
                    emb.WithColor(DiscordColor.DarkRed);
                    msg = await msg.ModifyOrResendAsync(this.Channel, emb.Build());

                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            this.Survivors = this.participants.ToList().AsReadOnly();
        }

        public bool AddParticipant(DiscordUser user) 
            => this.participants.Add(user);
    }
}
