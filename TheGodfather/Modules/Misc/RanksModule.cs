using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Misc.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Misc
{
    [Group("rank"), Module(ModuleType.Misc), NotBlocked]
    [Aliases("ranks", "ranking", "level", "xp")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class RanksModule : TheGodfatherServiceModule<GuildRanksService>
    {
        #region rank
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-member")] DiscordMember? member = null)
            => this.ExecuteGroupAsync(ctx, member as DiscordUser);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-user")] DiscordUser? user = null)
        {
            user ??= ctx.User;

            UserRanksService rs = ctx.Services.GetRequiredService<UserRanksService>();
            return ctx.RespondWithLocalizedEmbedAsync(async emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithTitle(user.ToDiscriminatorString());
                emb.WithThumbnail(user.AvatarUrl);
                emb.AddLocalizedTitleField("str-xp", rs.GetUserXp(ctx.Guild.Id, user.Id), inline: true);
                if (ctx.Guild is { }) {
                    short rank = rs.CalculateRankForUser(ctx.Guild.Id, user.Id);
                    XpRank? rankInfo = ctx.Guild is { } ? await this.Service.GetAsync(ctx.Guild.Id, rank) : null;
                    emb.AddLocalizedTitleField("str-rank", rank, inline: true);
                    emb.AddLocalizedTitleField("str-xp-next", UserRanksService.CalculateXpNeededForRank((short)(rank + 1)), inline: true);
                    if (rankInfo is { })
                        emb.AddLocalizedTitleField("str-rank-name", Formatter.Italic(rankInfo.Name), inline: true);
                    else
                        emb.AddLocalizedField("str-rank", "str-rank-noname", inline: true);
                }
            });
        }
        #endregion

        #region rank add
        [Command("add"), Priority(0)]
        [Aliases("register", "rename", "mv", "newname", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("str-rank")] short rank,
                                  [RemainingText, Description("str-rank-name")] string name)
        {
            if (rank is < 0 or > 150)
                throw new CommandFailedException(ctx, "cmd-err-rank", 0, 150);

            if (string.IsNullOrWhiteSpace(name))
                throw new CommandFailedException(ctx, "cmd-err-name-404");

            if (name.Length > XpRank.NameLimit)
                throw new CommandFailedException(ctx, "cmd-err-name", XpRank.NameLimit);

            await this.Service.RemoveAsync(ctx.Guild.Id, rank);
            await this.Service.AddAsync(new XpRank {
                GuildId = ctx.Guild.Id,
                Name = name,
                Rank = rank,
            });

            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region rank delete
        [Command("delete")]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("str-rank")] short rank)
        {
            await this.Service.RemoveAsync(ctx.Guild.Id, rank);
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region rank list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public async Task RankListAsync(CommandContext ctx)
        {
            IReadOnlyList<XpRank> ranks = await this.Service.GetAllAsync(ctx.Guild.Id);
            if (!ranks.Any())
                throw new CommandFailedException(ctx, "cmd-err-rank-none");

            await ctx.PaginateAsync(
                "str-ranks",
                ranks.OrderBy(r => r.Rank),
                rank => $"{Formatter.InlineCode($"{rank.Rank:D2}")}" +
                        $" | XP: {Formatter.InlineCode($"{UserRanksService.CalculateXpNeededForRank(rank.Rank):D5}")}" +
                        $" | {Formatter.Bold(rank.Name)}",
                this.ModuleColor
            );
        }
        #endregion

        #region rank top
        [Command("top")]
        public Task TopAsync(CommandContext ctx)
            => this.InternalTopAsync(ctx, global: false);
        #endregion

        #region rank topglobal
        [Command("topglobal")]
        [Aliases("bestglobally", "globallystrongest", "globaltop", "topg", "gtop", "globalbest", "bestglobal")]
        public Task TopGlobalAsync(CommandContext ctx)
            => this.InternalTopAsync(ctx, global: true);
        #endregion


        #region internals
        private async Task InternalTopAsync(CommandContext ctx, bool global)
        {
            var emb = new LocalizedEmbedBuilder(this.Localization, ctx.Guild.Id);
            emb.WithLocalizedTitle(global ? "str-rank-topg" : "str-rank-top");
            emb.WithColor(this.ModuleColor);
            string unknown = this.Localization.GetString(ctx.Guild.Id, "str-404");

            UserRanksService rs = ctx.Services.GetRequiredService<UserRanksService>();
            IReadOnlyList<XpCount> top = await rs.GetTopRankedUsersAsync(global ? null : ctx.Guild?.Id);

            var ranks = new Dictionary<short, string>();
            var notFoundUsers = new List<ulong>();
            foreach (XpCount xpc in top) {
                DiscordUser? user = null;
                try {
                    user = await ctx.Client.GetUserAsync(xpc.UserId);
                } catch (NotFoundException) {
                    notFoundUsers.Add(xpc.UserId);
                }

                short rank = UserRanksService.CalculateRankForXp(xpc.Xp);
                if (ctx.Guild is { } && !ranks.ContainsKey(rank)) {
                    XpRank? gr = await this.Service.GetAsync(ctx.Guild.Id, rank);
                    if (gr is { })
                        ranks.Add(rank, gr.Name);
                }

                if (ctx.Guild is { } && ranks.TryGetValue(rank, out string? name))
                    emb.AddField(user?.Username ?? unknown, $"{name} ({rank}) ({xpc.Xp} XP)");
                else
                    emb.AddField(user?.Username ?? unknown, $"LVL {rank} ({xpc.Xp} XP)");

            }

            await ctx.RespondAsync(embed: emb.Build());

            try {
                await rs.RemoveDeletedUsers(notFoundUsers);
                LogExt.Debug(ctx, "Removed not found users from XP count table");
            } catch (Exception e) {
                LogExt.Warning(ctx, e, "Failed to remove not found users from XP count table");
            }
        }
        #endregion
    }
}