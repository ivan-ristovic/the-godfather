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
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Currency
{
    public partial class CasinoModule
    {
        [Group("holdem")]
        [Description("Play a Texas Hold'Em game.")]
        [Aliases("poker", "texasholdem", "texas")]
        [UsageExamples("!casino holdem 10000")]
        public class HoldemModule : TheGodfatherModule
        {

            public HoldemModule(SharedData shared, DBService db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.SapGreen;
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Amount of money required to enter.")] int amount = 1000)
            {
                if (amount < 5)
                    throw new InvalidCommandUsageException($"Entering balance cannot be lower than 5 {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}");

                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Shared.GetEventInChannel(ctx.Channel.Id) is HoldemGame)
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }
                
                var game = new HoldemGame(ctx.Client.GetInteractivity(), ctx.Channel, amount);
                this.Shared.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await this.InformAsync(ctx, StaticDiscordEmoji.Clock1, $"The Hold'Em game will start in 30s or when there are 7 participants. Use command {Formatter.InlineCode("casino holdem <entering sum>")} to join the pool. Entering sum is set to {game.MoneyNeeded} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}.");
                    await this.JoinAsync(ctx);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (game.Participants.Count > 1) {
                        await game.RunAsync();

                        if (!(game.Winner is null))
                            await this.InformAsync(ctx, StaticDiscordEmoji.Trophy, $"Winner: {game.Winner.Mention}");

                        using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                            foreach (HoldemParticipant participant in game.Participants)
                                await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, v => v + participant.Balance);
                            await db.SaveChangesAsync();
                        }
                    } else {
                        if (game.IsParticipating(ctx.User)) {
                            using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                                await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, v => v + game.MoneyNeeded);
                                await db.SaveChangesAsync();
                            }
                        }
                        await this.InformAsync(ctx, StaticDiscordEmoji.AlarmClock, "Not enough users joined the Hold'Em game.");
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_HOLDEM_JOIN
            [Command("join")]
            [Description("Join a pending Texas Hold'Em game.")]
            [Aliases("+", "compete", "enter", "j", "<<", "<")]
            [UsageExamples("!casino holdem join")]
            public async Task JoinAsync(CommandContext ctx)
            {
                if (!(this.Shared.GetEventInChannel(ctx.Channel.Id) is HoldemGame game))
                    throw new CommandFailedException("There are no Texas Hold'Em games running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Texas Hold'Em game has already started, you can't join it.");

                if (game.Participants.Count >= 7)
                    throw new CommandFailedException("Texas Hold'Em slots are full (max 7 participants), kthxbye.");

                if (game.IsParticipating(ctx.User))
                    throw new CommandFailedException("You are already participating in the Texas Hold'Em game!");

                DiscordMessage handle;
                try {
                    DiscordDmChannel dm = await ctx.Client.CreateDmChannelAsync(ctx.User.Id);
                    handle = await dm.SendMessageAsync("Alright, waiting for Hold'Em game to start! Once the game starts, return here to see your hand!");
                } catch {
                    throw new CommandFailedException("I can't send you a message! Please enable DMs from me so I can send you the cards.");
                }

                using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                    if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, game.MoneyNeeded))
                        throw new CommandFailedException($"You do not have enough {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}! Use command {Formatter.InlineCode("bank")} to check your account status.");
                    await db.SaveChangesAsync();
                }
                
                game.AddParticipant(ctx.User, handle);
                await this.InformAsync(ctx, StaticDiscordEmoji.CardSuits[0], $"{ctx.User.Mention} joined the Hold'Em game.");
            }
            #endregion

            #region COMMAND_HOLDEM_RULES
            [Command("rules")]
            [Description("Explain the Texas Hold'Em rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!casino holdem rules")]
            public Task RulesAsync(CommandContext ctx)
            {
                return this.InformAsync(ctx,
                    StaticDiscordEmoji.Information,
                    "Texas hold 'em (also known as Texas holdem, hold 'em, and holdem) is a variation of " +
                    "the card game of poker. Two cards, known as the hole cards, are dealt face down to " +
                    "each player, and then five community cards are dealt face up in three stages. The " +
                    "stages consist of a series of three cards (\"the flop\"), later an additional single " +
                    "card (\"the turn\" or \"fourth street\"), and a final card (\"the river\" or \"fifth " +
                    "street\"). Each player seeks the best five card poker hand from any combination of " +
                    "the seven cards of the five community cards and their own two hole cards."
                );
            }
            #endregion
        }
    }
}
