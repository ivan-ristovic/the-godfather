#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections;
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
        #region COMMAND_GUILD_LISTMEMBERS
        [Command("listmembers")]
        [Description("Rename guild.")]
        [Aliases("memberlist", "listm", "lm", "mem", "members")]
        public async Task ListMembers(CommandContext ctx, [Description("Page")] int page = 1)
        {
            var members = await ctx.Guild.GetAllMembersAsync();

            if (page < 1 || page > members.Count / 10 + 1)
                throw new ArgumentException("No members on that page.");

            string s = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < members.Count ? starti + 10 : members.Count;
            var membersarray = members.ToArray();
            for (var i = starti; i < endi; i++)
                s += $"**{membersarray[i].Username}** , joined at: {membersarray[i].JoinedAt}\n";

            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = $"Members (page {page}) :",
                Description = s,
                Color = DiscordColor.SapGreen
            });
        }
        #endregion

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

