#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("animalrace")]
        [Description("Start a new animal race!")]
        [Aliases("r", "race", "ar")]
        [UsageExample("!game animalrace")]
        [Cooldown(2, 5, CooldownBucketType.User), Cooldown(3, 5, CooldownBucketType.Channel)]
        [ListeningCheck]
        public class RaceModule : TheGodfatherBaseModule
        {

            public RaceModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (Game.RunningInChannel(ctx.Channel.Id)) {
                    if (Game.GetGameInChannel(ctx.Channel.Id) is AnimalRace)
                        await JoinRaceAsync(ctx).ConfigureAwait(false);
                    else
                        throw new CommandFailedException("Another game is already running in the current channel.");
                    return;
                }

                var game = new AnimalRace(ctx.Client.GetInteractivity(), ctx.Channel);
                Game.RegisterGameInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.RespondWithIconEmbedAsync($"The race will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("game animalrace")} to join the race.", ":clock1:")
                        .ConfigureAwait(false);
                    await JoinRaceAsync(ctx)
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(30))
                        .ConfigureAwait(false);

                    if (game.ParticipantCount > 1) {
                        await game.RunAsync()
                            .ConfigureAwait(false);

                        foreach (var uid in game.WinnerIds)
                            await Database.UpdateUserStatsAsync(uid, "races_won")
                                .ConfigureAwait(false);
                    } else {
                        await ctx.RespondWithIconEmbedAsync("Not enough users joined the race.", ":alarm_clock:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }
            

            #region COMMAND_ANIMALRACE_JOIN
            [Command("join")]
            [Description("Join an existing animal race game.")]
            [Aliases("+", "compete", "enter", "j")]
            [UsageExample("!game animalrace join")]
            public async Task JoinRaceAsync(CommandContext ctx)
            {
                var game = Game.GetGameInChannel(ctx.Channel.Id) as AnimalRace;
                if (game == null)
                    throw new CommandFailedException("There is no animal race game running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Race has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Race slots are full (max 10 participants), kthxbye.");

                if (!game.AddParticipant(ctx.User, out DiscordEmoji emoji))
                    throw new CommandFailedException("You are already participating in the race!");

                await ctx.RespondWithIconEmbedAsync($"{ctx.User.Mention} joined the race as {emoji}", ":bicyclist:")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
