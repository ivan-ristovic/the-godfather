#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("russianroulette")]
        [Description("Starts a russian roulette game which I will commentate.")]
        [Aliases("rr", "roulette", "russianr")]
        public class RussianRouletteModule : TheGodfatherServiceModule<ChannelEventService>
        {

            public RussianRouletteModule(ChannelEventService service, DbContextBuilder db)
                : base(service, db)
            {

            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Service.GetEventInChannel(ctx.Channel.Id) is RussianRouletteGame)
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var game = new RussianRouletteGame(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await this.InformAsync(ctx, Emojis.Clock1, $"The russian roulette game will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("game russianroulette")} to join the pool.");
                    await this.JoinAsync(ctx);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (game.ParticipantCount > 1) {
                        await game.RunAsync();

                        if (game.Survivors.Any())
                            await this.InformAsync(ctx, Emojis.Trophy, $"Survivors:\n\n{string.Join("\n", game.Survivors.Select(u => u.Mention))}");
                        else
                            await this.InformAsync(ctx, Emojis.Dead, "Nobody survived!");
                    } else {
                        await this.InformAsync(ctx, Emojis.AlarmClock, "Not enough users joined the Russian roulette pool.");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_RUSSIANROULETTE_JOIN
            [Command("join")]
            [Description("Join an existing Russian roulette game pool.")]
            [Aliases("+", "compete", "j", "enter")]
            public Task JoinAsync(CommandContext ctx)
            {
                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out RussianRouletteGame game))
                    throw new CommandFailedException("There is no Russian roulette game running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Russian roulette has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Russian roulette slots are full (max 10 participants), kthxbye.");

                if (!game.AddParticipant(ctx.User))
                    throw new CommandFailedException("You are already participating in the Russian roulette!");

                return this.InformAsync(ctx, Emojis.Gun, $"{ctx.User.Mention} joined the Russian roulette pool.");
            }
            #endregion

            #region COMMAND_RUSSIANROULETTE_RULES
            [Command("rules")]
            [Description("Explain the Russian roulette rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
            {
                return this.InformAsync(ctx,
                    Emojis.Information,
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
