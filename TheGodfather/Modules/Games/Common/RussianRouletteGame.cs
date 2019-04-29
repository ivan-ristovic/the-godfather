#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Collections;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class RussianRouletteGame : ChannelEvent
    {
        public int ParticipantCount => this.participants.Count;
        public bool Started { get; private set; }
        public IReadOnlyList<DiscordUser> Survivors { get; private set; }

        private readonly ConcurrentHashSet<DiscordUser> participants;


        public RussianRouletteGame(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            this.Started = false;
            this.participants = new ConcurrentHashSet<DiscordUser>();
        }


        public override async Task RunAsync()
        {
            this.Started = true;

            for (int round = 1; round < 5 && this.ParticipantCount > 1; round++) {
                DiscordMessage msg = await this.Channel.SendMessageAsync($"Round #{round} starts in 5s!");

                await Task.Delay(TimeSpan.FromSeconds(5));

                var participants = this.participants.ToList();
                var eb = new StringBuilder();
                foreach (DiscordUser participant in participants) {
                    if (GFRandom.Generator.Next(6) < round) {
                        eb.AppendLine($"{participant.Mention} {StaticDiscordEmoji.Dead} {StaticDiscordEmoji.Blast} {StaticDiscordEmoji.Gun}");
                        this.participants.TryRemove(participant);
                    } else {
                        eb.AppendLine($"{participant.Mention} {StaticDiscordEmoji.Relieved} {StaticDiscordEmoji.Gun}");
                    }
                    
                    msg = await msg.ModifyAsync(embed: new DiscordEmbedBuilder {
                        Title = $"ROUND #{round}",
                        Description = eb.ToString(),
                        Color = DiscordColor.DarkRed
                    }.Build());

                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            this.Survivors = this.participants.ToList().AsReadOnly();
        }

        public bool AddParticipant(DiscordUser user)
        {
            if (this.participants.Any(u => user.Id == u.Id))
                return false;
            return this.participants.Add(user);
        }
    }
}
