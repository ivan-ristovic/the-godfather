#region USING_DIRECTIVES
using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("roles")]
    [Description("Miscellaneous role control commands.")]
    [Aliases("role", "r", "rl")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    [ListeningCheck]
    public class RoleAdminModule : TheGodfatherBaseModule
    {

        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            await InteractivityUtil.SendPaginatedCollectionAsync(
                ctx,
                "Roles in this guild:",
                ctx.Guild.Roles.OrderByDescending(r => r.Position),
                r => $"{Formatter.Bold(r.Name)} | {r.Color.ToString()} | ID: {Formatter.InlineCode(r.Id.ToString())}",
                DiscordColor.Gold,
                10
            ).ConfigureAwait(false);
        }


        #region COMMAND_ROLES_CREATE
        [Command("create"), Priority(2)]
        [Description("Create a new role.")]
        [Aliases("new", "add", "+", "c")]
        [UsageExample("!roles create \"My role\" #C77B0F no no")]
        [UsageExample("!roles create ")]
        [UsageExample("!roles create #C77B0F My new role")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task CreateRoleAsync(CommandContext ctx,
                                         [Description("Name.")] string name,
                                         [Description("Color.")] DiscordColor? color = null,
                                         [Description("Hoisted (visible in online list)?")] bool hoisted = false,
                                         [Description("Mentionable?")] bool mentionable = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing role name.");

            if (ctx.Guild.Roles.Any(r => string.Compare(r.Name, name, true) == 0)) 
                if (!await AskYesNoQuestionAsync(ctx, "A role with that name already exists. Continue?").ConfigureAwait(false))
                    return;

            await ctx.Guild.CreateRoleAsync(name, null, color, hoisted, mentionable, GetReasonString(ctx))
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx, $"Successfully created role {Formatter.Bold(name)}!")
                .ConfigureAwait(false);
        }

        [Command("create"), Priority(1)]
        public async Task CreateRoleAsync(CommandContext ctx,
                                         [Description("Color.")] DiscordColor color,
                                         [RemainingText, Description("Name.")] string name)
            => await CreateRoleAsync(ctx, name, color, false, false).ConfigureAwait(false);

        [Command("create"), Priority(0)]
        public async Task CreateRoleAsync(CommandContext ctx,
                                         [RemainingText, Description("Name.")] string name)
            => await CreateRoleAsync(ctx, name, null, false, false).ConfigureAwait(false);
        #endregion

        #region COMMAND_ROLES_DELETE
        [Command("delete")]
        [Description("Create a new role.")]
        [Aliases("del", "remove", "d", "-", "rm")]
        [UsageExample("!role delete My role")]
        [UsageExample("!role delete @admins")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task DeleteRoleAsync(CommandContext ctx,
                                         [Description("Role.")] DiscordRole role,
                                         [RemainingText, Description("Reason.")] string reason = null)
        {
            string name = role.Name;
            await role.DeleteAsync(GetReasonString(ctx, reason))
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx, $"Successfully removed role {Formatter.Bold(name)}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ROLES_INFO
        [Command("info")]
        [Description("Get information about a given role.")]
        [Aliases("i")]
        [UsageExample("!role info Admins")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RoleInfoAsync(CommandContext ctx,
                                       [Description("Role.")] DiscordRole role)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = $"Information about role {role.Name}:",
                Color = DiscordColor.Orange
            };
            emb.AddField("Position", role.Position.ToString(), true)
               .AddField("Color", role.Color.ToString(), true)
               .AddField("Id", role.Id.ToString(), true)
               .AddField("Mentionable", role.IsMentionable.ToString(), true)
               .AddField("Visible", role.IsHoisted.ToString(), true)
               .AddField("Managed", role.IsManaged.ToString(), true)
               .AddField("Created at", role.CreationTimestamp.ToString(), true)
               .AddField("Permissions:", role.Permissions.ToPermissionString(), false);

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ROLES_MENTIONALL
        [Command("mentionall")]
        [Description("Mention all users from given role.")]
        [Aliases("mention", "@", "ma")]
        [UsageExample("!role mentionall Admins")]
        [RequirePermissions(Permissions.MentionEveryone)]
        public async Task MentionAllFromRoleAsync(CommandContext ctx, 
                                                 [Description("Role.")] DiscordRole role)
        {
            if (role.IsMentionable) {
                await ctx.RespondAsync(role.Mention)
                    .ConfigureAwait(false);
                return;
            }

            if (!ctx.Channel.PermissionsFor(ctx.Member).HasPermission(Permissions.Administrator))
                throw new CommandFailedException("Only administrator can mention the non-mentionable roles.");

            var users = await ctx.Guild.GetAllMembersAsync()
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var user in users.Where(u => u.Roles.Contains(role)))
                sb.Append(user.Mention).Append(" ");

            string warning = "";
            if (sb.Length >= 2000)
                warning = " (failed to mention all role members)";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Mentioning everyone in role {role.Name}:" + warning,
                Description = sb.ToString(0, 2000)
            }).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ROLES_SETCOLOR
        [Command("setcolor"), Priority(1)]
        [Description("Set a color for the role.")]
        [Aliases("clr", "c", "sc", "setc")]
        [UsageExample("!role setcolor #FF0000 Admins")]
        [UsageExample("!role setcolor Admins #FF0000")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetRoleColorAsync(CommandContext ctx, 
                                           [Description("Role.")] DiscordRole role,
                                           [Description("Color.")] DiscordColor color)
        {
            await role.UpdateAsync(color: color, reason: GetReasonString(ctx))
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx, $"Successfully changed color for {Formatter.Bold(role.Name)}!")
                .ConfigureAwait(false);
        }

        [Command("setcolor"), Priority(0)]
        public async Task SetRoleColorAsync(CommandContext ctx,
                                           [Description("Color.")] DiscordColor color,
                                           [Description("Role.")] DiscordRole role)
            => await SetRoleColorAsync(ctx, role, color).ConfigureAwait(false);
        #endregion

        #region COMMAND_ROLES_SETNAME
        [Command("setname"), Priority(1)]
        [Description("Set a name for the role.")]
        [Aliases("name", "rename", "n")]
        [UsageExample("!role setname @Admins Administrators")]
        [UsageExample("!role setname Administrators @Admins")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RenameRoleAsync(CommandContext ctx,
                                         [Description("Role.")] DiscordRole role,
                                         [RemainingText, Description("New name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("I need a new name for the role.");

            await role.UpdateAsync(name: name, reason: GetReasonString(ctx))
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx, $"Successfully changed role name to {Formatter.Bold(name)}.")
                .ConfigureAwait(false);
        }

        [Command("setname"), Priority(0)]
        public async Task RenameRoleAsync(CommandContext ctx,
                                         [Description("New name.")] string name,
                                         [Description("Role.")] DiscordRole role)
            => await RenameRoleAsync(ctx, role, name).ConfigureAwait(false);
        #endregion

        #region COMMAND_ROLES_SETMENTIONABLE
        [Command("setmentionable"), Priority(1)]
        [Description("Set role mentionable var.")]
        [Aliases("mentionable", "m", "setm")]
        [UsageExample("!role setmentionable Admins")]
        [UsageExample("!role setmentionable Admins false")]
        [UsageExample("!role setmentionable false Admins")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetRoleMentionableAsync(CommandContext ctx,
                                                 [Description("Role.")] DiscordRole role,
                                                 [Description("Mentionable?")] bool mentionable = true)
        {
            await role.UpdateAsync(mentionable: mentionable, reason: GetReasonString(ctx))
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx, $"Successfully set mentionable var for {Formatter.Bold(role.Name)} to {Formatter.Bold(mentionable.ToString())}.")
                .ConfigureAwait(false);
        }

        [Command("setmentionable"), Priority(0)]
        public async Task SetRoleMentionableAsync(CommandContext ctx,
                                                 [Description("Mentionable?")] bool mentionable,
                                                 [Description("Role.")] DiscordRole role)
            => await SetRoleMentionableAsync(ctx, role, mentionable).ConfigureAwait(false);
        #endregion

        #region COMMAND_ROLES_SETVISIBILITY
        [Command("setvisible")]
        [Description("Set role hoisted var (visibility in online list).")]
        [Aliases("separate", "h", "seth", "hoist", "sethoist")]
        [UsageExample("!role setvisible Admins")]
        [UsageExample("!role setvisible Admins false")]
        [UsageExample("!role setvisible false Admins")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetRoleVisibleAsync(CommandContext ctx,
                                             [Description("Role.")] DiscordRole role,
                                             [Description("Hoisted (visible in online list)?")] bool hoisted = false)
        {
            await role.UpdateAsync(hoist: hoisted, reason: GetReasonString(ctx))
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx, $"Successfully set hoisted var for role {Formatter.Bold(role.Name)} to {Formatter.Bold(hoisted.ToString())}.")
                .ConfigureAwait(false);
        }

        [Command("setvisible"), Priority(0)]
        public async Task SetRoleVisibleAsync(CommandContext ctx,
                                             [Description("Hoisted (visible in online list)?")] bool hoisted,
                                             [Description("Role.")] DiscordRole role)
            => await SetRoleVisibleAsync(ctx, role, hoisted).ConfigureAwait(false);
        #endregion
    }
}
