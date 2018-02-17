#region USING_DIRECTIVES
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Extensions;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public sealed class Caro : BoardGame
    {
        private static string _header = DiscordEmoji.FromUnicode("\U0001f199") + string.Join("", EmojiUtil.Numbers);


        public Caro(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser player1, DiscordUser player2)
            : base(interactivity, channel, player1, player2, 10, 10) { }
        

        protected override bool GameOver()
        {
            // left - right
            for (int i = 0; i < BOARD_SIZE_Y; i++) {
                for (int j = 0; j < BOARD_SIZE_X - 4; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i, j + 1] && _board[i, j] == _board[i, j + 2] && _board[i, j] == _board[i, j + 3] && _board[i, j] == _board[i, j + 4])
                        return true;
                }
            }

            // up - down
            for (int i = 0; i < BOARD_SIZE_Y - 4; i++) {
                for (int j = 0; j < BOARD_SIZE_X; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j] && _board[i, j] == _board[i + 2, j] && _board[i, j] == _board[i + 3, j] && _board[i, j] == _board[i + 4, j])
                        return true;
                }
            }

            // diagonal - right
            for (int i = 0; i < BOARD_SIZE_Y - 4; i++) {
                for (int j = 0; j < BOARD_SIZE_X - 4; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j + 1] && _board[i, j] == _board[i + 2, j + 2] && _board[i, j] == _board[i + 3, j + 3] && _board[i, j] == _board[i + 4, j + 4])
                        return true;
                }
            }

            // diagonal - left 
            for (int i = 0; i < BOARD_SIZE_Y - 4; i++) {
                for (int j = 4; j < BOARD_SIZE_X; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j - 1] && _board[i, j] == _board[i + 2, j - 2] && _board[i, j] == _board[i + 3, j - 3] && _board[i, j] == _board[i + 4, j - 4])
                        return true;
                }
            }

            return false;
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
                        case 1: sb.Append(EmojiUtil.BoardPieceX); break;
                        case 2: sb.Append(EmojiUtil.BoardPieceO); break;
                    }
                sb.AppendLine();
            }

            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() {
                Description = sb.ToString()
            }.Build()).ConfigureAwait(false);
        }
    }
}


