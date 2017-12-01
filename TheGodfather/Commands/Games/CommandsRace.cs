#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TheGodfather.Exceptions;
using TheGodfather.Helpers.DataManagers;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Games
{
    public partial class CommandsGames
    {
        [Group("race", CanInvokeWithoutSubcommand = true)]
        [Description("Racing!")]
        [Aliases("r")]
        [Cooldown(2, 5, CooldownBucketType.User), Cooldown(3, 5, CooldownBucketType.Channel)]
        [PreExecutionCheck]
        public class CommandsRace
        {
            #region PRIVATE_FIELDS
            private ConcurrentDictionary<ulong, Race> _games = new ConcurrentDictionary<ulong, Race>();
            #endregion

            public async Task ExecuteGroupAsync(CommandContext ctx) => await NewRaceAsync(ctx).ConfigureAwait(false);


            #region COMMAND_RACE_NEW
            [Command("new")]
            [Description("Start a new race.")]
            [Aliases("create")]
            public async Task NewRaceAsync(CommandContext ctx)
            {
                Race race;
                if (Race.GameExistsInChannel(ctx.Channel.Id)) {
                    await JoinRaceAsync(ctx)
                        .ConfigureAwait(false);
                    return;
                } else {
                    race = new Race(ctx.Client, ctx.Channel.Id);
                    if (!_games.TryAdd(ctx.Channel.Id, race))
                        throw new CommandFailedException("Failed to create a new racing game! Please try again.");
                    await ctx.RespondAsync("Race will start in 30s or when there are 10 participants. Type " + Formatter.InlineCode("!game race") + " to join the race.")
                        .ConfigureAwait(false);
                }

                await JoinRaceAsync(ctx)
                    .ConfigureAwait(false);
                await Task.Delay(30000)
                    .ConfigureAwait(false);

                if (race.ParticipantCount > 1) {
                    await race.StartRaceAsync()
                        .ConfigureAwait(false);
                } else {
                    await ctx.RespondAsync("Not enough users joined the race.")
                        .ConfigureAwait(false);
                    race.StopRace();
                }

                _games.TryRemove(ctx.Channel.Id, out _);

                var statman = ctx.Dependencies.GetDependency<GameStatsManager>();
                foreach (var uid in race.WinnerIds)
                    statman.UpdateRacesWonForUser(uid);
            }
            #endregion

            #region COMMAND_RACE_JOIN
            [Command("join")]
            [Description("Join a race.")]
            [Aliases("+", "compete")]
            public async Task JoinRaceAsync(CommandContext ctx)
            {
                if (_games[ctx.Channel.Id].GameRunning)
                    throw new CommandFailedException("Race already started, you can't join it.");

                if (_games[ctx.Channel.Id].ParticipantCount >= 10)
                    throw new CommandFailedException("Race is full, kthxbye.");

                var emoji = _games[ctx.Channel.Id].AddParticipant(ctx.User.Id);
                if (emoji == null)
                    throw new CommandFailedException("You are already participating!");

                await ctx.RespondAsync($"{ctx.User.Mention} joined the race as {emoji}")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
