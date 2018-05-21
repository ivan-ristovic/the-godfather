#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public sealed class Othello : BoardGame
    {
        private static readonly string _header = DiscordEmoji.FromUnicode("\U0001f199") + string.Join("", StaticDiscordEmoji.Numbers.Take(8));


        public Othello(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser player1, DiscordUser player2, TimeSpan? movetime = null)
            : base(interactivity, channel, player1, player2, 8, 8, movetime)
        {
            _board[3, 3] = _board[4, 4] = 1;
            _board[3, 4] = _board[4, 3] = 2;
        }


        protected override bool GameOver()
        {
            for (int i = 0; i < BOARD_SIZE_Y; i++)
                for (int j = 0; j < BOARD_SIZE_X; j++)
                    if (_board[i, j] == 0)
                        return false;
            return true;
        }

        protected override bool TryPlayMove(int val, int row, int col)
        {
            if (_board[row, col] != 0)
                return false;

            bool legal = false;

            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {

                    int posX = col + x;
                    int posY = row + y;
                    bool found = false;
                    int current = BoardElementAt(posY, posX);
                    
                    if (current == -1 || current == 0 || current == val)
                        continue;
                    
                    while (!found) {
                        posX += x;
                        posY += y;
                        current = BoardElementAt(posY, posX);

                        if (current == val) {
                            found = true;
                            legal = true;
                            posX -= x;
                            posY -= y;
                            current = BoardElementAt(posY, posX);

                            while (current != 0) {
                                _board[posY, posX] = val;
                                posX -= x;
                                posY -= y;
                                current = _board[posY, posX];
                            }
                        } else if (current == -1 || current == 0) {
                            found = true;
                        }
                    }
                }
            }

            if (legal)
                _board[row, col] = val;

            return legal;
        }

        protected override async Task UpdateBoardAsync()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(_header);
            for (int i = 0; i < BOARD_SIZE_Y; i++) {
                sb.Append(StaticDiscordEmoji.Numbers[i]);
                for (int j = 0; j < BOARD_SIZE_X; j++)
                    switch (_board[i, j]) {
                        case 0: sb.Append(StaticDiscordEmoji.BoardSquare); break;
                        case 1: sb.Append(StaticDiscordEmoji.BoardPieceBlueCircle); break;
                        case 2: sb.Append(StaticDiscordEmoji.BoardPieceRedCircle); break;
                    }
                sb.AppendLine();
            }

            sb.AppendLine().Append("User to move: ").AppendLine(_move % 2 == 0 ? _p1.Mention : _p2.Mention);

            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() {
                Description = sb.ToString()
            }.Build()).ConfigureAwait(false);
        }

        protected override void ResolveWinner()
        {
            int p1count = 0, p2count = 0;
            for (int i = 0; i < BOARD_SIZE_Y; i++) {
                for (int j = 0; j < BOARD_SIZE_X; j++) {
                    if (_board[i, j] == 0)
                        p1count++;
                    else
                        p2count++;
                }
            }

            if (p1count == p2count)
                Winner = null;
            else
                Winner = (p1count > p2count) ? _p1 : _p2;
        }
    }
}


