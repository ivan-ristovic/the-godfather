#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Gambling.Common;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Gambling
{
    public partial class CasinoModule
    {
        [Group("blackjack"), Module(ModuleType.Gambling)]
        [Description("Play a blackjack game.")]
        [Aliases("bj")]
        [UsageExample("!casino blackjack")]
        public class BlackjackModule : TheGodfatherBaseModule
        {

            public BlackjackModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (Game.RunningInChannel(ctx.Channel.Id)) {
                    if (Game.GetGameInChannel(ctx.Channel.Id) is BlackjackGame)
                        await JoinAsync(ctx).ConfigureAwait(false);
                    else
                        throw new CommandFailedException("Another game is already running in the current channel.");
                    return;
                }

                var game = new BlackjackGame(ctx.Client.GetInteractivity(), ctx.Channel);
                Game.RegisterGameInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.RespondWithIconEmbedAsync($"The Blackjack game will start in 30s or when there are 5 participants. Use command {Formatter.InlineCode("casino blackjack")} to join the pool.", ":clock1:")
                        .ConfigureAwait(false);
                    await JoinAsync(ctx)
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(30))
                        .ConfigureAwait(false);

                    if (game.ParticipantCount > 1) {
                        await game.RunAsync()
                            .ConfigureAwait(false);

                        /*
                        foreach (var uid in game.WinnerIds)
                            await Database.UpdateUserStatsAsync(uid, GameStatsType.AnimalRacesWon)
                                .ConfigureAwait(false);
                        */
                    } else {
                        await ctx.RespondWithIconEmbedAsync("Not enough users joined the Blackjack pool.", ":alarm_clock:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_BLACKJACK_JOIN
            [Command("join"), Module(ModuleType.Games)]
            [Description("Join a pending Blackjack game.")]
            [Aliases("+", "compete", "enter", "j")]
            [UsageExample("!casino blackjack join")]
            public async Task JoinAsync(CommandContext ctx)
            {
                var game = Game.GetGameInChannel(ctx.Channel.Id) as BlackjackGame;
                if (game == null)
                    throw new CommandFailedException("There is no Blackjack game running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Blackjack game has already started, you can't join it.");

                if (game.ParticipantCount >= 5)
                    throw new CommandFailedException("Blackjack slots are full (max 5 participants), kthxbye.");

                if (!game.AddParticipant(ctx.User, 0))
                    throw new CommandFailedException("You are already participating in the race!");

                await ctx.RespondWithIconEmbedAsync($"{ctx.User.Mention} joined the Blackjack game.", ":spades:")
                    .ConfigureAwait(false);
            }
            #endregion

            /*
            #region COMMAND_BLACKJACK_STATS
            [Command("stats"), Module(ModuleType.Games)]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExample("!casino blackjack stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                var top = await Database.???(ctx.Client)
                    .ConfigureAwait(false);

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Trophy, $"Top players in Blackjack:\n\n{top}")
                    .ConfigureAwait(false);
            }
            #endregion
            */
        }
    }
}
