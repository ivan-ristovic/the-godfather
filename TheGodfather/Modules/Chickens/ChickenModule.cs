using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Chickens.Extensions;
using TheGodfather.Modules.Chickens.Services;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Chickens
{
    [Group("chicken"), Module(ModuleType.Chickens), NotBlocked]
    [Aliases("chickens", "cock", "hen", "chick", "coc", "cc")]
    [RequireGuild, Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class ChickenModule : TheGodfatherServiceModule<ChickenService>
    {
        #region chicken
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description(TranslationKey.desc_member)] DiscordMember? member = null)
            => this.InfoAsync(ctx, member);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description(TranslationKey.desc_chicken_name)] string chickenName)
            => this.InfoAsync(ctx, chickenName);
        #endregion

        #region chicken fight
        [Command("fight"), Priority(1)]
        [Aliases("f", "duel", "attack")]
        public async Task FightAsync(CommandContext ctx,
                                    [Description(TranslationKey.desc_member)] DiscordMember member)
        {
            if (ctx.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_war);

            if (member == ctx.User)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_self);

            Chicken? c1 = await this.Service.GetCompleteAsync(ctx.Guild.Id, ctx.User.Id);
            if (c1 is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_none);
            c1.Owner = ctx.User;

            if (c1.Stats.TotalVitality < Chicken.MinVitalityToFight)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_weak(ctx.User.Mention));

            Chicken? c2 = await this.Service.GetCompleteAsync(ctx.Guild.Id, member.Id);
            if (c2 is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_404(member.Mention));
            c2.Owner = member;

            if (c1.IsTooStrongFor(c2))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_strdiff(Chicken.MaxFightStrDiff));

            var res = ChickenFightResult.Fight(c1, c2);
            await this.Service.UpdateAsync(res);
            await ctx.Services.GetRequiredService<BankAccountService>().IncreaseBankAccountAsync(ctx.Guild.Id, res.Winner.UserId, res.Reward);

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);

                var desc = new StringBuilder();
                desc.AppendLine(this.Localization.GetString(
                    ctx.Guild.Id, 
                    TranslationKey.fmt_chicken_fight_h(
                        Emojis.Chicken, c1.Name, c1.Stats.ToShortString(), Emojis.DuelSwords, c2.Name, c2.Stats.ToShortString(), Emojis.Chicken
                    )
                )).AppendLine();
                desc.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.fmt_chicken_fight_w(Emojis.Trophy, res.Winner.Name))).AppendLine();
                desc.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.fmt_chicken_fight_gain(res.Winner.Name, res.StrGain)));
                if (res.IsLoserDead)
                    desc.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.fmt_chicken_fight_d(res.Loser.Name)));
                else
                    desc.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.fmt_chicken_fight_loss(res.Loser.Name, ChickenFightResult.VitLoss)));
                desc.AppendLine();
                string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
                desc.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.fmt_chicken_fight_rew(res.Winner.Owner?.Mention, res.Reward, currency)));
                emb.WithDescription(desc);
            });
        }

        [Command("fight"), Priority(0)]
        public async Task FightAsync(CommandContext ctx,
                                    [Description(TranslationKey.desc_chicken_name)] string chickenName)
        {
            Chicken? chicken = this.Service.GetByName(ctx.Guild.Id, chickenName);
            if (chicken is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_name_404);

            try {
                DiscordMember member = await ctx.Guild.GetMemberAsync(chicken.UserId);
                await this.FightAsync(ctx, member);
            } catch (NotFoundException) {
                await this.Service.RemoveAsync(chicken);
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_owner);
            }
        }
        #endregion

        #region chicken heal
        [Command("heal")]
        [Aliases("+hp", "hp")]
        [Cooldown(1, 300, CooldownBucketType.Guild)]
        public async Task HealAsync(CommandContext ctx)
        {
            if (!ctx.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_war);

            if (!await this.Service.HealAsync(ctx.Guild.Id, ctx.User.Id, 100))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_none);

            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, TranslationKey.fmt_chicken_heal(ctx.User.Mention, 100));
        }
        #endregion

        #region chicken info
        [Command("info"), Priority(1)]
        [Aliases("information", "stats")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description(TranslationKey.desc_member)] DiscordMember? member = null)
        {
            member ??= ctx.Member;

            Chicken? chicken = await this.Service.GetCompleteAsync(ctx.Guild.Id, ctx.User.Id);
            if (chicken is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_none);
            chicken.Owner = member;

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle($"{Emojis.Chicken} {chicken.Name}");
                emb.WithColor(this.ModuleColor);

                emb.AddLocalizedField(TranslationKey.str_owner, chicken.Owner?.Mention ?? chicken.UserId.ToString(), inline: true);
                emb.AddLocalizedField(TranslationKey.str_value, $"{chicken.SellPrice:n0}", inline: true);
                emb.AddLocalizedField(TranslationKey.str_stats, chicken.Stats.ToString(), inline: true);
                if (chicken.Stats.Upgrades?.Any() ?? false)
                    emb.AddLocalizedField(TranslationKey.str_upgrades, chicken.Stats.Upgrades.Select(u => u.Upgrade.Name).JoinWith(", "), inline: true);

                emb.WithLocalizedFooter(TranslationKey.str_chickens, chicken.Owner?.AvatarUrl);
            });
        }

        [Command("info"), Priority(1)]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description(TranslationKey.desc_chicken_name)] string chickenName)
        {
            Chicken? chicken = this.Service.GetByName(ctx.Guild.Id, chickenName);
            if (chicken is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_name_404);

            try {
                DiscordMember member = await ctx.Guild.GetMemberAsync(chicken.UserId);
                await this.InfoAsync(ctx, member);
            } catch (NotFoundException) {
                await this.Service.RemoveAsync(chicken);
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_owner);
            }
        }
        #endregion

        #region chicken rename
        [Command("rename")]
        [Aliases("rn", "name")]
        public async Task RenameAsync(CommandContext ctx,
                                     [RemainingText, Description(TranslationKey.desc_name_new)] string newname)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_missing_name);

            if (newname.Length > Chicken.NameLimit)
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_name(Chicken.NameLimit));

            if (!newname.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_name_alnum);

            if (!ctx.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_war);

            if (!await this.Service.RenameAsync(ctx.Guild.Id, ctx.User.Id, newname))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_none);

            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region chicken sell
        [Command("sell"), UsesInteractivity]
        [Aliases("s")]
        public async Task SellAsync(CommandContext ctx)
        {
            if (!ctx.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_war);

            Chicken? chicken = await this.Service.GetCompleteAsync(ctx.Guild.Id, ctx.User.Id);
            if (chicken is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_none);
            chicken.Owner = ctx.User;

            long price = chicken.SellPrice;
            string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_chicken_sell(ctx.User.Mention, price, currency)))
                return;

            await this.Service.RemoveAsync(chicken);
            await ctx.Services.GetRequiredService<BankAccountService>().IncreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, price);

            await ctx.InfoAsync(this.ModuleColor, Emojis.Chicken, TranslationKey.fmt_chicken_sell(ctx.User.Mention, chicken.Name, price, currency));
        }
        #endregion

        #region chicken list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyList<Chicken> chickens = await this.Service.GetAllAsync(ctx.Guild.Id);
            if (!chickens.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chickens_none);

            await Task.WhenAll(chickens.Select(c => this.Service.SetOwnerAsync(c, ctx.Client)));

            await ctx.PaginateAsync(
                TranslationKey.str_chicken_list,
                chickens,
                c => $"{Formatter.Bold(c.Name)} | {c.Owner?.Mention ?? "?"} | {c.Stats.TotalStrength} ({c.Stats.BareStrength}) STR",
                this.ModuleColor
            );
        }
        #endregion

        #region chicken top
        [Command("top")]
        [Aliases("best", "strongest")]
        public async Task TopAsync(CommandContext ctx)
        {
            IReadOnlyList<Chicken> top = await this.Service.GetTopAsync(ctx.Guild.Id);
            await this.PrintChickensAsync(ctx, top);
        }
        #endregion

        #region chicken topglobal
        [Command("topglobal")]
        [Aliases("bestglobally", "globallystrongest", "globaltop", "topg", "gtop", "globalbest", "bestglobal")]
        public async Task GlobalTopAsync(CommandContext ctx)
        {
            IReadOnlyList<Chicken> top = await this.Service.GetGlobalTopAsync();
            await this.PrintChickensAsync(ctx, top);
        }
        #endregion


        #region internals
        private async Task PrintChickensAsync(CommandContext ctx, IEnumerable<Chicken> chickens)
        {
            if (!chickens.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chickens_none);

            await Task.WhenAll(chickens.Select(c => this.Service.SetOwnerAsync(c, ctx.Client)));

            await ctx.PaginateAsync(chickens, (emb, c) => {
                emb.WithTitle($"{Emojis.Chicken} {c.Name} {Emojis.Chicken}");
                emb.WithLocalizedDescription(TranslationKey.fmt_chicken_owned_by(c.Owner?.Mention));
                emb.AddLocalizedField(TranslationKey.str_chicken_str, $"{c.BareStrength} ({c.Stats.TotalStrength})", inline: true);
                emb.AddLocalizedField(TranslationKey.str_chicken_vit, $"{c.Vitality}/{c.BareMaxVitality} ({c.Stats.TotalMaxVitality})", inline: true);
                emb.AddLocalizedField(TranslationKey.str_chicken_upg, c.Upgrades.Select(u => u.Upgrade.Name).JoinWith(", "), unknown: false);
                return emb;
            }, this.ModuleColor);
        }
        #endregion
    }
}
