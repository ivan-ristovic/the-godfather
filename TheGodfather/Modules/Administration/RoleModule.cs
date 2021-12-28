using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace TheGodfather.Modules.Administration;

[Group("role")][Module(ModuleType.Administration)][NotBlocked]
[Aliases("roles", "rl")]
[RequireGuild]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class RoleModule : TheGodfatherModule
{
    #region role
    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_role)] DiscordRole role)
        => this.InfoAsync(ctx, role);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx)
        => this.ListAsync(ctx);
    #endregion

    #region role create
    [Command("create")][Priority(1)][UsesInteractivity]
    [Aliases("new", "add", "a", "+", "+=", "<<", "<", "<-", "<=")]
    [RequirePermissions(Permissions.ManageRoles)]
    public async Task CreateAsync(CommandContext ctx,
        [Description(TranslationKey.desc_name)] string name,
        [Description(TranslationKey.desc_color)] DiscordColor? color = null,
        [Description(TranslationKey.desc_hoisted)] bool hoisted = false,
        [Description(TranslationKey.desc_mentionable)] bool mentionable = false,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_missing_name);

        if (ctx.Guild.Roles.Select(kvp => kvp.Value).Any(r => string.Compare(r.Name, name, true) == 0))
            if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_role_exists))
                return;

        await ctx.Guild.CreateRoleAsync(name, null, color, hoisted, mentionable, ctx.BuildInvocationDetailsString(reason));
        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("create")][Priority(0)]
    public Task CreateAsync(CommandContext ctx,
        [Description(TranslationKey.desc_color)] DiscordColor color,
        [RemainingText][Description(TranslationKey.desc_name)] string name)
        => this.CreateAsync(ctx, name, color);
    #endregion

    #region role delete
    [Command("delete")]
    [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
    [RequirePermissions(Permissions.ManageRoles)]
    public async Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_role)] DiscordRole role,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        await role.DeleteAsync(ctx.BuildInvocationDetailsString(reason));
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region role info
    [Command("info")]
    [Aliases("i")]
    [RequirePermissions(Permissions.ManageRoles)]
    public Task InfoAsync(CommandContext ctx,
        [Description(TranslationKey.desc_role)] DiscordRole role)
    {
        return ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(role.Name);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_pos, role.Position, true);
            emb.AddLocalizedField(TranslationKey.str_color, role.Color, true);
            emb.AddLocalizedField(TranslationKey.str_id, role.Id, true);
            emb.AddLocalizedField(TranslationKey.str_mention, role.IsMentionable, true);
            emb.AddLocalizedField(TranslationKey.str_hoist, role.IsHoisted, true);
            emb.AddLocalizedField(TranslationKey.str_managed, role.IsManaged, true);
            emb.AddLocalizedTimestampField(TranslationKey.str_created_at, role.CreationTimestamp, true);
            emb.AddLocalizedField(TranslationKey.str_perms, role.Permissions.ToPermissionString(), true);
        });
    }
    #endregion

    #region role list
    [Command("list")]
    [Aliases("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync(CommandContext ctx)
    {
        return ctx.PaginateAsync(
            TranslationKey.str_roles,
            ctx.Guild.Roles.Select(kvp => kvp.Value).OrderByDescending(r => r.Position),
            r => $"{Formatter.InlineCode(r.Id.ToString())} | {r.Mention} [{GetFlags(r)}]",
            this.ModuleColor
        );

        static string GetFlags(DiscordRole role)
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

    #endregion

    #region role mention
    [Command("mention")]
    [Aliases("mentionall", "@", "ma")]
    [RequireUserPermissions(Permissions.Administrator)][RequireBotPermissions(Permissions.ManageRoles)]
    public async Task MentionAllFromRoleAsync(CommandContext ctx,
        [Description(TranslationKey.desc_role)] DiscordRole role)
    {
        if (role.IsMentionable) {
            await ctx.RespondAsync(role.Mention);
        } else {
            await role.ModifyAsync(mentionable: true);
            await ctx.RespondAsync(role.Mention);
            await role.ModifyAsync(mentionable: false);
        }
    }
    #endregion

    #region role setcolor
    [Command("setcolor")][Priority(1)]
    [Aliases("clr", "c", "sc", "setc")]
    [RequirePermissions(Permissions.ManageRoles)]
    public async Task SetColorAsync(CommandContext ctx,
        [Description(TranslationKey.desc_role)] DiscordRole role,
        [Description(TranslationKey.desc_color)] DiscordColor color,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        await role.ModifyAsync(color: color, reason: ctx.BuildInvocationDetailsString(reason));
        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("setcolor")][Priority(0)]
    public Task SetColorAsync(CommandContext ctx,
        [Description(TranslationKey.desc_color)] DiscordColor color,
        [Description(TranslationKey.desc_role)] DiscordRole role,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.SetColorAsync(ctx, role, color, reason);
    #endregion

    #region role setname
    [Command("setname")][Priority(1)]
    [Aliases("name", "rename", "n", "mv")]
    [RequirePermissions(Permissions.ManageRoles)]
    public async Task RenameAsync(CommandContext ctx,
        [Description(TranslationKey.desc_name_new)] string newname,
        [Description(TranslationKey.desc_role)] DiscordRole role,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(newname))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_missing_name);

        await role.ModifyAsync(newname, reason: ctx.BuildInvocationDetailsString(reason));
        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("setname")][Priority(0)]
    public Task RenameAsync(CommandContext ctx,
        [Description(TranslationKey.desc_role)] DiscordRole role,
        [RemainingText][Description(TranslationKey.desc_name_new)] string newname)
        => this.RenameAsync(ctx, role, newname);
    #endregion

    #region role setmentionable
    [Command("setmentionable")][Priority(1)]
    [Aliases("mentionable", "m", "setm")]
    [RequirePermissions(Permissions.ManageRoles)]
    public async Task SetMentionableAsync(CommandContext ctx,
        [Description(TranslationKey.desc_role)] DiscordRole role,
        [Description(TranslationKey.desc_mentionable)] bool mentionable = true,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        await role.ModifyAsync(mentionable: mentionable, reason: ctx.BuildInvocationDetailsString(reason));
        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("setmentionable")][Priority(0)]
    public Task SetMentionableAsync(CommandContext ctx,
        [Description(TranslationKey.desc_mentionable)] bool mentionable,
        [Description(TranslationKey.desc_role)] DiscordRole role,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.SetMentionableAsync(ctx, role, mentionable, reason);
    #endregion

    #region role setvisibility
    [Command("setvisibility")][Priority(1)]
    [Aliases("setvisible", "separate", "h", "seth", "hoist", "sethoist")]
    [RequirePermissions(Permissions.ManageRoles)]
    public async Task SetVisibleAsync(CommandContext ctx,
        [Description(TranslationKey.desc_role)] DiscordRole role,
        [Description(TranslationKey.desc_hoisted)] bool hoisted = true,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        await role.ModifyAsync(hoist: hoisted, reason: ctx.BuildInvocationDetailsString(reason));
        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("setvisibility")][Priority(0)]
    public Task SetVisibleAsync(CommandContext ctx,
        [Description(TranslationKey.desc_hoisted)] bool hoisted,
        [Description(TranslationKey.desc_role)] DiscordRole role,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.SetVisibleAsync(ctx, role, hoisted, reason);
    #endregion
}