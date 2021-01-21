using System;
using System.Linq;
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
    public sealed class OthelloGame : BaseBoardGame
    {
        private static readonly string _header = Emojis.ArrowUp + Emojis.Numbers.All.Skip(1).Take(8).JoinWith("");


        public OthelloGame(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser player1, DiscordUser player2, TimeSpan? movetime = null)
            : base(interactivity, channel, player1, player2, sizeX: 8, sizeY: 8, movetime)
        {
            this.board[3, 3] = this.board[4, 4] = 1;
            this.board[3, 4] = this.board[4, 3] = 2;
        }


        protected override bool IsGameOver()
        {
            for (int i = 0; i < this.SizeX; i++) {
                for (int j = 0; j < this.SizeY; j++) {
                    if (this.board[i, j] == 0)
                        return false;
                }
            }
            return true;
        }

        protected override bool TryPlayMove(int val, int row, int col)
        {
            if (this.board[row, col] != 0)
                return false;

            bool legal = false;

            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {

                    int posX = col + x;
                    int posY = row + y;
                    bool found = false;
                    int current = this.BoardElementAt(posY, posX);

                    if (current == -1 || current == 0 || current == val)
                        continue;

                    while (!found) {
                        posX += x;
                        posY += y;
                        current = this.BoardElementAt(posY, posX);

                        if (current == val) {
                            found = true;
                            legal = true;
                            posX -= x;
                            posY -= y;
                            current = this.BoardElementAt(posY, posX);

                            while (current != 0) {
                                this.board[posY, posX] = val;
                                posX -= x;
                                posY -= y;
                                current = this.board[posY, posX];
                            }
                        } else if (current is (-1) or 0) {
                            found = true;
                        }
                    }
                }
            }

            if (legal)
                this.board[row, col] = val;

            return legal;
        }

        protected override Task<DiscordMessage> UpdateBoardAsync(LocalizationService lcs)
        {
            var sb = new StringBuilder();
            sb.AppendLine(_header);

            for (int i = 0; i < this.SizeX; i++) {
                sb.Append(Emojis.Numbers.Get(i));
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

        protected override void ResolveGameWinner()
        {
            int p1count = 0, p2count = 0;
            for (int i = 0; i < this.SizeX; i++) {
                for (int j = 0; j < this.SizeY; j++) {
                    if (this.board[i, j] == 0)
                        p1count++;
                    else
                        p2count++;
                }
            }

            this.Winner = p1count == p2count ? null : (p1count > p2count) ? this.player1 : this.player2;
        }
    }
}


