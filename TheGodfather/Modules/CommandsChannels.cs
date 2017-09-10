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
    [Group("channel", CanInvokeWithoutSubcommand = false)]
    [Description("Miscellaneous channel control commands.")]
    [Aliases("channels", "c", "chn")]
    [RequireUserPermissions(Permissions.ManageChannels)]
    public class CommandsChannels
    {
        #region COMMAND_CHANNEL_CREATE
        [Command("createtxt")]
        [Description("Create new txt channel.")]
        [Aliases("+t", "make", "new", "add")]
        public async Task CreateTextChannel(CommandContext ctx, [Description("Name")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Missing channel name.");
            
            await ctx.Guild.CreateChannelAsync(name, ChannelType.Text);
        }
        #endregion

        #region COMMAND_CHANNEL_CREATEVOICE
        [Command("createvoice")]
        [Description("Create new voice channel.")]
        [Aliases("+v", "makev", "newv", "addv")]
        public async Task CreateVoiceChannel(CommandContext ctx, [Description("Name")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Missing channel name.");

            await ctx.Guild.CreateChannelAsync(name, ChannelType.Voice);
        }
        #endregion
    }
}
