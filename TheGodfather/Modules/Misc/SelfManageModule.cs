using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Misc;

[Group("grant")]
[Alias("give")]
[RequireContext(ContextType.Guild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class GrantModule : TheGodfatherModule
{
    private readonly SelfRoleService selfRoles;
    private readonly ForbiddenNamesService forbiddenNames;

    public GrantModule(LocalizationService localization, SelfRoleService selfRoles, ForbiddenNamesService forbiddenNames)
        : base(localization, new Color(0x95A5A6))
    {
        this.selfRoles = selfRoles;
        this.forbiddenNames = forbiddenNames;
    }

    [Command("")]
    public Task ExecuteGroupAsync(params IRole[] roles)
        => this.GiveRoleAsync(roles);

    [Command("role")]
    [Alias("roles", "rl", "r")]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task GiveRoleAsync(params IRole[] roles)
    {
        if (this.Context.User is not IGuildUser member)
            return;

        var failed = new List<IRole>();
        foreach (IRole role in roles.DistinctBy(r => r.Id))
            if (await this.selfRoles.ContainsAsync(this.Context.Guild.Id, role.Id))
                await member.AddRoleAsync(role, new RequestOptions { AuditLogReason = "_gf: Self-granted" });
            else
                failed.Add(role);

        if (failed.Any())
            await this.ImpInfoAsync(this.ModuleColor, TranslationKey.cmd_err_grant_fail(failed.Select(r => r.Mention).JoinWith()));
        else
            await this.InfoAsync(this.ModuleColor);
    }

    [Command("nickname")]
    [Alias("nick", "name", "n")]
    [RequireBotPermission(GuildPermission.ManageNicknames)]
    public async Task GiveNameAsync([Remainder] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new LocalizedCommandException(TranslationKey.cmd_err_missing_name);
        if (this.forbiddenNames.IsNameForbidden(this.Context.Guild.Id, name, out _))
            throw new LocalizedCommandException(TranslationKey.cmd_err_fn_match);

        if (this.Context.User is IGuildUser member)
            await member.ModifyAsync(props => props.Nickname = name, new RequestOptions {
                AuditLogReason = this.Localization.GetString(this.Context.Guild.Id, TranslationKey.rsn_grant_name)
            });
        await this.InfoAsync(this.ModuleColor);
    }
}

[Group("revoke")]
[Alias("take")]
[RequireContext(ContextType.Guild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class RevokeModule : TheGodfatherModule
{
    private readonly SelfRoleService selfRoles;

    public RevokeModule(LocalizationService localization, SelfRoleService selfRoles)
        : base(localization, new Color(0x95A5A6))
    {
        this.selfRoles = selfRoles;
    }

    [Command("")]
    public Task ExecuteGroupAsync(params IRole[] roles)
        => this.RevokeRoleAsync(roles);

    [Command("role")]
    [Alias("roles", "rl", "r")]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task RevokeRoleAsync(params IRole[] roles)
    {
        if (this.Context.User is not IGuildUser member)
            return;

        var failed = new List<IRole>();
        foreach (IRole role in roles.DistinctBy(r => r.Id))
            if (await this.selfRoles.ContainsAsync(this.Context.Guild.Id, role.Id))
                await member.RemoveRoleAsync(role, new RequestOptions { AuditLogReason = "_gf: Self-revoked" });
            else
                failed.Add(role);

        if (failed.Any())
            await this.ImpInfoAsync(this.ModuleColor, TranslationKey.cmd_err_revoke_fail(failed.Select(r => r.Mention).JoinWith()));
        else
            await this.InfoAsync(this.ModuleColor);
    }
}
