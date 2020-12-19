using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Chickens.Common
{
    public class ChickenWar : IChannelEvent
    {
        public DiscordChannel Channel { get; protected set; }
        public InteractivityExtension Interactivity { get; protected set; }
        public int Gain { get; private set; }
        public bool Started { get; private set; }
        public ConcurrentQueue<Chicken> Team1 { get; }
        public ConcurrentQueue<Chicken> Team2 { get; }
        public ConcurrentQueue<Chicken> WinningTeam => this.Team1Won ? this.Team1 : this.Team2;
        public ConcurrentQueue<Chicken> LosingTeam => this.Team1Won ? this.Team2 : this.Team1;
        public string? Team1Name { get; }
        public string? Team2Name { get; }
        public bool Team1Won { get; private set; }
        public bool Team2Won { get; private set; }
        public ChickenFightResult? Result { get; private set; }


        public ChickenWar(InteractivityExtension interactivity, DiscordChannel channel, string team1, string team2)
        {
            this.Interactivity = interactivity;
            this.Channel = channel;

            this.Started = false;

            if (!string.IsNullOrWhiteSpace(team1))
                this.Team1Name = team1;
            if (!string.IsNullOrWhiteSpace(team2))
                this.Team2Name = team2;

            this.Team1 = new ConcurrentQueue<Chicken>();
            this.Team2 = new ConcurrentQueue<Chicken>();
        }


        public async Task RunAsync(LocalizationService _)
        {
            this.Started = true;

            int str1 = this.Team1.Sum(c => c.Stats.TotalStrength);
            int str2 = this.Team2.Sum(c => c.Stats.TotalStrength);

            var emb = new DiscordEmbedBuilder {
                Title = $"{Emojis.Chicken} {Emojis.DuelSwords} {Emojis.Chicken}",
                Description = $"{Formatter.Bold(this.Team1Name)} ({str1} STR) VS {Formatter.Bold(this.Team2Name)} ({str2} STR)",
                Color = DiscordColor.Aquamarine
            };
            emb.AddField(this.Team1Name, this.Team1.Select(c => c.Name).JoinWith(", "));
            emb.AddField(this.Team2Name, this.Team2.Select(c => c.Name).JoinWith(", "));

            await this.Channel.SendMessageAsync(embed: emb.Build());
            await Task.Delay(TimeSpan.FromSeconds(10));

            var c1 = new Chicken { Stats = new ChickenStats { BareStrength = str1 } };
            var c2 = new Chicken { Stats = new ChickenStats { BareStrength = str2 } };
            this.Result = ChickenFightResult.Fight(c1, c2);
            this.Team1Won = this.Result.Winner == c1;
            this.Team2Won = !this.Team1Won;
            this.Gain = this.Result.StrGain;
        }

        public bool AddParticipant(Chicken chicken, DiscordUser owner, bool team1 = false, bool team2 = false)
        {
            if (this.IsParticipating(owner))
                return false;

            chicken.Owner = owner;
            if (team1)
                this.Team1.Enqueue(chicken);
            else if (team2)
                this.Team2.Enqueue(chicken);
            else
                return false;

            return true;
        }

        public bool IsParticipating(DiscordUser user)
            => this.Team1.Any(c => c.UserId == user.Id) || this.Team2.Any(c => c.UserId == user.Id);
    }
}
