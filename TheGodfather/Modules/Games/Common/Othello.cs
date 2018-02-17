#region USING_DIRECTIVES
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Extensions;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public sealed class Othello : BoardGame
    {
        private static string _header = DiscordEmoji.FromUnicode("\U0001f199") + string.Join("", EmojiUtil.Numbers.Take(8));


        public Othello(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser player1, DiscordUser player2)
            : base(interactivity, channel, player1, player2, 8, 8)
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
                sb.Append(EmojiUtil.Numbers[i]);
                for (int j = 0; j < BOARD_SIZE_X; j++)
                    switch (_board[i, j]) {
                        case 0: sb.Append(EmojiUtil.BoardSquare); break;
                        case 1: sb.Append(EmojiUtil.BoardPieceBlueCircle); break;
                        case 2: sb.Append(EmojiUtil.BoardPieceRedCircle); break;
                    }
                sb.AppendLine();
            }

            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() {
                Description = sb.ToString()
            }.Build()).ConfigureAwait(false);
        }
    }
}


