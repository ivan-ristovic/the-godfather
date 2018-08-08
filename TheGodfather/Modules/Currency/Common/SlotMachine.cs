#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;

using Humanizer;

using System.Collections.Immutable;
using System.Text;

using TheGodfather.Common;
#endregion

namespace TheGodfather.Modules.Currency.Common
{
    public class SlotMachine
    {
        private static ImmutableArray<DiscordEmoji> _emoji = new DiscordEmoji[] {
            StaticDiscordEmoji.LargeBlueDiamond,
            StaticDiscordEmoji.Seven,
            StaticDiscordEmoji.MoneyBag,
            StaticDiscordEmoji.Trophy,
            StaticDiscordEmoji.Gift,
            StaticDiscordEmoji.Cherries
        }.ToImmutableArray();

        private static ImmutableArray<int> _multipliers = new int[] {
            10,
            7,
            5,
            4,
            3,
            2
        }.ToImmutableArray();


        public static DiscordEmbed RollToDiscordEmbed(DiscordUser user, long bid, out long won)
        {
            int[,] res = Roll();
            won = EvaluateSlotResult(res, bid);

            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.LargeOrangeDiamond} SLUT MACHINE {StaticDiscordEmoji.LargeOrangeDiamond}",
                Description = MakeStringFromResult(res),
                Color = DiscordColor.DarkGreen,
                ThumbnailUrl = user.AvatarUrl
            };

            var sb = new StringBuilder();
            for (int i = 0; i < _emoji.Length; i++)
                sb.Append(_emoji[i]).Append(Formatter.InlineCode($" x{_multipliers[i]}")).Append(" ");

            emb.AddField("Multipliers", sb.ToString());
            emb.AddField("Result", $"{user.Mention} won {Formatter.Bold(won.ToWords())} ({won:n0}) credits!");

            return emb.Build();
        }


        private static int[,] Roll()
        {
            int[,] result = new int[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    result[i, j] = GFRandom.Generator.Next(_emoji.Length);

            return result;
        }

        private static string MakeStringFromResult(int[,] res)
        {
            var sb = new StringBuilder();

            sb.Append(StaticDiscordEmoji.BlackSquare);
            for (int i = 0; i < 5; i++)
                sb.Append(StaticDiscordEmoji.SmallOrangeDiamond);
            sb.AppendLine();

            for (int i = 0; i < 3; i++) {
                if (i % 2 == 1)
                    sb.Append(StaticDiscordEmoji.Joystick);
                else
                    sb.Append(StaticDiscordEmoji.BlackSquare);
                sb.Append(StaticDiscordEmoji.SmallOrangeDiamond);
                for (int j = 0; j < 3; j++)
                    sb.Append(_emoji[res[i, j]]);
                sb.AppendLine(StaticDiscordEmoji.SmallOrangeDiamond);
            }

            sb.Append(StaticDiscordEmoji.BlackSquare);
            for (int i = 0; i < 5; i++)
                sb.Append(StaticDiscordEmoji.SmallOrangeDiamond);

            return sb.ToString();
        }

        private static long EvaluateSlotResult(int[,] res, long bid)
        {
            long pts = bid;

            for (int i = 0; i < 3; i++)
                if (res[i, 0] == res[i, 1] && res[i, 1] == res[i, 2])
                    pts *= _multipliers[res[i, 0]];

            for (int i = 0; i < 3; i++)
                if (res[0, i] == res[1, i] && res[1, i] == res[2, i])
                    pts *= _multipliers[res[0, i]];

            return pts == bid ? 0L : pts;
        }
    }
}
