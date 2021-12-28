using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Chickens.Services;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Chickens;

public partial class ChickenModule
{
    [Group("buy")][UsesInteractivity]
    [Aliases("b", "shop")]
    public sealed class BuyModule : TheGodfatherServiceModule<ChickenService>
    {
        #region chicken buy
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_chicken_name)] string name)
            => this.TryBuyInternalAsync(ctx, ChickenType.Default, name);
        #endregion

        #region chicken buy default
        [Command("default")]
        [Aliases("d", "def")]
        public Task DefaultAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_chicken_name)] string name)
            => this.TryBuyInternalAsync(ctx, ChickenType.Default, name);
        #endregion

        #region chicken buy wellfed
        [Command("wellfed")]
        [Aliases("wf", "fed")]
        public Task WellFedAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_chicken_name)] string name)
            => this.TryBuyInternalAsync(ctx, ChickenType.WellFed, name);
        #endregion

        #region chicken buy trained
        [Command("trained")]
        [Aliases("tr", "train")]
        public Task TrainedAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_chicken_name)] string name)
            => this.TryBuyInternalAsync(ctx, ChickenType.Trained, name);
        #endregion

        #region chicken buy steroidempowered
        [Command("steroidempowered")]
        [Aliases("s", "steroid", "empowered")]

        public Task EmpoweredAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_chicken_name)] string name)
            => this.TryBuyInternalAsync(ctx, ChickenType.SteroidEmpowered, name);
        #endregion

        #region chicken buy alien
        [Command("alien")]
        [Aliases("a", "extraterrestrial")]
        public Task AlienAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_chicken_name)] string name)
            => this.TryBuyInternalAsync(ctx, ChickenType.Alien, name);
        #endregion

        #region chicken buy list
        [Command("list")]
        [Aliases("ls", "view")]
        public Task ListAsync(CommandContext ctx)
        {
            CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.str_chicken_types);
                emb.WithColor(this.ModuleColor);
                foreach (((ChickenType type, ChickenStats stats), int i) in Chicken.StartingStats.Select((kvp, i) => (kvp, i))) {
                    string title = this.Localization.GetStringUnsafe(ctx.Guild?.Id, $"str-chicken-type-{i}", Chicken.Price(type), gcfg.Currency);
                    emb.AddField(title, stats.ToString());
                }
            });
        }
        #endregion


        #region internals
        private async Task TryBuyInternalAsync(CommandContext ctx, ChickenType type, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_missing_name);

            if (name.Length > Chicken.NameLimit)
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_name(Chicken.NameLimit));

            if (!name.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_name_alnum);

            if (await this.Service.ContainsAsync(ctx.Guild.Id, ctx.User.Id))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_dup);

            CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);

            long price = Chicken.Price(type);
            if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_chicken_buy(ctx.User.Mention, price, gcfg.Currency)))
                return;

            if (!await ctx.Services.GetRequiredService<BankAccountService>().TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, price))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_funds(gcfg.Currency, price));

            await this.Service.AddAsync(new Chicken(type) {
                GuildId = ctx.Guild.Id,
                Name = name,
                UserId = ctx.User.Id
            });

            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, TranslationKey.fmt_chicken_buy(ctx.User.Mention, type.Humanize(LetterCasing.LowerCase), name));
        }
        #endregion
    }
}