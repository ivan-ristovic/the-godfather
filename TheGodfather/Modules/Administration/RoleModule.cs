using System.Text;
using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Administration;

[Group("role")]
[Alias("roles", "rl")]
[RequireContext(ContextType.Guild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class RoleModule : TheGodfatherModule
{
    public RoleModule(LocalizationService localization)
        : base(localization, new Color(0x3498DB)) { }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(IRole role)
        => this.InfoAsync(role);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("create")]
    [Priority(1)]
    [Alias("new", "add", "a", "+", "+=", "<<", "<", "<-", "<=")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task CreateAsync(string name, string? color = null, bool hoisted = false, bool mentionable = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new LocalizedCommandException(TranslationKey.cmd_err_missing_name);

        Color? parsedColor = ParseColor(color);
        await this.Context.Guild.CreateRoleAsync(
            name,
            GuildPermissions.None,
            parsedColor,
            hoisted,
            mentionable,
            new RequestOptions { AuditLogReason = InvocationReason() }
        );
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("create")]
    [Priority(0)]
    public Task CreateWithColorAsync(string color, [Remainder] string name)
        => this.CreateAsync(name, color);

    [Command("delete")]
    [Alias("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task DeleteAsync(IRole role, [Remainder] string? reason = null)
    {
        await role.DeleteAsync(new RequestOptions { AuditLogReason = InvocationReason(reason) });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("info")]
    [Alias("i")]
    public Task InfoAsync(IRole role)
    {
        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(role.Name);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_pos, role.Position, true);
            emb.AddLocalizedField(TranslationKey.str_color, role.Color, true);
            emb.AddLocalizedField(TranslationKey.str_id, role.Id, true);
            emb.AddLocalizedField(TranslationKey.str_mention, role.IsMentionable, true);
            emb.AddLocalizedField(TranslationKey.str_hoist, role.IsHoisted, true);
            emb.AddLocalizedField(TranslationKey.str_managed, role.IsManaged, true);
            emb.AddLocalizedTimestampField(TranslationKey.str_created_at, role.CreatedAt, true);
            emb.AddLocalizedField(TranslationKey.str_perms, role.Permissions.ToList().Select(p => p.ToString()).JoinWith(", "), false);
        });
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync()
    {
        return this.RespondPagedAsync(
            TranslationKey.str_roles,
            this.Context.Guild.Roles.OrderByDescending(r => r.Position),
            role => $"{InlineCode(role.Id)} | {role.Mention} [{GetFlags(role)}]"
        );

        static string GetFlags(IRole role)
        {
            var sb = new StringBuilder();
            if (role.IsHoisted)
                sb.Append('H');
            if (role.IsManaged)
                sb.Append('I');
            if (role.IsMentionable)
                sb.Append('M');
            return sb.Length > 0 ? sb.ToString() : "/";
        }
    }

    [Command("mention")]
    [Alias("mentionall", "@", "ma")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task MentionAllFromRoleAsync(IRole role)
    {
        if (role is not SocketRole socketRole) {
            await this.ReplyAsync(role.Mention);
            return;
        }

        if (role.IsMentionable) {
            await this.ReplyAsync(role.Mention);
            return;
        }

        await socketRole.ModifyAsync(props => props.Mentionable = true);
        await this.ReplyAsync(role.Mention);
        await socketRole.ModifyAsync(props => props.Mentionable = false);
    }

    [Command("setcolor")]
    [Priority(1)]
    [Alias("clr", "c", "sc", "setc")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task SetColorAsync(IRole role, string color, [Remainder] string? reason = null)
    {
        if (role is not SocketRole socketRole || ParseColor(color) is not Color parsed)
            await this.FailAsync("Invalid role or color.");
        else {
            await socketRole.ModifyAsync(props => props.Color = parsed, new RequestOptions { AuditLogReason = InvocationReason(reason) });
            await this.InfoAsync(this.ModuleColor);
        }
    }

    [Command("setname")]
    [Priority(1)]
    [Alias("name", "rename", "n", "mv")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task RenameAsync(IRole role, [Remainder] string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new LocalizedCommandException(TranslationKey.cmd_err_missing_name);
        if (role is SocketRole socketRole)
            await socketRole.ModifyAsync(props => props.Name = newName, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("setmentionable")]
    [Priority(1)]
    [Alias("mentionable", "m", "setm")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task SetMentionableAsync(IRole role, bool mentionable = true)
    {
        if (role is SocketRole socketRole)
            await socketRole.ModifyAsync(props => props.Mentionable = mentionable, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("setvisibility")]
    [Priority(1)]
    [Alias("setvisible", "separate", "h", "seth", "hoist", "sethoist")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task SetVisibleAsync(IRole role, bool hoisted = true)
    {
        if (role is SocketRole socketRole)
            await socketRole.ModifyAsync(props => props.Hoist = hoisted, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }


    private static Color? ParseColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return null;

        string normalized = color.Trim().TrimStart('#');
        return uint.TryParse(normalized, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value)
            ? new Color(value)
            : null;
    }

    private string InvocationReason(string? reason = null)
        => string.IsNullOrWhiteSpace(reason)
            ? $"Requested by {this.Context.User} ({this.Context.User.Id})"
            : $"{reason} | Requested by {this.Context.User} ({this.Context.User.Id})";
}
