using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Games.Common
{
    public sealed class Connect4Game : BaseBoardGame
    {
        private static readonly string _header = Emojis.Numbers.All.Skip(1).Take(9).JoinWith("");


        public Connect4Game(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser player1, DiscordUser player2, TimeSpan? movetime = null)
            : base(interactivity, channel, player1, player2, sizeX: 9, sizeY: 7, movetime) { }


        protected override async Task AdvanceAsync(LocalizationService lcs)
        {
            int col = 0;
            bool player1plays = (this.move % 2 == 0);
            InteractivityResult<DiscordMessage> mctx = await this.Interactivity.WaitForMessageAsync(
                xm => {
                    if (xm.Channel.Id != this.Channel.Id || xm.Author.IsBot) return false;
                    if (player1plays && (xm.Author.Id != this.player1.Id)) return false;
                    if (!player1plays && (xm.Author.Id != this.player2.Id)) return false;
                    if (!int.TryParse(xm.Content, out col)) return false;
                    return col > 0 && col <= this.SizeY;
                },
                this.moveTime
            );
            if (mctx.TimedOut) {
                this.IsTimeoutReached = true;
                this.Winner = player1plays ? this.player2 : this.player1;
                return;
            }

            if (this.TryPlayMove(player1plays ? 1 : 2, 1, col - 1)) {
                this.move++;
                if (!this.deleteErrored) {
                    try {
                        await mctx.Result.DeleteAsync();
                    } catch (UnauthorizedException) {
                        await this.Channel.InformFailureAsync(lcs.GetString(this.Channel.GuildId, "cmd-err-game-perms"));
                        this.deleteErrored = true;
                    }
                }
            } else {
                await this.Channel.InformFailureAsync(lcs.GetString(this.Channel.GuildId, "cmd-err-game-move", col));
            }
        }

        protected override bool IsGameOver()
        {
            // left - right
            for (int i = 0; i < this.SizeX; i++) {
                for (int j = 0; j < this.SizeY - 3; j++) {
                    if (this.board[i, j] == 0)
                        continue;
                    if (this.board[i, j] == this.board[i, j + 1] && this.board[i, j] == this.board[i, j + 2] && this.board[i, j] == this.board[i, j + 3])
                        return true;
                }
            }

            // up - down
            for (int i = 0; i < this.SizeX - 3; i++) {
                for (int j = 0; j < this.SizeY; j++) {
                    if (this.board[i, j] == 0)
                        continue;
                    if (this.board[i, j] == this.board[i + 1, j] && this.board[i, j] == this.board[i + 2, j] && this.board[i, j] == this.board[i + 3, j])
                        return true;
                }
            }

            // diagonal - right
            for (int i = 0; i < this.SizeX - 3; i++) {
                for (int j = 0; j < this.SizeY - 3; j++) {
                    if (this.board[i, j] == 0)
                        continue;
                    if (this.board[i, j] == this.board[i + 1, j + 1] && this.board[i, j] == this.board[i + 2, j + 2] && this.board[i, j] == this.board[i + 3, j + 3])
                        return true;
                }
            }

            // diagonal - left 
            for (int i = 0; i < this.SizeX - 3; i++) {
                for (int j = 3; j < this.SizeY; j++) {
                    if (this.board[i, j] == 0)
                        continue;
                    if (this.board[i, j] == this.board[i + 1, j - 1] && this.board[i, j] == this.board[i + 2, j - 2] && this.board[i, j] == this.board[i + 3, j - 3])
                        return true;
                }
            }

            return false;
        }

        protected override bool TryPlayMove(int val, int row, int col)
        {
            if (this.board[0, col] != 0)
                return false;
            while (row < this.SizeX && this.board[row, col] == 0)
                row++;
            this.board[row - 1, col] = val;
            return true;
        }

        protected override Task<DiscordMessage> UpdateBoardAsync(LocalizationService lcs)
        {
            var sb = new StringBuilder();
            sb.AppendLine(_header);

            for (int i = 0; i < this.SizeX; i++) {
                for (int j = 0; j < this.SizeY; j++)
                    switch (this.board[i, j]) {
                        case 0: sb.Append(Emojis.BoardSquare); break;
                        case 1: sb.Append(Emojis.BoardPieceBlueCircle); break;
                        case 2: sb.Append(Emojis.BoardPieceRedCircle); break;
                    }
                sb.AppendLine();
            }

            sb.AppendLine()
              .Append(lcs.GetString(this.Channel.GuildId, "str-game-move"))
              .AppendLine(this.move % 2 == 0 ? this.player1.Mention : this.player2.Mention);

            return this.msgHandle.ModifyOrResendAsync(this.Channel, new DiscordEmbedBuilder {
                Description = sb.ToString()
            }.Build());
        }
    }
}


