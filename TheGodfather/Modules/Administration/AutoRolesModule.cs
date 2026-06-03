using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Administration;

[Group("automaticroles")]
[Alias("autoassignroles", "autoassign", "autoroles", "autorole", "aroles", "arole", "arl", "ar", "aar")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class AutoRolesModule : TheGodfatherModule
{
    private readonly AutoRoleService autoRoles;

    public AutoRolesModule(LocalizationService localization, AutoRoleService autoRoles)
        : base(localization, new Color(0x3498DB))
    {
        this.autoRoles = autoRoles;
    }

    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync(params IRole[] roles)
        => this.AddAsync(roles);

    [Command("add")]
    [Alias("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public async Task AddAsync(params IRole[] roles)
    {
        if (roles is null || !roles.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_missing_roles);

        await this.autoRoles.AddAsync(this.Context.Guild.Id, roles.Select(role => role.Id));
        string roleString = roles.OrderBy(role => role.Name).Select(role => role.Mention).JoinWith();
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_ar_add(roleString));
    }

    [Command("delete")]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task RemoveAsync(params IRole[] roles)
    {
        if (roles is null || !roles.Any()) {
            await this.RemoveAllAsync();
            return;
        }

        await this.autoRoles.RemoveAsync(this.Context.Guild.Id, roles.Select(role => role.Id));
        string roleString = roles.OrderBy(role => role.Name).Select(role => role.Mention).JoinWith();
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_ar_rem(roleString));
    }

    [Command("deleteall")]
    [Alias("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task RemoveAllAsync()
    {
        if (!await this.ConfirmAsync(TranslationKey.q_ar_rem_all))
            return;

        await this.autoRoles.ClearAsync(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor, TranslationKey.evt_ar_clear);
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync()
    {
        IReadOnlyList<ulong> roleIds = this.autoRoles.GetIds(this.Context.Guild.Id);
        if (!roleIds.Any()) {
            await this.ImpInfoAsync(this.ModuleColor, TranslationKey.cmd_err_ar_none);
            return;
        }

        var roles = roleIds.Select(roleId => (RoleId: roleId, Role: this.Context.Guild.GetRole(roleId))).ToList();
        ulong[] missing = roles.Where(pair => pair.Role is null).Select(pair => pair.RoleId).ToArray();
        if (missing.Any())
            await this.autoRoles.RemoveAsync(this.Context.Guild.Id, missing);

        await this.RespondPagedAsync(
            TranslationKey.str_ar,
            roles.Where(pair => pair.Role is not null).Select(pair => pair.Role!).OrderByDescending(role => role.Position),
            role => role.Mention
        );
    }
}
