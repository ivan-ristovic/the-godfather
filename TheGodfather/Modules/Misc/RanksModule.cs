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
    [Aliases("ranks", "ranking", "level")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class RanksModule : TheGodfatherServiceModule<GuildRanksService>
    {
        #region rank
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-member")] DiscordMember? member = null)
            => this.ExecuteGroupAsync(ctx, member as DiscordUser);

        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("desc-user")] DiscordUser? user = null)
        {
            user ??= ctx.User;

            UserRanksService rs = ctx.Services.GetRequiredService<UserRanksService>();
            short rank = rs.CalculateRankForUser(user.Id);
            XpRank? rankInfo = ctx.Guild is { } ? await rs.FindRankAsync(ctx.Guild.Id) : null;

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithTitle(user.ToDiscriminatorString());
                emb.WithThumbnail(user.AvatarUrl);
                emb.AddLocalizedTitleField("str-rank", rank, inline: true);
                emb.AddLocalizedTitleField("str-xp", rs.GetUserXp(user.Id), inline: true);
                emb.AddLocalizedTitleField("str-xp-next", UserRanksService.CalculateXpNeededForRank(++rank), inline: true);
                if (rankInfo is { })
                    emb.AddLocalizedTitleField("str-rank-name", Formatter.Italic(rankInfo.Name), inline: true);
                else
                    emb.AddLocalizedField("str-rank", "str-rank-noname", inline: true);
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
        public async Task TopAsync(CommandContext ctx)
        {
            var emb = new LocalizedEmbedBuilder(this.Localization, ctx.Guild.Id);
            emb.WithLocalizedTitle("str-rank-top");
            emb.WithColor(this.ModuleColor);
            string unknown = this.Localization.GetString(ctx.Guild.Id, "str-404");

            UserRanksService rs = ctx.Services.GetRequiredService<UserRanksService>();
            IReadOnlyList<XpCount> top = await rs.GetTopRankedUsersAsync();

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
                int removed = await rs.RemoveAsync(notFoundUsers);
                LogExt.Debug(ctx, "Removed {Count} not found users from XP count table", removed);
            } catch (Exception e) {
                LogExt.Warning(ctx, e, "Failed to remove not found users from XP count table");
            }
        }
        #endregion
    }
}