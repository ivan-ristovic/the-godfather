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
                                               [Description(TranslationKey.desc_gamble_bid)] int bid = BlackjackGame.InitialBid)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Service.GetEventInChannel(ctx.Channel.Id) is BlackjackGame)
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException(ctx, TranslationKey.cmd_err_evt_dup);
                    return;
                }

                string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
                var game = new BlackjackGame(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.ImpInfoAsync(
                        this.ModuleColor, 
                        Emojis.Clock1, 
                        TranslationKey.str_casino_blackjack_start(BlackjackGame.MaxParticipants, BlackjackGame.InitialBid, currency)
                    );
                    await this.JoinAsync(ctx, bid);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    BankAccountService bas = ctx.Services.GetRequiredService<BankAccountService>();
                    if (game.ParticipantCount > 0) {
                        await game.RunAsync(this.Localization);
                        if (game.Winners.Any()) {
                            if (game.Winner is null) {
                                await ctx.ImpInfoAsync(
                                    this.ModuleColor, 
                                    Emojis.Cards.Suits[0], 
                                    TranslationKey.fmt_winners(game.Winners.Select(w => w.User.Mention).JoinWith(", "))
                                );
                                foreach (BlackjackGame.Participant winner in game.Winners)
                                    await bas.IncreaseBankAccountAsync(ctx.Guild.Id, winner.Id, winner.Bid * 2);
                            } else {
                                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Cards.Suits[0], TranslationKey.str_casino_blackjack_win(game.Winner.Mention));
                                await bas.IncreaseBankAccountAsync(ctx.Guild.Id, game.Winner.Id, game.Winners.Single(p => p.Id == game.Winner.Id).Bid * 2);
                            }
                        } else {
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Cards.Suits[0], TranslationKey.str_casino_blackjack_lose);
                        }
                    } else {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.AlarmClock, TranslationKey.str_casino_blackjack_none);
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
                                       [Description(TranslationKey.desc_gamble_bid)] int bid = BlackjackGame.InitialBid)
            {
                if (bid is < 1 or > (int)MaxBid)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_gamble_bid(MaxBid));
                
                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out BlackjackGame? game) || game is null)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_casino_blackjack_none);

                if (game.Started)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_casino_blackjack_started);

                if (game.ParticipantCount >= BlackjackGame.MaxParticipants)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_casino_blackjack_full(BlackjackGame.MaxParticipants));

                if (game.IsParticipating(ctx.User))
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_casino_blackjack_dup);

                if (!await ctx.Services.GetRequiredService<BankAccountService>().TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, bid))
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_funds_insuf);

                game.AddParticipant(ctx.User, bid);

                string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Cards.Suits[0], TranslationKey.fmt_casino_blackjack_join(ctx.User.Mention, bid, currency));
            }
            #endregion

            #region casino blackjack rules
            [Command("rules")]
            [Aliases("help", "h", "ruling", "rule", "info")]
            public Task RulesAsync(CommandContext ctx)
                => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, TranslationKey.str_casino_blackjack);
            #endregion
        }
    }
}
