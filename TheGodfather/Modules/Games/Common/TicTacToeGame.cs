using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Games.Common
{
    public sealed class TicTacToeGame : BaseBoardGame
    {
        public TicTacToeGame(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser p1, DiscordUser p2, TimeSpan? movetime = null)
            : base(interactivity, channel, p1, p2, sizeX: 3, sizeY: 3, movetime) { }


        protected override async Task AdvanceAsync(LocalizationService lcs)
        {
            int field = 0;
            bool player1plays = (this.move % 2 == 0);
            InteractivityResult<DiscordMessage> mctx = await this.Interactivity.WaitForMessageAsync(
                xm => {
                    if (xm.Channel.Id != this.Channel.Id) return false;
                    if (player1plays && (xm.Author.Id != this.player1.Id)) return false;
                    if (!player1plays && (xm.Author.Id != this.player2.Id)) return false;
                    if (!int.TryParse(xm.Content, out field)) return false;
                    return field is > 0 and < 10;
                },
                this.moveTime
            );
            if (mctx.TimedOut) {
                this.IsTimeoutReached = true;
                this.Winner = player1plays ? this.player2 : this.player1;
                return;
            }

            if (this.TryPlayMove(player1plays ? 1 : 2, (field - 1) / this.SizeX, (field - 1) % this.SizeY)) {
                this.move++;
                if (!this.deleteErrored) {
                    try {
                        await mctx.Result.DeleteAsync();
                    } catch {
                        await this.Channel.InformFailureAsync(lcs.GetString(this.Channel.GuildId, TranslationKey.cmd_err_game_perms));
                        this.deleteErrored = true;
                    }
                }
            } else {
                await this.Channel.InformFailureAsync(lcs.GetString(this.Channel.GuildId, TranslationKey.cmd_err_game_move(field)));
            }
        }

        protected override bool IsGameOver()
        {
            for (int i = 0; i < this.SizeX; i++) {
                if (this.board[i, 0] != 0 && this.board[i, 0] == this.board[i, 1] && this.board[i, 1] == this.board[i, 2])
                    return true;
                if (this.board[0, i] != 0 && this.board[0, i] == this.board[1, i] && this.board[1, i] == this.board[2, i])
                    return true;
            }

            if (this.board[0, 0] != 0 && this.board[0, 0] == this.board[1, 1] && this.board[1, 1] == this.board[2, 2])
                return true;
            if (this.board[0, 2] != 0 && this.board[0, 2] == this.board[1, 1] && this.board[1, 1] == this.board[2, 0])
                return true;

            return false;
        }

        protected override Task<DiscordMessage> UpdateBoardAsync(LocalizationService lcs)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < this.SizeX; i++) {
                for (int j = 0; j < this.SizeY; j++) {
                    switch (this.board[i, j]) {
                        case 0: sb.Append(Emojis.BoardSquare); break;
                        case 1: sb.Append(Emojis.X); break;
                        case 2: sb.Append(Emojis.O); break;
                    }
                }
                sb.AppendLine();
            }

            sb.AppendLine()
              .Append(lcs.GetString(this.Channel.GuildId, TranslationKey.str_game_move))
              .AppendLine(this.move % 2 == 0 ? this.player1.Mention : this.player2.Mention);

            return this.msgHandle.ModifyOrResendAsync(this.Channel, new DiscordEmbedBuilder {
                Description = sb.ToString()
            }.Build());
        }
    }
}
