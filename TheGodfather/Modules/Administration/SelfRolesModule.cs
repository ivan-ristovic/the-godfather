using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.EventListeners.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration;

[Group("selfassignableroles")][Module(ModuleType.Administration)][NotBlocked]
[Aliases("sar", "selfassignablerole", "selfroles", "selfrole", "sr", "srl", "srole")]
[RequireGuild][RequireUserPermissions(Permissions.ManageGuild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class SelfRolesModule : TheGodfatherServiceModule<SelfRoleService>
{
    #region selfassignableroles
    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx)
        => this.ListAsync(ctx);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_roles_add)] params DiscordRole[] roles)
        => this.AddAsync(ctx, roles);
    #endregion

    #region selfassignableroles add
    [Command("add")]
    [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public async Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_roles_add)] params DiscordRole[] roles)
    {
        if (roles is null || !roles.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_missing_roles);

        await this.Service.AddAsync(ctx.Guild.Id, roles.Select(r => r.Id));

        string roleStr = roles.OrderBy(r => r.Name).JoinWith();
        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(DiscordEventType.GuildRoleCreated, TranslationKey.evt_sr_change);
            emb.AddLocalizedField(TranslationKey.str_roles_add, roleStr);
        });
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_sr_add(roleStr));
    }
    #endregion

    #region selfassignableroles delete
    [Command("delete")]
    [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task RemoveAsync(CommandContext ctx,
        [Description(TranslationKey.desc_roles_del)] params DiscordRole[] roles)
    {
        if (roles is null || !roles.Any()) {
            await this.RemoveAllAsync(ctx);
            return;
        }

        await this.Service.RemoveAsync(ctx.Guild.Id, roles.Select(r => r.Id));

        string roleStr = roles.OrderBy(r => r.Name).JoinWith();
        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, TranslationKey.evt_sr_change);
            emb.AddLocalizedField(TranslationKey.str_roles_rem, roleStr);
        });
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_sr_rem(roleStr));
    }
    #endregion

    #region selfassignableroles deleteall
    [Command("deleteall")][UsesInteractivity]
    [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task RemoveAllAsync(CommandContext ctx)
    {
        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_sr_rem_all))
            return;

        await this.Service.ClearAsync(ctx.Guild.Id);
        await ctx.GuildLogAsync(emb => emb.WithLocalizedTitle(TranslationKey.evt_sr_clear).WithColor(this.ModuleColor));
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_sr_clear);
    }
    #endregion

    #region selfassignableroles list
    [Command("list")]
    [Aliases("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync(CommandContext ctx)
    {
        IReadOnlyList<ulong> rids = this.Service.GetIds(ctx.Guild.Id);
        if (!rids.Any()) {
            await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.cmd_err_sr_none);
            return;
        }

        var roles = rids.Select(rid => (rid, ctx.Guild.GetRole(rid))).ToList();
        IEnumerable<ulong> missingRoles = roles.Where(kvp => kvp.Item2 is null).Select(kvp => kvp.rid);

        if (missingRoles.Any()) {
            await this.Service.RemoveAsync(ctx.Guild.Id, missingRoles);
            await ctx.GuildLogAsync(
                emb => {
                    emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, TranslationKey.evt_sr_change);
                    emb.AddLocalizedField(TranslationKey.str_roles_rem, missingRoles.JoinWith());
                },
                false
            );
        }
        await ctx.PaginateAsync(
            TranslationKey.str_sr,
            roles.Where(kvp => !missingRoles.Contains(kvp.rid)).Select(kvp => kvp.Item2).OrderByDescending(r => r.Position),
            r => r.ToString(),
            this.ModuleColor
        );
    }
    #endregion
}