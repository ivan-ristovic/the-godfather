#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Modules.Admin
{
    [Group("roles", CanInvokeWithoutSubcommand = true)]
    [Description("Miscellaneous role control commands.")]
    [Aliases("role", "r", "rl")]
    public class CommandsRoles
    {
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            string s = "";
            foreach (var role in ctx.Guild.Roles.OrderBy(r => r.Position).Reverse())
                s += role.Name + "\n";
            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = "Roles:",
                Description = s,
                Color = DiscordColor.Gold
            });
        }


        #region COMMAND_ROLES_CREATE
        [Command("create")]
        [Description("Create a new role.")]
        [Aliases("new", "add", "+")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task CreateRole(CommandContext ctx, 
                                    [Description("Role")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Missing role name.");

            await ctx.Guild.CreateRoleAsync(name);
            await ctx.RespondAsync("Successfully created role " + name);
        }
        #endregion

        #region COMMAND_ROLES_DELETE
        [Command("delete")]
        [Description("Create a new role.")]
        [Aliases("del", "remove", "d", "-")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task DeleteRole(CommandContext ctx, [Description("Role")] DiscordRole role = null)
        {
            if (role == null)
                throw new ArgumentException("Unknown role.");

            await ctx.Guild.DeleteRoleAsync(role);
            await ctx.RespondAsync("Successfully removed role " + role.Name);
        }
        #endregion
        
        #region COMMAND_ROLES_MENTIONALL
        [Command("mentionall")]
        [Description("Mention all users from given role.")]
        [Aliases("mention", "@", "ma")]
        public async Task MentionAllFromRole(CommandContext ctx, 
                                            [Description("Role")] DiscordRole role = null)
        {
            if (role == null)
                throw new ArgumentException("Unknown role.");

            var users = ctx.Guild.GetAllMembersAsync().Result.Where(u => u.Roles.Contains(role));
            string s = "";
            foreach (var user in users)
                s += user.Mention + " ";
            await ctx.RespondAsync(s);
        }
        #endregion

        #region COMMAND_ROLES_SETCOLOR
        [Command("setcolor")]
        [Description("Set a color for the role.")]
        [Aliases("clr", "c")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetColor(CommandContext ctx, 
                                  [Description("Role")] DiscordRole role = null,
                                  [Description("Color")] string color = null)
        {
            if (role == null || string.IsNullOrWhiteSpace(color))
                throw new ArgumentException("I need a valid role and a valid color.");

            await ctx.Guild.UpdateRoleAsync(role, color: new DiscordColor(color));
            await ctx.RespondAsync("Successfully changed role color.");
        }
        #endregion

        #region COMMAND_ROLES_SETNAME
        [Command("setname")]
        [Description("Set a name for the role.")]
        [Aliases("name", "rename", "n")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RenameRole(CommandContext ctx,
                                    [Description("Role")] DiscordRole role = null,
                                    [Description("New name")] string name = null)
        {
            if (role == null || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("I need a valid existing role and a new name.");

            await ctx.Guild.UpdateRoleAsync(role, name: name);
            await ctx.RespondAsync("Successfully changed role name.");
        }
        #endregion

        #region COMMAND_ROLES_SETMENTIONABLE
        [Command("setmentionable")]
        [Description("Set role mentionable var.")]
        [Aliases("mentionable", "m", "setm")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetMentionable(CommandContext ctx,
                                        [Description("Role")] DiscordRole role = null,
                                        [Description("True/False")] bool b = true)
        {
            if (role == null)
                throw new ArgumentException("Unknown role.");

            await ctx.Guild.UpdateRoleAsync(role, mentionable: b);
            await ctx.RespondAsync("Successfully set " + role.Name + "'s mentionable var to: " + b);
        }
        #endregion
        
        #region COMMAND_ROLES_SETVISIBILITY
        [Command("setvisible")]
        [Description("Set role hoist var (visibility in online list.")]
        [Aliases("separate", "h", "sets", "seth", "hoist", "sethoist")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetVisible(CommandContext ctx,
                                    [Description("Role")] DiscordRole role = null,
                                    [Description("True/False")] bool b = true)
        {
            if (role == null)
                throw new ArgumentException("Unknown role.");

            await ctx.Guild.UpdateRoleAsync(role, hoist: b);
            await ctx.RespondAsync("Successfully set " + role.Name + "'s hoist var to: " + b);
        }
        #endregion
    }
}
