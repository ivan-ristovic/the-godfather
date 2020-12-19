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
    [Aliases("cock", "hen", "chick", "coc", "cc")]
    [RequireGuild, Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class ChickenModule : TheGodfatherServiceModule<ChickenService>
    {
        public ChickenModule(ChickenService service)
            : base(service) { }


        #region chicken
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-member")] DiscordMember? member = null)
            => this.InfoAsync(ctx, member);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-chicken-name")] string chickenName)
            => this.InfoAsync(ctx, chickenName);
        #endregion

        #region chicken fight
        [Command("fight"), Priority(1)]
        [Aliases("f", "duel", "attack")]
        public async Task FightAsync(CommandContext ctx,
                                    [Description("desc-member")] DiscordMember member)
        {
            if (ctx.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                throw new CommandFailedException(ctx, "cmd-err-chicken-war");

            if (member == ctx.User)
                throw new CommandFailedException(ctx, "cmd-err-chicken-self");

            Chicken? c1 = await this.Service.GetCompleteAsync(ctx.Guild.Id, ctx.User.Id);
            if (c1 is null)
                throw new CommandFailedException(ctx, "cmd-err-chicken-none");
            c1.Owner = ctx.User;

            if (c1.Stats.TotalVitality < Chicken.MinVitalityToFight)
                throw new CommandFailedException(ctx, "cmd-err-chicken-weak", ctx.User.Mention);

            Chicken? c2 = await this.Service.GetCompleteAsync(ctx.Guild.Id, member.Id);
            if (c2 is null)
                throw new CommandFailedException(ctx, "cmd-err-chicken-404", member.Mention);
            c2.Owner = member;

            if (c1.IsTooStrongFor(c2))
                throw new CommandFailedException(ctx, "cmd-err-chicken-strdiff", Chicken.MaxFightStrDiff);

            var res = ChickenFightResult.Fight(c1, c2);
            await this.Service.UpdateAsync(res);
            await ctx.Services.GetRequiredService<BankAccountService>().IncreaseBankAccountAsync(ctx.Guild.Id, res.Winner.UserId, res.Reward);

            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);

                var desc = new StringBuilder();
                desc.AppendLine(lcs.GetString(ctx.Guild.Id, "fmt-chicken-fight-h",
                    Emojis.Chicken, c1.Name, c1.Stats.ToShortString(), Emojis.DuelSwords, c2.Name, c2.Stats.ToShortString(), Emojis.Chicken
                )).AppendLine();
                desc.AppendLine(lcs.GetString(ctx.Guild.Id, "fmt-chicken-fight-w", Emojis.Trophy, res.Winner.Name)).AppendLine();
                desc.AppendLine(lcs.GetString(ctx.Guild.Id, "fmt-chicken-fight-gain", res.Winner.Name, res.StrGain));
                if (res.IsLoserDead)
                    desc.AppendLine(lcs.GetString(ctx.Guild.Id, "fmt-chicken-fight-d", res.Loser.Name));
                else
                    desc.AppendLine(lcs.GetString(ctx.Guild.Id, "fmt-chicken-fight-loss", res.Loser.Name, ChickenFightResult.VitLoss));
                desc.AppendLine();
                string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
                desc.AppendLine(lcs.GetString(ctx.Guild.Id, "fmt-chicken-fight-rew", res.Winner.Owner?.Mention, res.Reward, currency));
                emb.WithDescription(desc);
            });
        }

        [Command("fight"), Priority(0)]
        public async Task FightAsync(CommandContext ctx,
                                    [Description("desc-chicken-name")] string chickenName)
        {
            Chicken? chicken = this.Service.GetByName(ctx.Guild.Id, chickenName);
            if (chicken is null)
                throw new CommandFailedException(ctx, "cmd-err-chicken-name-404");

            try {
                DiscordMember member = await ctx.Guild.GetMemberAsync(chicken.UserId);
                await this.FightAsync(ctx, member);
            } catch (NotFoundException) {
                await this.Service.RemoveAsync(chicken);
                throw new CommandFailedException(ctx, "cmd-err-chicken-owner");
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
                throw new CommandFailedException(ctx, "cmd-err-chicken-war");

            if (!await this.Service.HealAsync(ctx.Guild.Id, ctx.User.Id, 100))
                throw new CommandFailedException(ctx, "cmd-err-chicken-none");

            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, "fmt-chicken-heal", ctx.User.Mention, 100);
        }
        #endregion

        #region chicken info
        [Command("info"), Priority(1)]
        [Aliases("information", "stats")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("desc-member")] DiscordMember? member = null)
        {
            member ??= ctx.Member;

            Chicken? chicken = await this.Service.GetCompleteAsync(ctx.Guild.Id, ctx.User.Id);
            if (chicken is null)
                throw new CommandFailedException(ctx, "cmd-err-chicken-none");
            chicken.Owner = member;

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle($"{Emojis.Chicken} {chicken.Name}");
                emb.WithColor(this.ModuleColor);

                emb.AddLocalizedTitleField("str-owner", chicken.Owner?.Mention ?? chicken.UserId.ToString(), inline: true);
                emb.AddLocalizedTitleField("str-value", $"{chicken.SellPrice:n0}", inline: true);
                emb.AddLocalizedField("str-stats", chicken.Stats.ToString(), inline: true);
                if (chicken.Stats.Upgrades?.Any() ?? false)
                    emb.AddField("str-upgrades", chicken.Stats.Upgrades.Select(u => u.Upgrade.Name).JoinWith(", "), inline: true);

                emb.WithLocalizedFooter("str-chickens", chicken.Owner?.AvatarUrl);
            });
        }
        
        [Command("info"), Priority(1)]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("desc-chicken-name")] string chickenName)
        {
            Chicken? chicken = this.Service.GetByName(ctx.Guild.Id, chickenName);
            if (chicken is null)
                throw new CommandFailedException(ctx, "cmd-err-chicken-name-404");

            try {
                DiscordMember member = await ctx.Guild.GetMemberAsync(chicken.UserId);
                await this.InfoAsync(ctx, member);
            } catch (NotFoundException) {
                await this.Service.RemoveAsync(chicken);
                throw new CommandFailedException(ctx, "cmd-err-chicken-owner");
            }
        }
        #endregion

        #region chicken rename
        [Command("rename")]
        [Aliases("rn", "name")]
        public async Task RenameAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-name")] string newname)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException(ctx, "cmd-err-missing-name");

            if (newname.Length > Chicken.NameLimit)
                throw new InvalidCommandUsageException(ctx, "cmd-err-name", Chicken.NameLimit);

            if (!newname.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
                throw new InvalidCommandUsageException(ctx, "cmd-err-name-alnum");

            if (!ctx.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                throw new CommandFailedException(ctx, "cmd-err-chicken-war");

            if (!await this.Service.RenameAsync(ctx.Guild.Id, ctx.User.Id, newname))
                throw new CommandFailedException(ctx, "cmd-err-chicken-none");

            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region chicken sell
        [Command("sell"), UsesInteractivity]
        [Aliases("s")]
        public async Task SellAsync(CommandContext ctx)
        {
            if (!ctx.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                throw new CommandFailedException(ctx, "cmd-err-chicken-war");

            Chicken? chicken = await this.Service.GetCompleteAsync(ctx.Guild.Id, ctx.User.Id);
            if (chicken is null)
                throw new CommandFailedException(ctx, "cmd-err-chicken-none");
            chicken.Owner = ctx.User;

            long price = chicken.SellPrice;
            string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            if (!await ctx.WaitForBoolReplyAsync("q-chicken-sell", args: new[] { ctx.User.Mention, $"{price:n0}", currency }))
                return;

            await this.Service.RemoveAsync(chicken);
            await ctx.Services.GetRequiredService<BankAccountService>().IncreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, price);

            await ctx.InfoAsync(this.ModuleColor, Emojis.Chicken, "fmt-chicken-sell", ctx.User.Mention, chicken.Name, price);
        }
        #endregion

        #region chicken list
        [Command("list")]
        [Aliases("print", "show", "ls", "l", "p")]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyList<Chicken> chickens = await this.Service.GetAllAsync(ctx.Guild.Id);
            if (!chickens.Any())
                throw new CommandFailedException(ctx, "cmd-err-chickens-none");

            await Task.WhenAll(chickens.Select(c => this.Service.SetOwnerAsync(c, ctx.Client)));

            await ctx.PaginateAsync(
                "str-chicken-list",
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


        #region Helpers
        private async Task PrintChickensAsync(CommandContext ctx, IEnumerable<Chicken> chickens)
        {
            if (!chickens.Any())
                throw new CommandFailedException(ctx, "cmd-err-chickens-none");

            await Task.WhenAll(chickens.Select(c => this.Service.SetOwnerAsync(c, ctx.Client)));

            await ctx.PaginateAsync(chickens, (emb, c) => {
                emb.WithTitle($"{Emojis.Chicken} {c.Name} {Emojis.Chicken}");
                emb.WithLocalizedDescription("fmt-chicken-owned-by", c.Owner?.Mention ?? "?");
                emb.AddLocalizedTitleField("str-chicken-str", $"{c.BareStrength} ({c.Stats.TotalStrength})", inline: true);
                emb.AddLocalizedTitleField("str-chicken-vit", $"{c.Vitality}/{c.BareMaxVitality} ({c.Stats.TotalMaxVitality})", inline: true);
                emb.AddLocalizedTitleField("str-chicken-stats", c.Upgrades.Select(u => u.Upgrade.Name).JoinWith(", "));
                return emb;
            }, this.ModuleColor);
        }
        #endregion
    }
}
