#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Currency.Common
{
    public class ChickenWar : ChannelEvent
    {
        public bool Started { get; private set; }
        public bool Team1Won { get; private set; } = false;
        public bool Team2Won { get; private set; } = false;
        public string Team1Name { get; } = "Team1";
        public string Team2Name { get; } = "Team2";
        public ConcurrentQueue<Chicken> Team1 { get; } = new ConcurrentQueue<Chicken>();
        public ConcurrentQueue<Chicken> Team2 { get; } = new ConcurrentQueue<Chicken>();


        public ChickenWar(InteractivityExtension interactivity, DiscordChannel channel, string team1, string team2)
            : base(interactivity, channel)
        {
            Started = false;
            if (!string.IsNullOrWhiteSpace(team1))
                Team1Name = team1;
            if (!string.IsNullOrWhiteSpace(team2))
                Team2Name = team2;
        }


        public override async Task RunAsync()
        {
            Started = true;

            short str1 = (short)(Team1.Sum(c => c.Stats.Strength));
            short str2 = (short)(Team2.Sum(c => c.Stats.Strength));

            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.Chicken} CHICKEN WAR STARTING {StaticDiscordEmoji.Chicken}",
                Description = $"{Formatter.Bold(Team1Name)} ({str1} STR) vs {Formatter.Bold(Team2Name)} ({str2} STR)",
                Color = DiscordColor.Aquamarine
            };
            emb.AddField(Team1Name, string.Join(", ", Team1.Select(c => c.Name)));
            emb.AddField(Team2Name, string.Join(", ", Team2.Select(c => c.Name)));

            await _channel.SendMessageAsync(embed: emb.Build())
                .ConfigureAwait(false);

            await Task.Delay(TimeSpan.FromSeconds(10))
                .ConfigureAwait(false);

            var c1 = new Chicken() { Stats = new ChickenStats { Strength = str1 } };
            var c2 = new Chicken() { Stats = new ChickenStats { Strength = str2 } };
            if (c1.Fight(c2) == c1)
                Team1Won = true;
            else
                Team2Won = true;
        }

        public bool AddParticipant(Chicken chicken, DiscordUser owner, bool team1 = false, bool team2 = false)
        {
            if (IsParticipating(owner))
                return false;

            chicken.Owner = owner;
            if (team1)
                Team1.Enqueue(chicken);
            else if (team2)
                Team2.Enqueue(chicken);
            else
                return false;

            return true;
        }

        public bool IsParticipating(DiscordUser user) 
            => Team1.Any(c => c.OwnerId == user.Id) || Team2.Any(c => c.OwnerId == user.Id);
    }
}
