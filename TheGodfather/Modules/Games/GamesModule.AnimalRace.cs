#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("animalrace")]
        [Description("Start a new animal race!")]
        [Aliases("animr", "arace", "ar", "animalr")]
        public class AnimalRaceModule : TheGodfatherServiceModule<ChannelEventService>
        {

            public AnimalRaceModule(ChannelEventService service, DbContextBuilder db)
                : base(service, db)
            {

            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Service.GetEventInChannel(ctx.Channel.Id) is AnimalRace)
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var game = new AnimalRace(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await this.InformAsync(ctx, Emojis.Clock1, $"The race will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("game animalrace")} to join the race.");
                    await this.JoinAsync(ctx);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (game.ParticipantCount > 1) {
                        await game.RunAsync();

                        foreach (ulong uid in game.WinnerIds)
                            await this.Database.UpdateStatsAsync(uid, s => s.AnimalRacesWon++);
                    } else {
                        await this.InformAsync(ctx, Emojis.AlarmClock, "Not enough users joined the race.");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_ANIMALRACE_JOIN
            [Command("join")]
            [Description("Join an existing animal race game.")]
            [Aliases("+", "compete", "enter", "j")]
            public Task JoinAsync(CommandContext ctx)
            {
                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out AnimalRace game))
                    throw new CommandFailedException("There is no animal race game running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Race has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Race slots are full (max 10 participants), kthxbye.");

                if (!game.AddParticipant(ctx.User, out DiscordEmoji emoji))
                    throw new CommandFailedException("You are already participating in the race!");

                return this.InformAsync(ctx, Emojis.Bicyclist, $"{ctx.User.Mention} joined the race as {emoji}");
            }
            #endregion

            #region COMMAND_ANIMALRACE_STATS
            [Command("stats")]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            public async Task StatsAsync(CommandContext ctx)
            {
                IReadOnlyList<GameStats> topStats = await this.Database.GetTopAnimalRaceStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildAnimalRaceStatsString());
                await this.InformAsync(ctx, Emojis.Trophy, $"Top players in Animal Race:\n\n{topStats}");
            }
            #endregion
        }
    }
}
