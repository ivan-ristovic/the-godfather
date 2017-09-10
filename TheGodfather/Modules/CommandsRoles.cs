#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot
{
    [Group("roles", CanInvokeWithoutSubcommand = true)]
    [Description("Miscellaneous role control commands.")]
    [Aliases("role", "r", "rl")]
    [RequirePermissions(Permissions.ManageRoles)]
    public class CommandsRoles
    {
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder() { Title = "Roles:" };
            foreach (var role in ctx.Guild.Roles)
                embed.AddField(role.Name, role.Position.ToString(), inline: true);
            await ctx.RespondAsync("", embed: embed);
        }

        #region COMMAND_ROLES_CREATE
        [Command("create")]
        [Description("Create a new role.")]
        [Aliases("new", "add", "+")]
        public async Task CreateRole(CommandContext ctx, [Description("Role name")] string name = null)
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
        public async Task DeleteRole(CommandContext ctx, [Description("Role (mention)")] DiscordRole role = null)
        {
            if (role == null)
                throw new ArgumentNullException("Unknown role.");

            await ctx.Guild.DeleteRoleAsync(role);
            await ctx.RespondAsync("Successfully removed role " + role.Name);
        }
        #endregion

        #region COMMAND_ROLES_SETCOLOR
        [Command("setcolor")]
        [Description("Set a color for the role.")]
        [Aliases("clr", "c")]
        public async Task SetColor(CommandContext ctx, 
                                  [Description("Role name")] DiscordRole role = null,
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
        [Aliases("name", "rename")]
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

    }
}
