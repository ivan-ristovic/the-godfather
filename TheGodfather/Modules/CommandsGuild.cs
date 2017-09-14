#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot
{
    [Group("guild", CanInvokeWithoutSubcommand = false)]
    [Description("Miscellaneous guild control commands.")]
    [Aliases("server")]
    [RequirePermissions(Permissions.ManageGuild)]
    public class CommandsGuild
    {
        #region COMMAND_GUILD_RENAME
        [Command("rename")]
        [Description("Rename guild.")]
        [Aliases("r", "name", "setname")]
        public async Task RenameGuild(CommandContext ctx,
                                     [RemainingText, Description("New name")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Missing new guild name.");

            await ctx.Guild.ModifyAsync(name: name);
            await ctx.RespondAsync("Guild successfully renamed.");
        }
        #endregion
    }
}

