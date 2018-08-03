#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services.Common;
using TheGodfather.Services.Database.Stats;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("animalrace"), Module(ModuleType.Games)]
        [Description("Start a new animal race!")]
        [Aliases("animr", "arace", "ar", "animalr")]
        [UsageExamples("!game animalrace")]
        [NotBlocked]
        public class AnimalRaceModule : TheGodfatherModule
        {

            public AnimalRaceModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Shared.GetEventInChannel(ctx.Channel.Id) is AnimalRace)
                        await JoinAsync(ctx).ConfigureAwait(false);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var game = new AnimalRace(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Shared.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await InformAsync(ctx, StaticDiscordEmoji.Clock1, $"The race will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("game animalrace")} to join the race.")
                        .ConfigureAwait(false);
                    await JoinAsync(ctx)
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(30))
                        .ConfigureAwait(false);

                    if (game.ParticipantCount > 1) {
                        await game.RunAsync()
                            .ConfigureAwait(false);

                        foreach (var uid in game.WinnerIds)
                            await Database.UpdateUserStatsAsync(uid, GameStatsType.AnimalRacesWon)
                                .ConfigureAwait(false);
                    } else {
                        await InformAsync(ctx, StaticDiscordEmoji.AlarmClock, "Not enough users joined the race.")
                            .ConfigureAwait(false);
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            

            #region COMMAND_ANIMALRACE_JOIN
            [Command("join"), Module(ModuleType.Games)]
            [Description("Join an existing animal race game.")]
            [Aliases("+", "compete", "enter", "j")]
            [UsageExamples("!game animalrace join")]
            public async Task JoinAsync(CommandContext ctx)
            {
                if (!(this.Shared.GetEventInChannel(ctx.Channel.Id) is AnimalRace game))
                    throw new CommandFailedException("There is no animal race game running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Race has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Race slots are full (max 10 participants), kthxbye.");

                if (!game.AddParticipant(ctx.User, out DiscordEmoji emoji))
                    throw new CommandFailedException("You are already participating in the race!");

                await InformAsync(ctx, $"{ctx.User.Mention} joined the race as {emoji}", ":bicyclist:")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_ANIMALRACE_STATS
            [Command("stats"), Module(ModuleType.Games)]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExamples("!game animalrace stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                var top = await Database.GetTopRacersStringAsync(ctx.Client)
                    .ConfigureAwait(false);

                await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"Top players in Animal Race:\n\n{top}")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
