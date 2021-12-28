using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Misc.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Misc;

[Group("rank")][Module(ModuleType.Misc)][NotBlocked]
[Aliases("ranks", "ranking", "level", "xp")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class RanksModule : TheGodfatherServiceModule<GuildRanksService>
{
    #region rank
    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember? member = null)
        => this.ExecuteGroupAsync(ctx, member as DiscordUser);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordUser? user = null)
    {
        user ??= ctx.User;

        UserRanksService rs = ctx.Services.GetRequiredService<UserRanksService>();
        return ctx.RespondWithLocalizedEmbedAsync(async emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithTitle(user.ToDiscriminatorString());
            emb.WithThumbnail(user.AvatarUrl);
            emb.AddLocalizedField(TranslationKey.str_xp, rs.GetUserXp(ctx.Guild.Id, user.Id), true);
            if (ctx.Guild is { }) {
                short rank = rs.CalculateRankForUser(ctx.Guild.Id, user.Id);
                XpRank? rankInfo = ctx.Guild is { } ? await this.Service.GetAsync(ctx.Guild.Id, rank) : null;
                emb.AddLocalizedField(TranslationKey.str_rank, rank, true);
                emb.AddLocalizedField(TranslationKey.str_xp_next, UserRanksService.CalculateXpNeededForRank((short)(rank + 1)), true);
                if (rankInfo is { })
                    emb.AddLocalizedField(TranslationKey.str_rank_name, Formatter.Italic(rankInfo.Name), true);
                else
                    emb.AddLocalizedField(TranslationKey.str_rank, TranslationKey.str_rank_noname, inline: true);
            }
        });
    }
    #endregion

    #region rank add
    [Command("add")][Priority(0)]
    [Aliases("register", "rename", "mv", "newname", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    [RequireUserPermissions(Permissions.ManageGuild)]
    public async Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_rank)] short rank,
        [RemainingText][Description(TranslationKey.desc_rank_name)] string name)
    {
        if (rank is < 0 or > 150)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_rank(0, 150));

        if (string.IsNullOrWhiteSpace(name))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_name_404);

        if (name.Length > XpRank.NameLimit)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_name(XpRank.NameLimit));

        await this.Service.RemoveAsync(ctx.Guild.Id, rank);
        await this.Service.AddAsync(new XpRank {
            GuildId = ctx.Guild.Id,
            Name = name,
            Rank = rank
        });

        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region rank delete
    [Command("delete")]
    [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    [RequireUserPermissions(Permissions.ManageGuild)]
    public async Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_rank)] short rank)
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
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_rank_none);

        await ctx.PaginateAsync(
            TranslationKey.str_ranks,
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
        => this.InternalTopAsync(ctx, false);
    #endregion

    #region rank topglobal
    [Command("topglobal")]
    [Aliases("bestglobally", "globallystrongest", "globaltop", "topg", "gtop", "globalbest", "bestglobal")]
    public Task TopGlobalAsync(CommandContext ctx)
        => this.InternalTopAsync(ctx, true);
    #endregion


    #region internals
    private async Task InternalTopAsync(CommandContext ctx, bool global)
    {
        var emb = new LocalizedEmbedBuilder(this.Localization, ctx.Guild.Id);
        emb.WithLocalizedTitle(global ? TranslationKey.str_rank_topg : TranslationKey.str_rank_top);
        emb.WithColor(this.ModuleColor);
        string unknown = this.Localization.GetString(ctx.Guild.Id, TranslationKey.str_404);

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
                emb.AddField(user?.ToDiscriminatorString() ?? unknown, $"{name} ({rank}) ({xpc.Xp} XP)");
            else
                emb.AddField(user?.ToDiscriminatorString() ?? unknown, $"LVL {rank} ({xpc.Xp} XP)");

        }

        await ctx.RespondAsync(emb.Build());

        try {
            await rs.RemoveDeletedUsers(notFoundUsers);
            LogExt.Debug(ctx, "Removed not found users from XP count table");
        } catch (Exception e) {
            LogExt.Warning(ctx, e, "Failed to remove not found users from XP count table");
        }
    }
    #endregion
}