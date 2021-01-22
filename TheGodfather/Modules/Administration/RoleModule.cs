using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

namespace TheGodfather.Modules.Administration
{
    [Group("role"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("roles", "rl")]
    [RequireGuild]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class RoleModule : TheGodfatherModule
    {
        #region role
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-role")] DiscordRole role)
            => this.InfoAsync(ctx, role);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);
        #endregion

        #region role create
        [Command("create"), Priority(1), UsesInteractivity]
        [Aliases("new", "add", "a", "+", "+=", "<<", "<", "<-", "<=")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task CreateAsync(CommandContext ctx,
                                     [Description("desc-name")] string name,
                                     [Description("desc-color")] DiscordColor? color = null,
                                     [Description("desc-hoisted")] bool hoisted = false,
                                     [Description("desc-mentionable")] bool mentionable = false,
                                     [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new CommandFailedException(ctx, "cmd-err-missing-name");

            if (ctx.Guild.Roles.Select(kvp => kvp.Value).Any(r => string.Compare(r.Name, name, true) == 0)) {
                if (!await ctx.WaitForBoolReplyAsync("q-role-exists"))
                    return;
            }

            await ctx.Guild.CreateRoleAsync(name, null, color, hoisted, mentionable, ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("create"), Priority(0)]
        public Task CreateAsync(CommandContext ctx,
                               [Description("desc-color")] DiscordColor color,
                               [RemainingText, Description("desc-name")] string name)
            => this.CreateAsync(ctx, name, color, false, false);
        #endregion

        #region role delete
        [Command("delete")]
        [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("desc-role")] DiscordRole role,
                                     [RemainingText, Description("desc-rsn")] string? reason = null)
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
                             [Description("desc-role")] DiscordRole role)
        {
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle(role.Name);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedTitleField("str-pos", role.Position, inline: true);
                emb.AddLocalizedTitleField("str-color", role.Color, inline: true);
                emb.AddLocalizedTitleField("str-id", role.Id, inline: true);
                emb.AddLocalizedTitleField("str-mention", role.IsMentionable, inline: true);
                emb.AddLocalizedTitleField("str-hoist", role.IsHoisted, inline: true);
                emb.AddLocalizedTitleField("str-managed", role.IsManaged, inline: true);
                emb.AddLocalizedTimestampField("str-created-at", role.CreationTimestamp, inline: true);
                emb.AddLocalizedTitleField("str-perms", role.Permissions.ToPermissionString(), inline: true);
            });
        }
        #endregion

        #region role list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public Task ListAsync(CommandContext ctx)
        {
            return ctx.PaginateAsync(
                "str-roles",
                ctx.Guild.Roles.Select(kvp => kvp.Value).OrderByDescending(r => r.Position),
                r => $"{Formatter.InlineCode(r.Id.ToString())} | {r.Mention} [{GetFlags(r)}]",
                this.ModuleColor,
                10
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
        [RequireUserPermissions(Permissions.Administrator), RequireBotPermissions(Permissions.ManageRoles)]
        public async Task MentionAllFromRoleAsync(CommandContext ctx,
                                                 [Description("desc-role")] DiscordRole role)
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
        [Command("setcolor"), Priority(1)]
        [Aliases("clr", "c", "sc", "setc")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetColorAsync(CommandContext ctx,
                                       [Description("desc-role")] DiscordRole role,
                                       [Description("desc-color")] DiscordColor color,
                                       [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            await role.ModifyAsync(color: color, reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("setcolor"), Priority(0)]
        public Task SetColorAsync(CommandContext ctx,
                                 [Description("desc-color")] DiscordColor color,
                                 [Description("desc-role")] DiscordRole role,
                                 [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.SetColorAsync(ctx, role, color, reason);
        #endregion

        #region role setname
        [Command("setname"), Priority(1)]
        [Aliases("name", "rename", "n", "mv")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RenameAsync(CommandContext ctx,
                                     [Description("desc-name")] string newname,
                                     [Description("desc-role")] DiscordRole role,
                                     [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new CommandFailedException(ctx, "cmd-err-missing-name");

            await role.ModifyAsync(name: newname, reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("setname"), Priority(0)]
        public Task RenameAsync(CommandContext ctx,
                               [Description("desc-role")] DiscordRole role,
                               [RemainingText, Description("desc-name")] string newname)
            => this.RenameAsync(ctx, role, newname);
        #endregion

        #region role setmentionable
        [Command("setmentionable"), Priority(1)]
        [Aliases("mentionable", "m", "setm")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetMentionableAsync(CommandContext ctx,
                                             [Description("desc-role")] DiscordRole role,
                                             [Description("desc-mentionable")] bool mentionable = true,
                                             [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            await role.ModifyAsync(mentionable: mentionable, reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("setmentionable"), Priority(0)]
        public Task SetMentionableAsync(CommandContext ctx,
                                       [Description("desc-mentionable")] bool mentionable,
                                       [Description("desc-role")] DiscordRole role,
                                       [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.SetMentionableAsync(ctx, role, mentionable, reason);
        #endregion

        #region role setvisibility
        [Command("setvisibility"), Priority(1)]
        [Aliases("setvisible", "separate", "h", "seth", "hoist", "sethoist")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetVisibleAsync(CommandContext ctx,
                                         [Description("desc-role")] DiscordRole role,
                                         [Description("desc-hoisted")] bool hoisted = true,
                                         [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            await role.ModifyAsync(hoist: hoisted, reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("setvisibility"), Priority(0)]
        public Task SetVisibleAsync(CommandContext ctx,
                                   [Description("desc-hoisted")] bool hoisted,
                                   [Description("desc-role")] DiscordRole role,
                                   [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.SetVisibleAsync(ctx, role, hoisted, reason);
        #endregion
    }
}
