using System.Text;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Modules.Games.Extensions;

namespace TheGodfather.Modules.Games.Common;

public sealed class CaroGame : BaseBoardGame
{
    private static readonly string _header = Emojis.ArrowUp + Emojis.Numbers.All.Skip(1).JoinWith("");


    public CaroGame(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser player1, DiscordUser player2, TimeSpan? movetime = null)
        : base(interactivity, channel, player1, player2, 10, 10, movetime) { }


    protected override bool IsGameOver()
    {
        // left - right
        for (int i = 0; i < this.SizeX; i++)
        for (int j = 0; j < this.SizeY - 4; j++) {
            if (this.board[i, j] == 0)
                continue;
            if (this.board[i, j] == this.board[i, j + 1] && this.board[i, j] == this.board[i, j + 2] 
                                                         && this.board[i, j] == this.board[i, j + 3] && this.board[i, j] == this.board[i, j + 4])
                return true;
        }

        // up - down
        for (int i = 0; i < this.SizeX - 4; i++)
        for (int j = 0; j < this.SizeY; j++) {
            if (this.board[i, j] == 0)
                continue;
            if (this.board[i, j] == this.board[i + 1, j] && this.board[i, j] == this.board[i + 2, j] 
                                                         && this.board[i, j] == this.board[i + 3, j] && this.board[i, j] == this.board[i + 4, j])
                return true;
        }

        // diagonal - right
        for (int i = 0; i < this.SizeX - 4; i++)
        for (int j = 0; j < this.SizeY - 4; j++) {
            if (this.board[i, j] == 0)
                continue;
            if (this.board[i, j] == this.board[i + 1, j + 1] && this.board[i, j] == this.board[i + 2, j + 2] 
                                                             && this.board[i, j] == this.board[i + 3, j + 3] && this.board[i, j] == this.board[i + 4, j + 4])
                return true;
        }

        // diagonal - left 
        for (int i = 0; i < this.SizeX - 4; i++)
        for (int j = 4; j < this.SizeY; j++) {
            if (this.board[i, j] == 0)
                continue;
            if (this.board[i, j] == this.board[i + 1, j - 1] && this.board[i, j] == this.board[i + 2, j - 2] 
                                                             && this.board[i, j] == this.board[i + 3, j - 3] && this.board[i, j] == this.board[i + 4, j - 4])
                return true;
        }

        return false;
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
                    case 1: sb.Append(Emojis.X); break;
                    case 2: sb.Append(Emojis.O); break;
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