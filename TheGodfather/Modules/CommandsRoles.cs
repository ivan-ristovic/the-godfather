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


    }
}
