using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace TheGodfather.Modules.Games;

public partial class GamesModule
{
    [Group("rps")]
    [Aliases("rockpaperscissors")]
    public sealed class RockPaperScissorsModule : TheGodfatherModule
    {
        #region game rps
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            DiscordEmoji[] rpsEmojis = { Emojis.Rock, Emojis.Paper, Emojis.Scissors };
            DiscordEmoji? userPick = null;
            try {
                foreach (DiscordEmoji emoji in rpsEmojis)
                    await ctx.Message.CreateReactionAsync(emoji);

                InteractivityResult<MessageReactionAddEventArgs> res = await ctx.Client.GetInteractivity().WaitForReactionAsync(
                    e => rpsEmojis.Contains(e.Emoji),
                    ctx.User
                );
                if (!res.TimedOut)
                    userPick = res.Result.Emoji;
            } catch {
                // ignored
            }

            if (userPick is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_timed_out);

            DiscordEmoji gfPick = new SecureRandom().ChooseRandomElement(rpsEmojis);
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Joystick, TranslationKey.fmt_rps(ctx.User.Mention, userPick, gfPick, ctx.Client.CurrentUser.Mention));
        }
        #endregion

        #region game rps rules
        [Command("rules")]
        [Aliases("help", "h", "ruling", "rule")]
        public Task RulesAsync(CommandContext ctx)
            => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, TranslationKey.str_game_rps);
        #endregion
    }
}