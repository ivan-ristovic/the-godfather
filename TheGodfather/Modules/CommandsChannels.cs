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
        [Aliases("create", "+", "+t", "make", "new", "add")]
        public async Task CreateTextChannel(CommandContext ctx, [RemainingText, Description("Name")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Missing channel name.");
            
            await ctx.Guild.CreateChannelAsync(name, ChannelType.Text);
            await ctx.RespondAsync("Channel successfully created.");
        }
        #endregion

        #region COMMAND_CHANNEL_CREATEVOICE
        [Command("createvoice")]
        [Description("Create new voice channel.")]
        [Aliases("+v", "makev", "newv", "addv")]
        public async Task CreateVoiceChannel(CommandContext ctx, [RemainingText, Description("Name")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Missing channel name.");

            await ctx.Guild.CreateChannelAsync(name, ChannelType.Voice);
            await ctx.RespondAsync("Channel successfully created.");
        }
        #endregion
        
        #region COMMAND_CHANNEL_DELETE
        [Command("delete")]
        [Description("Delete channel.")]
        [Aliases("-", "del", "d", "remove")]
        public async Task DeleteChannel(CommandContext ctx, [Description("Channel")] DiscordChannel c = null)
        {
            if (c == null)
                throw new ArgumentException("Can't find such channel.");

            await c.DeleteAsync();
            await ctx.RespondAsync("Channel successfully deleted.");
        }
        #endregion

        #region COMMAND_CHANNEL_RENAME
        [Command("rename")]
        [Description("Rename channel.")]
        [Aliases("r", "name")]
        public async Task RenameChannel(CommandContext ctx, 
                                       [Description("Channel")] DiscordChannel c = null,
                                       [RemainingText, Description("New name")] string name = null)
        {
            if (c == null)
                throw new ArgumentException("Can't find such channel.");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Missing new channel name.");

            await c.ModifyAsync(name);
            await ctx.RespondAsync("Channel successfully renamed.");
        }
        #endregion

        #region COMMAND_CHANNEL_SETTOPIC
        [Command("settopic")]
        [Description("Set channel topic.")]
        [Aliases("t", "topic")]
        public async Task SetChannelTopic(CommandContext ctx,
                                         [Description("Channel")] DiscordChannel c = null,
                                         [RemainingText, Description("New name")] string topic = null)
        {
            if (c == null)
                throw new ArgumentException("Can't find such channel.");
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Missing topic.");

            await c.ModifyAsync(topic: topic);
            await ctx.RespondAsync("Channel topic successfully changed.");
        }
        #endregion
    }
}
