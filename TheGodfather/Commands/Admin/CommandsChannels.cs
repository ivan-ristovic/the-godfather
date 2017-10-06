#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfatherBot.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Commands.Admin
{
    [Group("channel", CanInvokeWithoutSubcommand = false)]
    [Description("Miscellaneous channel control commands.")]
    [Aliases("channels", "c", "chn")]
    public class CommandsChannels
    {
        #region COMMAND_CHANNEL_CREATE
        [Command("createtxt")]
        [Description("Create new txt channel.")]
        [Aliases("create", "+", "+t", "make", "new", "add")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateTextChannel(CommandContext ctx, 
                                           [RemainingText, Description("Name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing channel name.");
            
            await ctx.Guild.CreateChannelAsync(name, ChannelType.Text);
            await ctx.RespondAsync($"Channel {Formatter.Bold(name)} successfully created.");
        }
        #endregion

        #region COMMAND_CHANNEL_CREATEVOICE
        [Command("createvoice")]
        [Description("Create new voice channel.")]
        [Aliases("+v", "makev", "newv", "addv")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateVoiceChannel(CommandContext ctx, 
                                            [RemainingText, Description("Name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing channel name.");

            await ctx.Guild.CreateChannelAsync(name, ChannelType.Voice);
            await ctx.RespondAsync($"Channel {Formatter.Bold(name)} successfully created.");
        }
        #endregion
        
        #region COMMAND_CHANNEL_DELETE
        [Command("delete")]
        [Description("Delete channel")]
        [Aliases("-", "del", "d", "remove")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task DeleteChannel(CommandContext ctx, 
                                       [Description("Channel.")] DiscordChannel c = null)
        {
            if (c == null)
                throw new InvalidCommandUsageException("Can't find such channel.");

            string name = c.Name;
            await c.DeleteAsync();
            await ctx.RespondAsync($"Channel {Formatter.Bold(name)} successfully deleted.");
        }
        #endregion

        #region COMMAND_CHANNEL_RENAME
        [Command("rename")]
        [Description("Rename channel.")]
        [Aliases("r", "name", "setname")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task RenameChannel(CommandContext ctx, 
                                       [Description("Channel.")] DiscordChannel c = null,
                                       [RemainingText, Description("New name.")] string name = null)
        {
            if (c == null)
                throw new InvalidCommandUsageException("Can't find such channel.");
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing new channel name.");
            if (name.Contains(" "))
                throw new InvalidCommandUsageException("Name cannot contain spaces.");

            await c.ModifyAsync(name);
            await ctx.RespondAsync("Channel successfully renamed.");
        }
        #endregion

        #region COMMAND_CHANNEL_SETTOPIC
        [Command("settopic")]
        [Description("Set channel topic.")]
        [Aliases("t", "topic")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task SetChannelTopic(CommandContext ctx,
                                         [Description("Channel.")] DiscordChannel c = null,
                                         [RemainingText, Description("New topic.")] string topic = null)
        {
            if (c == null)
                throw new InvalidCommandUsageException("Can't find such channel.");
            if (string.IsNullOrWhiteSpace(topic))
                throw new InvalidCommandUsageException("Missing topic.");

            await c.ModifyAsync(topic: topic);
            await ctx.RespondAsync("Channel topic successfully changed.");
        }
        #endregion


        [Group("this", CanInvokeWithoutSubcommand = false)]
        [Description("Control over current channel.")]
        [Aliases("current", "cur", "curr")]
        public class CommandsThisChannel
        {
            #region COMMAND_CHANNEL_THIS_DELETE
            [Command("delete")]
            [Description("Delete this channel.")]
            [Aliases("-", "del", "d", "remove")]
            [RequirePermissions(Permissions.ManageChannels)]
            public async Task DeleteChannel(CommandContext ctx)
            {
                await ctx.Channel.DeleteAsync();
            }
            #endregion

            #region COMMAND_CHANNEL_THIS_RENAME
            [Command("rename")]
            [Description("Rename channel.")]
            [Aliases("r", "name", "setname")]
            [RequirePermissions(Permissions.ManageChannels)]
            public async Task RenameChannel(CommandContext ctx,
                                           [RemainingText, Description("New name.")] string name = null)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Missing new channel name.");

                if (name.Contains(" "))
                    throw new ArgumentException("Name cannot contain spaces.");

                await ctx.Channel.ModifyAsync(name);
                await ctx.RespondAsync("Channel successfully renamed.");
            }
            #endregion

            #region COMMAND_CHANNEL_THIS_SETTOPIC
            [Command("settopic")]
            [Description("Set channel topic.")]
            [Aliases("t", "topic")]
            [RequirePermissions(Permissions.ManageChannels)]
            public async Task SetChannelTopic(CommandContext ctx,
                                             [RemainingText, Description("New topic.")] string topic = null)
            {
                if (string.IsNullOrWhiteSpace(topic))
                    throw new InvalidCommandUsageException("Missing topic.");

                await ctx.Channel.ModifyAsync(topic: topic);
                await ctx.RespondAsync("Channel topic successfully changed.");
            }
            #endregion
        }
    }
}
