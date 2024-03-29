﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Chickens.Services;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Chickens;

public partial class ChickenModule
{
    [Group("upgrade")][UsesInteractivity]
    [Aliases("perks", "upgrades", "upg", "u")]
    public sealed class UpgradeModule : TheGodfatherServiceModule<ChickenUpgradeService>
    {
        #region chicken upgrade
        [GroupCommand][Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand][Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_chicken_upgrade_ids)] params int[] ids)
        {
            if (ids is null || !ids.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_upg_ids_none);

            if (ctx.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_war);

            Chicken? chicken = await ctx.Services.GetRequiredService<ChickenService>().GetCompleteAsync(ctx.Guild.Id, ctx.User.Id);
            if (chicken is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_none);
            chicken.Owner = ctx.User;

            if (chicken.Stats.Upgrades?.Any(u => ids.Contains(u.Id)) ?? false)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_upg_dup);

            IReadOnlyList<ChickenUpgrade> upgrades = await this.Service.GetAsync();
            var toBuy = upgrades.Where(u => ids.Contains(u.Id)).ToList();

            CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
            long totalCost = toBuy.Sum(u => u.Cost);
            string upgradeNames = toBuy.Select(u => u.Name).JoinWith(", ");
            if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_chicken_upg(ctx.User.Mention, totalCost, gcfg.Currency, upgradeNames)))
                return;

            if (!await ctx.Services.GetRequiredService<BankAccountService>().TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, totalCost))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_funds(gcfg.Currency, totalCost));

            await ctx.Services.GetRequiredService<ChickenBoughtUpgradeService>().AddAsync(
                toBuy.Select(u => new ChickenBoughtUpgrade {
                    Id = u.Id,
                    GuildId = chicken.GuildId,
                    UserId = chicken.UserId
                })
            );

            int addedStr = toBuy.Where(u => u.UpgradesStat == ChickenStatUpgrade.Str).Sum(u => u.Modifier);
            int addedVit = toBuy.Where(u => u.UpgradesStat == ChickenStatUpgrade.MaxVit).Sum(u => u.Modifier);
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, TranslationKey.fmt_chicken_upg(ctx.User.Mention, chicken.Name, toBuy.Count, addedStr, addedVit));
        }
        #endregion

        #region chicken upgrade list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyList<ChickenUpgrade> upgrades = await this.Service.GetAsync();
            if (!upgrades.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_res_none);

            string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            await ctx.PaginateAsync(upgrades.OrderBy(u => u.Cost), (emb, u) => {
                emb.WithTitle(u.Name);
                emb.AddLocalizedField(TranslationKey.str_id, u.Id, true);
                emb.AddLocalizedField(TranslationKey.str_cost, $"{u.Cost:n0} {currency}", true);
                emb.AddLocalizedField(TranslationKey.str_cost, $"+{u.Modifier}{u.UpgradesStat.Humanize(LetterCasing.AllCaps)}", true);
                return emb;
            }, this.ModuleColor);
        }
        #endregion
    }
}