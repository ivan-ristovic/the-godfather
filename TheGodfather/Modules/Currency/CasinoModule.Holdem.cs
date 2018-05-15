#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Currency
{
    public partial class CasinoModule
    {
        [Group("holdem"), Module(ModuleType.Currency)]
        [Description("Play a Texas Hold'Em game.")]
        [Aliases("poker", "texasholdem", "texas")]
        [UsageExample("!casino holdem 10000")]
        public class HoldemModule : TheGodfatherBaseModule
        {

            public HoldemModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Amount of money required to enter.")] int balance = 1000)
            {
                if (balance < 5)
                    throw new InvalidCommandUsageException("Entering balance cannot be lower than 5 credits.");

                if (ChannelEvent.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is HoldemGame)
                        await JoinAsync(ctx).ConfigureAwait(false);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                long? total = await Database.GetUserCreditAmountAsync(ctx.User.Id)
                    .ConfigureAwait(false);
                if (!total.HasValue || total < balance)
                    throw new CommandFailedException("You do not have that many credits on your account! Specify a smaller entering amount.");

                var game = new HoldemGame(ctx.Client.GetInteractivity(), ctx.Channel, balance);
                ChannelEvent.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.RespondWithIconEmbedAsync($"The Hold'Em game will start in 30s or when there are 7 participants. Use command {Formatter.InlineCode("casino holdem <entering sum>")} to join the pool. Entering sum is set to {game.MoneyNeeded} credits.", ":clock1:")
                        .ConfigureAwait(false);
                    await JoinAsync(ctx)
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(30))
                        .ConfigureAwait(false);

                    if (game.ParticipantCount > 1) {
                        await game.RunAsync()
                            .ConfigureAwait(false);

                        if (game.Winner != null) {
                            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Trophy, $"Winner: {game.Winner.Mention}")
                                .ConfigureAwait(false);
                        }

                        foreach (var participant in game.Participants) {
                            await Database.GiveCreditsToUserAsync(ctx.User.Id, participant.Balance)
                                .ConfigureAwait(false);
                        }
                    } else {
                        await Database.GiveCreditsToUserAsync(ctx.User.Id, game.MoneyNeeded)
                            .ConfigureAwait(false);
                        await ctx.RespondWithIconEmbedAsync("Not enough users joined the Hold'Em game.", ":alarm_clock:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    ChannelEvent.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_HOLDEM_JOIN
            [Command("join"), Module(ModuleType.Currency)]
            [Description("Join a pending Texas Hold'Em game.")]
            [Aliases("+", "compete", "enter", "j")]
            [UsageExample("!casino holdem join")]
            public async Task JoinAsync(CommandContext ctx)
            {
                if (!(ChannelEvent.GetEventInChannel(ctx.Channel.Id) is HoldemGame game))
                    throw new CommandFailedException("There are no Texas Hold'Em games running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Texas Hold'Em game has already started, you can't join it.");

                if (game.ParticipantCount >= 7)
                    throw new CommandFailedException("Texas Hold'Em slots are full (max 7 participants), kthxbye.");
                
                if (game.IsParticipating(ctx.User))
                    throw new CommandFailedException("You are already participating in the Texas Hold'Em game!");

                DiscordMessage handle;
                try {
                    var dm = await ctx.Client.CreateDmChannelAsync(ctx.User.Id)
                        .ConfigureAwait(false);
                    handle = await dm.SendMessageAsync("Alright, waiting for Hold'Em game to start! Once the game starts, return here to see your hand!")
                        .ConfigureAwait(false);
                } catch {
                    throw new CommandFailedException("I can't send you a message! Please enable DMs from me so I can send you the cards.");
                }

                if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, game.MoneyNeeded))
                    throw new CommandFailedException("You do not have that many credits on your account! Specify a smaller bid amount.");

                game.AddParticipant(ctx.User, handle);
                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.CardSuits[0], $"{ctx.User.Mention} joined the Hold'Em game.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_HOLDEM_RULES
            [Command("rules"), Module(ModuleType.Currency)]
            [Description("Explain the Texas Hold'Em rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExample("!casino holdem rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ctx.RespondWithIconEmbedAsync(
                    "TODO",
                    ":information_source:"
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
