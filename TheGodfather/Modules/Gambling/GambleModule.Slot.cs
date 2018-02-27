#region USING_DIRECTIVES
using System;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Gambling
{
    public partial class GambleModule : TheGodfatherBaseModule
    {
        #region COMMAND_SLOT
        [Command("slot")]
        [Description("Roll a slot machine.")]
        [Aliases("slotmachine")]
        [UsageExample("!gamble slot 20")]
        public async Task SlotMachine(CommandContext ctx,
                                     [Description("Bid.")] int bid = 5)
        {
            if (bid <= 0)
                throw new InvalidCommandUsageException("Invalid bid amount!");

            if (!await Database.RetrieveCreditsAsync(ctx.User.Id, bid).ConfigureAwait(false))
                throw new CommandFailedException("You do not have enough credits in WM bank!");

            DiscordEmoji[,] res = RollSlot(ctx);
            int won = EvaluateSlotResult(res, bid);

            var em = new DiscordEmbedBuilder() {
                Title = "TOTALLY NOT RIGGED SERBIAN SLOT MACHINE",
                Description = MakeStringFromResult(res),
                Color = DiscordColor.Yellow
            };
            em.AddField("Result", $"You won {Formatter.Bold(won.ToString())} credits!");

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);

            if (won > 0)
                await Database.IncreaseBalanceForUserAsync(ctx.User.Id, won)
                    .ConfigureAwait(false);
        }
        #endregion

        #region HELPER_FUNCTIONS
        private DiscordEmoji[,] RollSlot(CommandContext ctx)
        {
            DiscordEmoji[] emoji = {
                DiscordEmoji.FromName(ctx.Client, ":peach:"),
                DiscordEmoji.FromName(ctx.Client, ":moneybag:"),
                DiscordEmoji.FromName(ctx.Client, ":gift:"),
                DiscordEmoji.FromName(ctx.Client, ":large_blue_diamond:"),
                DiscordEmoji.FromName(ctx.Client, ":seven:"),
                DiscordEmoji.FromName(ctx.Client, ":cherries:")
            };

            var rnd = new Random();
            DiscordEmoji[,] result = new DiscordEmoji[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    result[i, j] = emoji[rnd.Next(emoji.Length)];

            return result;
        }

        private string MakeStringFromResult(DiscordEmoji[,] res)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++)
                    sb.Append(res[i, j]);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private int EvaluateSlotResult(DiscordEmoji[,] res, int bid)
        {
            int pts = bid;
            
            for (int i = 0; i < 3; i++) {
                if (res[i, 0] == res[i, 1] && res[i, 1] == res[i, 2]) {
                    if (res[i, 0].GetDiscordName() == ":large_blue_diamond:")
                        pts *= 50;
                    else if (res[i, 0].GetDiscordName() == ":moneybag:")
                        pts *= 25;
                    else if (res[i, 0].GetDiscordName() == ":seven:")
                        pts *= 10;
                    else
                        pts *= 5;
                }
            }
            
            for (int i = 0; i < 3; i++) {
                if (res[0, i] == res[1, i] && res[1, i] == res[2, i]) {
                    if (res[0, i].GetDiscordName() == ":large_blue_diamond:")
                        pts *= 50;
                    else if (res[0, i].GetDiscordName() == ":moneybag:")
                        pts *= 25;
                    else if (res[0, i].GetDiscordName() == ":seven:")
                        pts *= 10;
                    else
                        pts *= 5;
                }
            }

            return pts == bid ? 0 : pts;
        }
        #endregion
    }
}
