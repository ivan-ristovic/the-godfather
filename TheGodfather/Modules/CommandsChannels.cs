#region USING_DIRECTIVES
using System;
using System.Linq;
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
    public class CommandsChannels
    {
        #region COMMAND_CHANNEL_CREATE
        [Command("createtxt")]
        [Description("Create new txt channel.")]
        [Aliases("create", "+", "+t", "make", "new", "add")]
        [RequirePermissions(Permissions.ManageChannels)]
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
        [RequirePermissions(Permissions.ManageChannels)]
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
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task DeleteChannel(CommandContext ctx, [Description("Channel")] DiscordChannel c = null)
        {
            if (c == null)
                throw new ArgumentException("Can't find such channel.");

            await c.DeleteAsync();
            await ctx.RespondAsync("Channel successfully deleted.");
        }
        #endregion

        #region COMMAND_CHANNEL_INVITE
        [Command("invite")]
        [Description("Get an instant invite link for the current channel.")]
        [Aliases("r", "name", "setname")]
        [RequirePermissions(Permissions.CreateInstantInvite)]
        public async Task RenameChannel(CommandContext ctx)
        {
            var invites = ctx.Channel.GetInvitesAsync().Result.Where(
                inv => (inv.Channel.Id == ctx.Channel.Id) && !inv.IsTemporary
            );

            if (invites.Count() > 0)
                await ctx.RespondAsync(invites.ElementAt(0).ToString());
            else {
                var invite = await ctx.Channel.CreateInviteAsync(max_age: 3600, temporary: true);
                await ctx.RespondAsync("This invite will expire in one hour!\n" + invite.ToString());
            }
        }
        #endregion

        #region COMMAND_CHANNEL_RENAME
        [Command("rename")]
        [Description("Rename channel.")]
        [Aliases("r", "name", "setname")]
        [RequirePermissions(Permissions.ManageChannels)]
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
        [RequirePermissions(Permissions.ManageChannels)]
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
