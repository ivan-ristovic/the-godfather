#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule : TheGodfatherModule
    {
        [Group("russianroulette"), Module(ModuleType.Games)]
        [Description("Starts a russian roulette game which I will commentate.")]
        [Aliases("rr", "roulette", "russianr")]
        [UsageExamples("!game russianroulette")]
        public class RussianRouletteModule : TheGodfatherModule
        {

            public RussianRouletteModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Shared.GetEventInChannel(ctx.Channel.Id) is RussianRoulette)
                        await JoinAsync(ctx).ConfigureAwait(false);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var game = new RussianRoulette(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Shared.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.InformSuccessAsync(StaticDiscordEmoji.Clock1, $"The russian roulette game will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("game russianroulette")} to join the pool.")
                        .ConfigureAwait(false);
                    await JoinAsync(ctx)
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(30))
                        .ConfigureAwait(false);

                    if (game.ParticipantCount > 1) {
                        await game.RunAsync()
                            .ConfigureAwait(false);

                        if (game.Survivors.Any())
                            await ctx.InformSuccessAsync(StaticDiscordEmoji.Trophy, $"Survivors:\n\n{string.Join("\n", game.Survivors.Select(u => u.Mention))}").ConfigureAwait(false);
                        else
                            await ctx.InformSuccessAsync(StaticDiscordEmoji.Dead, "Nobody survived!").ConfigureAwait(false);
                    } else {
                        await ctx.InformSuccessAsync(StaticDiscordEmoji.AlarmClock, "Not enough users joined the Russian roulette pool.")
                            .ConfigureAwait(false);
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_RUSSIANROULETTE_JOIN
            [Command("join"), Module(ModuleType.Games)]
            [Description("Join an existing Russian roulette game pool.")]
            [Aliases("+", "compete", "j", "enter")]
            [UsageExamples("!game russianroulette join")]
            public async Task JoinAsync(CommandContext ctx)
            {
                if (!(this.Shared.GetEventInChannel(ctx.Channel.Id) is RussianRoulette game))
                    throw new CommandFailedException("There is no Russian roulette game running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Russian roulette has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Russian roulette slots are full (max 10 participants), kthxbye.");

                if (!game.AddParticipant(ctx.User))
                    throw new CommandFailedException("You are already participating in the Russian roulette!");

                await ctx.InformSuccessAsync(StaticDiscordEmoji.Gun, $"{ctx.User.Mention} joined the Russian roulette pool.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_RUSSIANROULETTE_RULES
            [Command("rules"), Module(ModuleType.Games)]
            [Description("Explain the Russian roulette rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!game numberrace rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ctx.InformSuccessAsync(
                    "Every user has a gun in hand. The game is played in rounds. Each round everyone adds another bullet to their revolvers and rolls." +
                    "After that, everyone pulls the trigger. Those that survive, move on the next round. The game stops when there is only one survivor left, or when round 6 is reached (at that point everyone who is alive up until that point wins).",
                    ":information_source:"
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
