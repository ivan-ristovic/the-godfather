using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Currency
{
    public partial class CasinoModule
    {
        [Group("blackjack")]
        [Aliases("bj")]
        public sealed class BlackjackModule : TheGodfatherServiceModule<ChannelEventService>
        {
            #region casino blackjack
            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-gamble-bid")] int bid = BlackjackGame.InitialBid)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Service.GetEventInChannel(ctx.Channel.Id) is BlackjackGame)
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException(ctx, "cmd-err-evt-dup");
                    return;
                }

                string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
                var game = new BlackjackGame(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, "str-casino-blackjack-start", 
                        BlackjackGame.MaxParticipants, BlackjackGame.InitialBid, currency
                    );
                    await this.JoinAsync(ctx, bid);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    BankAccountService bas = ctx.Services.GetRequiredService<BankAccountService>();
                    if (game.ParticipantCount > 0) {
                        await game.RunAsync(ctx.Services.GetRequiredService<LocalizationService>());

                        if (game.Winners.Any()) {
                            if (game.Winner is null) {
                                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Cards.Suits[0], "fmt-winners",
                                    game.Winners.Select(w => w.User.Mention).JoinWith(", ")
                                );
                                foreach (BlackjackGame.Participant winner in game.Winners)
                                    await bas.IncreaseBankAccountAsync(ctx.Guild.Id, winner.Id, winner.Bid * 2);
                            } else {
                                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Cards.Suits[0], "str-casino-blackjack-win", game.Winner.Mention);
                                await bas.IncreaseBankAccountAsync(ctx.Guild.Id, game.Winner.Id, game.Winners.Single(p => p.Id == game.Winner.Id).Bid * 2);
                            }
                        } else {
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Cards.Suits[0], "str-casino-blackjack-lose");
                        }
                    } else {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.AlarmClock, "str-casino-blackjack-none");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region casino blackjack join
            [Command("join")]
            [Aliases("+", "compete", "enter", "j", "<<", "<")]
            public async Task JoinAsync(CommandContext ctx,
                                       [Description("desc-gamble-bid")] int bid = BlackjackGame.InitialBid)
            {
                if (bid < 1 || bid > MaxBid)
                    throw new CommandFailedException(ctx, "cmd-err-gamble-bid", MaxBid);
                
                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out BlackjackGame? game) || game is null)
                    throw new CommandFailedException(ctx, "cmd-err-casino-blackjack-none");

                if (game.Started)
                    throw new CommandFailedException(ctx, "cmd-err-casino-blackjack-started");

                if (game.ParticipantCount >= BlackjackGame.MaxParticipants)
                    throw new CommandFailedException(ctx, "cmd-err-casino-blackjack-full", BlackjackGame.MaxParticipants);

                if (game.IsParticipating(ctx.User))
                    throw new CommandFailedException(ctx, "cmd-err-casino-blackjack-dup");

                if (!await ctx.Services.GetRequiredService<BankAccountService>().TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, bid))
                    throw new CommandFailedException(ctx, "cmd-err-funds-insuf");

                game.AddParticipant(ctx.User, bid);

                string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Cards.Suits[0], "fmt-casino-blackjack-join", ctx.User.Mention, bid, currency);
            }
            #endregion

            #region casino blackjack rules
            [Command("rules")]
            [Aliases("help", "h", "ruling", "rule", "info")]
            public Task RulesAsync(CommandContext ctx)
                => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, "str-casino-blackjack");
            #endregion
        }
    }
}
