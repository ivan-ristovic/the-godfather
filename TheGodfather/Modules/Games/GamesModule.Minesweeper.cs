using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Modules.Games.Extensions;

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("minesweeper")]
        [Aliases("mines", "ms")]
        public sealed class MinesweeperModule : TheGodfatherModule
        {
            #region game minesweeper
            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description(TranslationKey.desc_game_ms_rows)] int rows = 9,
                                         [Description(TranslationKey.desc_game_ms_cols)] int cols = 9,
                                         [Description(TranslationKey.desc_game_ms_bombs)] int bombs = 10)
            {
                if (rows < 4 || rows > 9 || cols < 4 || cols > 12)
                    throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_game_ms_dim(4, 9, 4, 12));

                try {
                    var field = new MinesweeperField(rows, cols, bombs);
                    return ctx.RespondAsync(field.ToEmojiString());
                } catch (ArgumentException) {
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_ms_bombs);
                }
            }
            #endregion

            #region game minesweeper rules
            [Command("rules")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
                => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, TranslationKey.str_game_ms);
            #endregion
        }
    }
}
