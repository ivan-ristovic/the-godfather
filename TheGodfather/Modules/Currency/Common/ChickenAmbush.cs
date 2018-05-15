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
    public class ChickenAmbush : ChannelEvent
    {
        public bool Started { get; private set; }
        public readonly Chicken Ambushed;
        public bool AmbushedChickenSurvived { get; private set; }
        public ConcurrentQueue<Chicken> Ambushers { get; } = new ConcurrentQueue<Chicken>();
        public int AmbusherCount => Ambushers.Count;


        public ChickenAmbush(InteractivityExtension interactivity, DiscordChannel channel, Chicken ambushed)
            : base(interactivity, channel)
        {
            Ambushed = ambushed;
            Started = false;
        }


        public override async Task RunAsync()
        {
            Started = true;

            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.Chicken} CHICKEN AMBUSH STARTING {StaticDiscordEmoji.Chicken}",
                Description = $"{Formatter.Bold(Ambushed.Name)} fell into an ambush! The outcome will be known after the dust settles..."
            };

            foreach (var chicken in Ambushers)
                emb.AddField("Ambusher", chicken.Name);

            await _channel.SendMessageAsync(embed: emb.Build())
                .ConfigureAwait(false);

            await Task.Delay(TimeSpan.FromSeconds(10))
                .ConfigureAwait(false);

            Chicken combined = new Chicken() {
                Strength = (short)(Ambushers.Sum(c => c.Strength) - Ambushers.Count * 15)
            };
            AmbushedChickenSurvived = (Ambushed.Fight(combined).OwnerId == Ambushed.OwnerId);
        }

        public void AddParticipant(Chicken chicken, DiscordUser owner)
        {
            if (IsParticipating(owner))
                return;

            chicken.Owner = owner;
            Ambushers.Enqueue(chicken);
        }

        public bool IsParticipating(DiscordUser owner) => Ambushers.Any(c => c.OwnerId == owner.Id);
    }
}
