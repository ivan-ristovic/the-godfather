using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
        [Group("holdem")]
        [Aliases("poker", "texasholdem", "texas")]
        public sealed class HoldemModule : TheGodfatherServiceModule<ChannelEventService>
        {
            #region casino holdem
            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description(TranslationKey.desc_gamble_balance)] int balance = HoldemGame.DefaultBalance)
            {
                if (balance is < 1 or > (int)MaxBid)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_gamble_bid(MaxBid));

                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Service.GetEventInChannel(ctx.Channel.Id) is HoldemGame)
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException(ctx, TranslationKey.cmd_err_evt_dup);
                    return;
                }

                string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
                var game = new HoldemGame(ctx.Client.GetInteractivity(), ctx.Channel, balance);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.ImpInfoAsync(
                        this.ModuleColor, 
                        Emojis.Clock1, 
                        TranslationKey.str_casino_holdem_start(HoldemGame.MaxParticipants, HoldemGame.DefaultBalance, currency)
                    );
                    await this.JoinAsync(ctx);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    BankAccountService bas = ctx.Services.GetRequiredService<BankAccountService>();
                    if (game.Participants.Count > 1) {
                        await game.RunAsync(this.Localization);
                        if (game.Winner is { })
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Cards.Suits[0], TranslationKey.fmt_winners(game.Winner.Mention));
                    } else {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.AlarmClock, TranslationKey.str_casino_holdem_none);
                    }

                    foreach (HoldemGame.Participant participant in game.Participants)
                        await bas.IncreaseBankAccountAsync(ctx.Guild.Id, participant.Id, participant.Balance);
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region casino holdem join
            [Command("join")]
            [Aliases("+", "compete", "enter", "j", "<<", "<")]
            public async Task JoinAsync(CommandContext ctx)
            {
                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out HoldemGame? game) || game is null)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_casino_holdem_none);

                if (game.Started)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_casino_holdem_started);

                if (game.Participants.Count >= HoldemGame.MaxParticipants)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_casino_holdem_full(HoldemGame.MaxParticipants));

                if (game.IsParticipating(ctx.User))
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_casino_holdem_dup);

                if (!await ctx.Services.GetRequiredService<BankAccountService>().TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, game.MaxBalance))
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_funds_insuf);

                DiscordMessage handle;
                try {
                    DiscordDmChannel? dm = await ctx.Client.CreateDmChannelAsync(ctx.User.Id);
                    if (dm is null)
                        throw new CommandFailedException(ctx, TranslationKey.cmd_err_dm_create);
                    handle = await dm.LocalizedEmbedAsync(this.Localization, Emojis.Cards.Suits[0], this.ModuleColor, TranslationKey.str_casino_holdem_dm);
                } catch {
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_dm_create);
                }

                game.AddParticipant(ctx.User, handle);

                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Cards.Suits[0], TranslationKey.fmt_casino_holdem_join(ctx.User.Mention));
            }
            #endregion

            #region casino holdem rules
            [Command("rules")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
                => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, TranslationKey.str_casino_holdem);
            #endregion
        }
    }
}
