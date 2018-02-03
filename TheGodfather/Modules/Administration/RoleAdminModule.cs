#region USING_DIRECTIVES
using System;
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
    public class RoleAdminModule : GodfatherBaseModule
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
        [UsageExample("!roles create My new role")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task CreateRoleAsync(CommandContext ctx,
                                         [Description("Name.")] string name,
                                         [Description("Color.")] DiscordColor? color = null,
                                         [Description("Hoisted?")] bool hoisted = false,
                                         [Description("Mentionable?")] bool mentionable = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing role name.");

            if (ctx.Guild.Roles.Any(r => string.Compare(r.Name, name, true) == 0)) 
                if (!await AskYesNoQuestionAsync(ctx, "A role with that name already exists. Continue?").ConfigureAwait(false))
                    return;

            await ctx.Guild.CreateRoleAsync(name, null, color, hoisted, mentionable, GetReasonString(ctx))
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx, $"Successfully created role {Formatter.Bold(name)}!")
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
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task DeleteRoleAsync(CommandContext ctx,
                                         [Description("Role.")] DiscordRole role)
        {
            if (role == null)
                throw new InvalidCommandUsageException("Unknown role.");

            string name = role.Name;
            await role.DeleteAsync()
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully removed role {Formatter.Bold(name)}!")
                .ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_ROLES_MENTIONALL
        [Command("mentionall")]
        [Description("Mention all users from given role.")]
        [Aliases("mention", "@", "ma")]
        [RequirePermissions(Permissions.MentionEveryone)]
        public async Task MentionAllFromRoleAsync(CommandContext ctx, 
                                                 [Description("Role.")] DiscordRole role)
        {
            if (role == null)
                throw new InvalidCommandUsageException("Unknown role.");
            
            var users = await ctx.Guild.GetAllMembersAsync()
                .ConfigureAwait(false);
            string desc = "";
            foreach (var user in users.Where(u => u.Roles.Contains(role)))
                desc += user.Mention + " ";

            await ctx.RespondAsync(desc)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ROLES_SETCOLOR
        [Command("setcolor")]
        [Description("Set a color for the role.")]
        [Aliases("clr", "c", "sc")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetRoleColorAsync(CommandContext ctx, 
                                           [Description("Role.")] DiscordRole role,
                                           [Description("Color.")] string color)
        {
            if (role == null || string.IsNullOrWhiteSpace(color))
                throw new InvalidCommandUsageException("I need a valid role and a valid color in hex.");

            await role.UpdateAsync(color: new DiscordColor(color))
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully changed color for {Formatter.Bold(role.Name)}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ROLES_SETNAME
        [Command("setname")]
        [Description("Set a name for the role.")]
        [Aliases("name", "rename", "n")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RenameRoleAsync(CommandContext ctx,
                                         [Description("Role.")] DiscordRole role,
                                         [RemainingText, Description("New name.")] string name)
        {
            if (role == null || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("I need a valid existing role and a new name.");

            await role.UpdateAsync(name: name, reason: $"{ctx.User.Username} ({ctx.User.Id})")
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully changed role name to {Formatter.Bold(name)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ROLES_SETMENTIONABLE
        [Command("setmentionable")]
        [Description("Set role mentionable var.")]
        [Aliases("mentionable", "m", "setm")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetRoleMentionableAsync(CommandContext ctx,
                                                 [Description("Role.")] DiscordRole role,
                                                 [Description("[true/false]")] bool b)
        {
            if (role == null)
                throw new InvalidCommandUsageException("Unknown role.");

            await role.UpdateAsync(mentionable: b, reason: $"{ctx.User.Username} ({ctx.User.Id})")
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully set mentionable var for {Formatter.Bold(role.Name)} to {Formatter.Bold(b.ToString())}.")
                .ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_ROLES_SETVISIBILITY
        [Command("setvisible")]
        [Description("Set role hoist var (visibility in online list.")]
        [Aliases("separate", "h", "seth", "hoist", "sethoist")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetRoleVisibleAsync(CommandContext ctx,
                                             [Description("Role.")] DiscordRole role,
                                             [Description("[true/false]")] bool b)
        {
            if (role == null)
                throw new InvalidCommandUsageException("Unknown role.");

            await role.UpdateAsync(hoist: b, reason: $"{ctx.User.Username} ({ctx.User.Id})")
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully set hoist var for {Formatter.Bold(role.Name)} to {Formatter.Bold(b.ToString())}.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
