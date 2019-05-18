#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public abstract class BaseBoardGame : BaseChannelGame
    {
        protected readonly int SizeY;
        protected readonly int SizeX;
        protected int[,] board;
        protected bool deleteErrored;
        protected int move;
        protected TimeSpan moveTime;
        protected DiscordMessage msgHandle;
        protected DiscordUser player1;
        protected DiscordUser player2;


        protected BaseBoardGame(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser p1,
            DiscordUser p2, int sizeX, int sizeY, TimeSpan? movetime = null)
            : base(interactivity, channel)
        {
            this.SizeY = sizeX;
            this.SizeX = sizeY;
            this.board = new int[this.SizeX, this.SizeY];
            this.player1 = p1;
            this.player2 = p2;
            this.moveTime = movetime ?? TimeSpan.FromSeconds(30);
        }


        protected int BoardElementAt(int row, int col)
            => (col >= 0 && col < this.SizeY && row >= 0 && row < this.SizeX) ? this.board[row, col] : -1;


        public sealed override async Task RunAsync()
        {
            this.msgHandle = await this.Channel.SendMessageAsync($"{this.player1.Mention} vs {this.player2.Mention}");

            while (!this.IsTimeoutReached && this.move < this.SizeY * this.SizeX && !this.IsGameOver()) {
                await this.UpdateBoardAsync();
                await this.AdvanceAsync();
            }

            if (this.IsGameOver())
                this.ResolveGameWinner();

            await this.UpdateBoardAsync();
        }


        protected virtual bool TryPlayMove(int val, int row, int col)
        {
            if (this.board[row, col] != 0)
                return false;
            this.board[row, col] = val;
            return true;
        }

        protected virtual async Task AdvanceAsync()
        {
            int row = 0, col = 0;
            bool player1plays = (this.move % 2 == 0);
            InteractivityResult<DiscordMessage> mctx = await this.Interactivity.WaitForMessageAsync(
                xm => {
                    if (xm.Channel.Id != this.Channel.Id || xm.Author.IsBot) return false;
                    if (player1plays && (xm.Author.Id != this.player1.Id)) return false;
                    if (!player1plays && (xm.Author.Id != this.player2.Id)) return false;
                    string[] split = xm.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length < 2) return false;
                    if (!int.TryParse(split[0], out row)) return false;
                    if (!int.TryParse(split[1], out col)) return false;
                    return row > 0 && row <= this.SizeX && col > 0 && col <= this.SizeY;
                },
                this.moveTime
            );
            if (mctx.TimedOut) {
                this.IsTimeoutReached = true;
                this.Winner = player1plays ? this.player2 : this.player1;
                return;
            }

            if (this.TryPlayMove(player1plays ? 1 : 2, row - 1, col - 1)) {
                this.move++;
                if (!this.deleteErrored) {
                    try {
                        await mctx.Result.DeleteAsync();
                    } catch {
                        await this.Channel.InformFailureAsync("Consider giving me the permissions to delete messages so that I can clean up the move posts.");
                        this.deleteErrored = true;
                    }
                }
            } else {
                await this.Channel.InformFailureAsync($"Move [{row} {col}] is invalid.");
            }
        }

        protected virtual void ResolveGameWinner()
            => this.Winner = (this.move % 2 == 0) ? this.player2 : this.player1;


        protected abstract bool IsGameOver();
        protected abstract Task UpdateBoardAsync();
    }
}
