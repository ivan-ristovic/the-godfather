#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class RussianRoulette : ChannelEvent
    {
        private ConcurrentHashSet<DiscordUser> _participants = new ConcurrentHashSet<DiscordUser>();
        public int ParticipantCount => _participants.Count();
        public bool Started { get; private set; }
        public List<DiscordUser> Survivors = null;


        public RussianRoulette(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            Started = false;
        }


        public override async Task RunAsync()
        {
            Started = true;

            for (int chance = 1; chance < 5 && ParticipantCount > 1; chance++) {
                var msg = await _channel.SendMessageAsync($"Round #{chance} starts in 5s!")
                    .ConfigureAwait(false);

                await Task.Delay(TimeSpan.FromSeconds(5))
                    .ConfigureAwait(false);

                var participants = _participants.ToList();
                var events = new StringBuilder();
                foreach (var participant in participants) {
                    if (GFRandom.Generator.Next(6) < chance) {
                        events.AppendLine($"{participant.Mention} {StaticDiscordEmoji.Dead} {StaticDiscordEmoji.Blast} {StaticDiscordEmoji.Gun}");
                        _participants.TryRemove(participant);
                    } else {
                        events.AppendLine($"{participant.Mention} {StaticDiscordEmoji.Relieved} {StaticDiscordEmoji.Gun}");
                    }
                    
                    msg = await msg.ModifyAsync(embed: new DiscordEmbedBuilder() {
                        Title = $"ROUND #{chance}",
                        Description = events.ToString(),
                        Color = DiscordColor.DarkRed
                    }.Build()).ConfigureAwait(false);

                    await Task.Delay(TimeSpan.FromSeconds(2))
                        .ConfigureAwait(false);
                }
            }

            Survivors = _participants.ToList();
        }

        public bool AddParticipant(DiscordUser user)
        {
            if (_participants.Any(u => user.Id == u.Id))
                return false;
            return _participants.Add(user);
        }
    }
}
