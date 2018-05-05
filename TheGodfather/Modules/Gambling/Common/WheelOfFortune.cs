#region USING_DIRECTIVES
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
        
        public int WonAmount => (int)(_bid * Multipliers[_index]);

        private DiscordUser _user;
        private int _bid = 0;
        private int _index = 0;


        public WheelOfFortune(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser user, int bid)
            : base(interactivity, channel)
        {
            _user = user;
            _bid = bid;
            _index = GFRandom.Generator.Next(Multipliers.Length);
        }


        public override async Task RunAsync()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.MoneyBag} WM WHEEL OF FORTUNE! {StaticDiscordEmoji.MoneyBag}",
                Color = DiscordColor.Yellow
            };

            string description = $@"
‣‣‣‣‣‣‣‣‣‣『{Multipliers[0]}』‣‣‣‣‣‣‣‣‣‣
‣‣‣『{Multipliers[7]}』         『{Multipliers[1]}』‣‣‣
『{Multipliers[6]}』    {_emojis[_index]}       『{Multipliers[2]}』
‣‣‣『{Multipliers[5]}』         『{Multipliers[3]}』‣‣‣
‣‣‣‣‣‣‣‣‣‣『{Multipliers[4]}』‣‣‣‣‣‣‣‣‣‣";

            emb.WithDescription(description);

            emb.AddField("Result", $"{_user.Mention} won {Formatter.Bold(WonAmount.ToString())} credits!");

            await _channel.SendMessageAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
    }
}
