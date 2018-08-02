#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;
using TheGodfather.Services.Database.Stats;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("numberrace"), Module(ModuleType.Games)]
        [Description("Number racing game commands.")]
        [Aliases("nr", "n", "nunchi", "numbers", "numbersrace")]
        [UsageExamples("!game numberrace")]
        [NotBlocked]
        public class NumberRaceModule : TheGodfatherModule
        {

            public NumberRaceModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Shared.GetEventInChannel(ctx.Channel.Id) is NumberRace)
                        await JoinRaceAsync(ctx).ConfigureAwait(false);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var game = new NumberRace(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Shared.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.InformSuccessAsync($"The race will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("game numberrace")} to join the race.", ":clock1:")
                        .ConfigureAwait(false);
                    await JoinRaceAsync(ctx)
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(30))
                        .ConfigureAwait(false);

                    if (game.ParticipantCount > 1) {
                        await game.RunAsync()
                            .ConfigureAwait(false);

                        if (game.IsTimeoutReached) {
                            if (game.Winner != null) {
                                await ctx.InformSuccessAsync(StaticDiscordEmoji.Trophy, $"{game.Winner.Mention} won due to no replies from other users!")
                                    .ConfigureAwait(false);
                            } else {
                                await ctx.InformSuccessAsync("No replies, aborting Number Race...", ":alarm_clock:")
                                        .ConfigureAwait(false);
                            }
                        } else {
                            await ctx.InformSuccessAsync(StaticDiscordEmoji.Trophy, "Winner: " + game.Winner.Mention)
                                .ConfigureAwait(false);
                        }

                        if (game.Winner != null)
                            await Database.UpdateUserStatsAsync(game.Winner.Id, GameStatsType.NumberRacesWon)
                                .ConfigureAwait(false);
                    } else {
                        await ctx.InformSuccessAsync("Not enough users joined the race.", ":alarm_clock:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_NUMBERRACE_JOIN
            [Command("join"), Module(ModuleType.Games)]
            [Description("Join an existing number race game.")]
            [Aliases("+", "compete", "j", "enter")]
            [UsageExamples("!game numberrace join")]
            public async Task JoinRaceAsync(CommandContext ctx)
            {
                if (!(this.Shared.GetEventInChannel(ctx.Channel.Id) is NumberRace game))
                    throw new CommandFailedException("There is no number race game running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Race has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Race slots are full (max 10 participants), kthxbye.");

                if (!game.AddParticipant(ctx.User))
                    throw new CommandFailedException("You are already participating in the race!");

                await ctx.InformSuccessAsync($"{ctx.User.Mention} joined the game.", ":bicyclist:")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_NUMBERRACE_RULES
            [Command("rules"), Module(ModuleType.Games)]
            [Description("Explain the number race rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!game numberrace rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ctx.InformSuccessAsync(
                    "I will start by typing a number. Users have to count up by 1 from that number. " +
                    "If someone makes a mistake (types an incorrent number, or repeats the same number) " +
                    "they are out of the game. If nobody posts a number 20s after the last number was posted, " +
                    "then the user that posted that number wins the game. The game ends when only one user remains.",
                    ":information_source:"
                ).ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_NUMBERRACE_STATS
            [Command("stats"), Module(ModuleType.Games)]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExamples("!game numberrace stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                var top = await Database.GetTopNunchiPlayersStringAsync(ctx.Client)
                    .ConfigureAwait(false);

                await ctx.InformSuccessAsync(StaticDiscordEmoji.Trophy, $"Top players in Number Race:\n\n{top}")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}