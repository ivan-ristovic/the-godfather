using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Common;
using TheGodfather.Exceptions;

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        #region COMMAND_GAME_RPS
        [Command("rps")]
        [Description("Rock, paper, scissors game against TheGodfather")]
        [Aliases("rockpaperscissors")]
        public async Task RpsAsync(CommandContext ctx,
                                  [Description("rock/paper/scissors")] string rps)
        {
            if (string.IsNullOrWhiteSpace(rps))
                throw new CommandFailedException("Missing your pick!");

            DiscordEmoji userPick;
            if (string.Compare(rps, "rock", true) == 0 || string.Compare(rps, "r", true) == 0)
                userPick = DiscordEmoji.FromName(ctx.Client, ":new_moon:");
            else if (string.Compare(rps, "paper", true) == 0 || string.Compare(rps, "p", true) == 0)
                userPick = DiscordEmoji.FromName(ctx.Client, ":newspaper:");
            else if (string.Compare(rps, "scissors", true) == 0 || string.Compare(rps, "s", true) == 0)
                userPick = DiscordEmoji.FromName(ctx.Client, ":scissors:");
            else
                throw new CommandFailedException("Invalid pick. Must be rock, paper or scissors.");

            DiscordEmoji gfPick = (new SecureRandom().Next(3)) switch {
                0 => DiscordEmoji.FromName(ctx.Client, ":new_moon:"),
                1 => DiscordEmoji.FromName(ctx.Client, ":newspaper:"),
                _ => DiscordEmoji.FromName(ctx.Client, ":scissors:"),
            };
            await this.InformAsync(ctx, Emojis.Joystick, $"{ctx.User.Mention} {userPick} {gfPick} {ctx.Client.CurrentUser.Mention}");
        }
        #endregion

    }
}
