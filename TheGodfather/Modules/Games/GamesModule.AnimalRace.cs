#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services.Common;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.Stats;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("animalrace")]
        [Description("Start a new animal race!")]
        [Aliases("animr", "arace", "ar", "animalr")]
        [UsageExamples("!game animalrace")]
        public class AnimalRaceModule : TheGodfatherModule
        {

            public AnimalRaceModule(SharedData shared, DBService db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Teal;
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Shared.GetEventInChannel(ctx.Channel.Id) is AnimalRace)
                        await JoinAsync(ctx);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var game = new AnimalRace(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Shared.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await InformAsync(ctx, StaticDiscordEmoji.Clock1, $"The race will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("game animalrace")} to join the race.");
                    await JoinAsync(ctx);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (game.ParticipantCount > 1) {
                        await game.RunAsync();

                        foreach (ulong uid in game.WinnerIds)
                            await Database.UpdateUserStatsAsync(uid, GameStatsType.AnimalRacesWon);
                    } else {
                        await InformAsync(ctx, StaticDiscordEmoji.AlarmClock, "Not enough users joined the race.");
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            

            #region COMMAND_ANIMALRACE_JOIN
            [Command("join")]
            [Description("Join an existing animal race game.")]
            [Aliases("+", "compete", "enter", "j")]
            [UsageExamples("!game animalrace join")]
            public Task JoinAsync(CommandContext ctx)
            {
                if (!(this.Shared.GetEventInChannel(ctx.Channel.Id) is AnimalRace game))
                    throw new CommandFailedException("There is no animal race game running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Race has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Race slots are full (max 10 participants), kthxbye.");

                if (!game.AddParticipant(ctx.User, out DiscordEmoji emoji))
                    throw new CommandFailedException("You are already participating in the race!");

                return InformAsync(ctx, StaticDiscordEmoji.Bicyclist, $"{ctx.User.Mention} joined the race as {emoji}");
            }
            #endregion

            #region COMMAND_ANIMALRACE_STATS
            [Command("stats")]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExamples("!game animalrace stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                string top = await Database.GetTopRacersStringAsync(ctx.Client);
                await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"Top players in Animal Race:\n\n{top}");
            }
            #endregion
        }
    }
}
