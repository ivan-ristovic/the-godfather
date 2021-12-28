using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.EventListeners.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration;

[Group("levelroles")][Module(ModuleType.Administration)][NotBlocked]
[Aliases("lr", "levelrole", "lvlroles", "levelrl", "lvlrole", "lvlr", "lvlrl", "lrole")]
[RequireGuild][RequireUserPermissions(Permissions.ManageGuild)][RequireBotPermissions(Permissions.ManageRoles)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class LevelRolesModule : TheGodfatherServiceModule<LevelRoleService>
{
    #region levelroles
    [GroupCommand][Priority(2)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_rank)] short rank,
        [Description(TranslationKey.desc_role_grant)] DiscordRole role)
        => this.AddAsync(ctx, rank, role);

    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_role_grant)] DiscordRole role,
        [Description(TranslationKey.desc_rank)] short rank)
        => this.AddAsync(ctx, rank, role);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx)
        => this.ListAsync(ctx);
    #endregion

    #region levelroles add
    [Command("add")][Priority(1)]
    [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_role_grant)] DiscordRole role,
        [Description(TranslationKey.desc_rank)] short rank)
        => this.AddAsync(ctx, rank, role);

    [Command("add")][Priority(0)]
    public async Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_rank)] short rank,
        [Description(TranslationKey.desc_role_grant)] DiscordRole role)
    {
        if (rank < 1 || rank > 1000)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_rank(1, 1000));

        LevelRole? lr = await this.Service.GetAsync(ctx.Guild.Id, rank);
        if (lr is { })
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_lr);

        await this.Service.AddAsync(new LevelRole {
            GuildId = ctx.Guild.Id,
            Rank = rank,
            RoleId = role.Id
        });

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(DiscordEventType.GuildRoleCreated, TranslationKey.evt_lr_change);
            emb.AddLocalizedField(TranslationKey.str_lr_role, role.Mention, true);
            emb.AddLocalizedField(TranslationKey.str_lr_rank, rank, true);
        });
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_lr_add(role.Mention, rank));
    }
    #endregion

    #region levelroles delete
    [Command("delete")][Priority(1)]
    [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task RemoveAsync(CommandContext ctx,
        [Description(TranslationKey.desc_roles_del)] params DiscordRole[] roles)
    {
        if (roles is null || !roles.Any()) {
            await this.RemoveAllAsync(ctx);
            return;
        }

        IReadOnlyList<LevelRole> lrs = await this.Service.GetAllAsync(ctx.Guild.Id);
        int removed = await this.Service.RemoveAsync(lrs.Where(lr => roles.SelectIds().Contains(lr.RoleId)));

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, TranslationKey.evt_lr_change);
            emb.AddLocalizedField(TranslationKey.str_roles_rem, removed);
        });
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_lr_rem(removed));
    }

    [Command("delete")][Priority(1)]
    public async Task RemoveAsync(CommandContext ctx,
        [Description(TranslationKey.desc_ranks)] params short[] ranks)
    {
        if (ranks is null || !ranks.Any()) {
            await this.RemoveAllAsync(ctx);
            return;
        }

        int removed = await this.Service.RemoveAsync(ctx.Guild.Id, ranks);

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, TranslationKey.evt_lr_change);
            emb.AddLocalizedField(TranslationKey.str_roles_rem, removed);
        });
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_lr_rem(removed));
    }
    #endregion

    #region levelroles deleteall
    [Command("deleteall")][UsesInteractivity]
    [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task RemoveAllAsync(CommandContext ctx)
    {
        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_lr_rem_all))
            return;

        await this.Service.ClearAsync(ctx.Guild.Id);
        await ctx.GuildLogAsync(emb => emb.WithLocalizedTitle(TranslationKey.evt_lr_clear).WithColor(this.ModuleColor));
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_lr_clear);
    }
    #endregion

    #region levelroles list
    [Command("list")]
    [Aliases("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync(CommandContext ctx)
    {
        IReadOnlyList<LevelRole> lrs = await this.Service.GetAllAsync(ctx.Guild.Id);
        if (!lrs.Any()) {
            await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.cmd_err_lr_none);
            return;
        }

        var roles = lrs.Select(lr => (LevelRole: lr, Role: ctx.Guild.GetRole(lr.RoleId))).ToList();
        IEnumerable<short> missingRoleRanks = roles.Where(kvp => kvp.Role is null).Select(kvp => kvp.LevelRole.Rank);

        if (missingRoleRanks.Any()) {
            await this.Service.RemoveAsync(ctx.Guild.Id, missingRoleRanks);
            await ctx.GuildLogAsync(
                emb => {
                    emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, TranslationKey.evt_lr_change);
                    emb.AddLocalizedField(TranslationKey.str_roles_rem, missingRoleRanks.JoinWith());
                },
                false
            );
        }

        await ctx.PaginateAsync(
            TranslationKey.str_lr,
            roles.Where(kvp => !missingRoleRanks.Contains(kvp.LevelRole.Rank)).OrderBy(kvp => kvp.LevelRole.Rank),
            kvp => $"{Formatter.InlineCode($"{kvp.LevelRole.Rank:D3}")} | {kvp.Role.Mention}",
            this.ModuleColor
        );
    }
    #endregion
}