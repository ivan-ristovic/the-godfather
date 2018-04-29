#region USING_DIRECTIVES
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Modules.Games.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Gambling.Common
{
    public class WheelOfFortune : Game
    {
        private static readonly ImmutableArray<string> _emojis = new string[] {
            "⬆", "↗", "➡", "↘", "⬇", "↙", "⬅", "↖",
        }.ToImmutableArray();

        private static readonly ImmutableArray<float> Multipliers = new float[] {
            2.4f, 0.3f, 1.7f, 0.5f, 1.2f, 0.1f, 0.2f, 1.5f
        }.ToImmutableArray();

        public int Duration { get; }
        public float Multiplier => Multipliers[Duration % Multipliers.Length];
        public int WonAmount { get; private set; }
        public bool Rotating = false;

        private DiscordUser _user;
        private int _bid;


        public WheelOfFortune(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser user, int bid)
            : base(interactivity, channel)
        {
            _user = user;
            _bid = bid;
            Duration = GFRandom.Generator.Next(3, 12);
            WonAmount = (int)(_bid * Multiplier);
        }


        public override async Task RunAsync()
        {
            Rotating = true;

            var msg = await _channel.SendMessageAsync($"Rolling Wheel of Fortune for {_user.Mention}!")
                .ConfigureAwait(false);

            for (int i = 0; i < Duration; i++) {
                await PrintGameAsync(msg, i)
                    .ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(1))
                    .ConfigureAwait(false);
            }

            Rotating = false;

            await Task.Delay(TimeSpan.FromSeconds(1))
                .ConfigureAwait(false);
            await PrintGameAsync(msg, Duration)
                .ConfigureAwait(false);
        }


        private async Task PrintGameAsync(DiscordMessage msg, int rotation)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.MoneyBag} WHEEL OF FORTUNE {StaticDiscordEmoji.MoneyBag}",
                Color = DiscordColor.Yellow
            };

            string description = $@"
‣‣‣‣‣‣‣‣‣‣『{Multipliers[0]}』‣‣‣‣‣‣‣‣‣‣
‣‣‣『{Multipliers[7]}』         『{Multipliers[1]}』‣‣‣
『{Multipliers[6]}』    {_emojis[rotation % Multipliers.Length]}       『{Multipliers[2]}』
‣‣‣『{Multipliers[5]}』         『{Multipliers[3]}』‣‣‣
‣‣‣‣‣‣‣‣‣‣『{Multipliers[4]}』‣‣‣‣‣‣‣‣‣‣";
            
            emb.WithDescription(description);

            if (!Rotating)
                emb.AddField("Result", $"{_user.Mention} won {Formatter.Bold(WonAmount.ToString())} credits!");

            await msg.ModifyAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
    }
}
