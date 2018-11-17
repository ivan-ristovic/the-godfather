#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Games.Common;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("russianroulette")]
        [Description("Starts a russian roulette game which I will commentate.")]
        [Aliases("rr", "roulette", "russianr")]
        [UsageExamples("!game russianroulette")]
        public class RussianRouletteModule : TheGodfatherModule
        {

            public RussianRouletteModule(SharedData shared, DatabaseContextBuilder db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Teal;
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Shared.GetEventInChannel(ctx.Channel.Id) is RussianRouletteGame)
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var game = new RussianRouletteGame(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Shared.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await this.InformAsync(ctx, StaticDiscordEmoji.Clock1, $"The russian roulette game will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("game russianroulette")} to join the pool.");
                    await this.JoinAsync(ctx);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (game.ParticipantCount > 1) {
                        await game.RunAsync();

                        if (game.Survivors.Any())
                            await this.InformAsync(ctx, StaticDiscordEmoji.Trophy, $"Survivors:\n\n{string.Join("\n", game.Survivors.Select(u => u.Mention))}");
                        else
                            await this.InformAsync(ctx, StaticDiscordEmoji.Dead, "Nobody survived!");
                    } else {
                        await this.InformAsync(ctx, StaticDiscordEmoji.AlarmClock, "Not enough users joined the Russian roulette pool.");
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_RUSSIANROULETTE_JOIN
            [Command("join")]
            [Description("Join an existing Russian roulette game pool.")]
            [Aliases("+", "compete", "j", "enter")]
            [UsageExamples("!game russianroulette join")]
            public Task JoinAsync(CommandContext ctx)
            {
                if (!(this.Shared.GetEventInChannel(ctx.Channel.Id) is RussianRouletteGame game))
                    throw new CommandFailedException("There is no Russian roulette game running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Russian roulette has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Russian roulette slots are full (max 10 participants), kthxbye.");

                if (!game.AddParticipant(ctx.User))
                    throw new CommandFailedException("You are already participating in the Russian roulette!");

                return this.InformAsync(ctx, StaticDiscordEmoji.Gun, $"{ctx.User.Mention} joined the Russian roulette pool.");
            }
            #endregion

            #region COMMAND_RUSSIANROULETTE_RULES
            [Command("rules")]
            [Description("Explain the Russian roulette rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!game numberrace rules")]
            public Task RulesAsync(CommandContext ctx)
            {
                return this.InformAsync(ctx,
                    StaticDiscordEmoji.Information,
                    "Every user has a gun in hand. The game is played in rounds. Each round everyone adds another " +
                    "bullet to their revolvers and rolls. After that, everyone pulls the trigger. Those that " +
                    "survive, move on the next round. The game stops when there is only one survivor left, or when " +
                    "round 6 is reached (at that point everyone who is alive up until that point wins)."
                );
            }
            #endregion
        }
    }
}
