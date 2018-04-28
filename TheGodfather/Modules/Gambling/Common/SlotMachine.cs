#region USING_DIRECTIVES
using System.Text;
using System.Collections.Generic;

using TheGodfather.Common;

using DSharpPlus;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Gambling.Common
{
    public class SlotMachine
    {
        private static Dictionary<int, DiscordEmoji> IdToEmoji = new Dictionary<int, DiscordEmoji>() {
            {0, StaticDiscordEmoji.LargeBlueDiamond },
            {1, StaticDiscordEmoji.MoneyBag },
            {2, StaticDiscordEmoji.Seven },
            {3, StaticDiscordEmoji.Gift },
            {4, StaticDiscordEmoji.Cherries },
        };


        public static DiscordEmbed EmbedSlotRoll(DiscordUser user, int bid, out int won)
        {
            var res = RollSlot();
            won = EvaluateSlotResult(res, bid);

            var emb = new DiscordEmbedBuilder() {
                Title = "TOTALLY NOT RIGGED SERBIAN SLOT MACHINE",
                Description = MakeStringFromResult(res),
                Color = DiscordColor.Yellow
            };

            emb.AddField("Result", $"{user.Mention} won {Formatter.Bold(won.ToString())} credits!");

            return emb.Build();
        }


        private static int[,] RollSlot()
        {
            int[,] result = new int[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    result[i, j] = GFRandom.Generator.Next(IdToEmoji.Count);

            return result;
        }

        private static string MakeStringFromResult(int[,] res)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++)
                    sb.Append(IdToEmoji[res[i, j]]);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static int EvaluateSlotResult(int[,] res, int bid)
        {
            int pts = bid;

            for (int i = 0; i < 3; i++) {
                if (res[i, 0] == res[i, 1] && res[i, 1] == res[i, 2]) {
                    if (res[i, 0] == 0)
                        pts *= 7;
                    else if (res[i, 0] == 1)
                        pts *= 5;
                    else if (res[i, 0] == 2)
                        pts *= 4;
                    else
                        pts *= 3;
                }
            }

            for (int i = 0; i < 3; i++) {
                if (res[0, i] == res[1, i] && res[1, i] == res[2, i]) {
                    if (res[0, i] == 0)
                        pts *= 7;
                    else if (res[0, i] == 1)
                        pts *= 5;
                    else if (res[0, i] == 2)
                        pts *= 4;
                    else
                        pts *= 3;
                }
            }

            return pts == bid ? 0 : pts;
        }
    }
}
