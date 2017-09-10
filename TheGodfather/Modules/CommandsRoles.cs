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
        public async Task Ban(CommandContext ctx, [Description("Role name")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("Missing role name.");

            await ctx.Guild.CreateRoleAsync(name);
            await ctx.RespondAsync("Successfully created role " + name);
        }
        #endregion
    }
}
