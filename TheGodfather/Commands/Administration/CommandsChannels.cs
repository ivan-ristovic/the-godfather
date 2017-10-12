#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Commands.Administration
{
    [Group("channel", CanInvokeWithoutSubcommand = false)]
    [Description("Miscellaneous channel control commands.")]
    [Aliases("channels", "c", "chn")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class CommandsChannels
    {
        #region COMMAND_CHANNEL_CREATECATEGORY
        [Command("createcategory")]
        [Description("Create new channel category.")]
        [Aliases("createc", "+c", "makec", "newc", "addc")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateCategory(CommandContext ctx,
                                        [RemainingText, Description("Name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing category name.");

            await ctx.Guild.CreateChannelAsync(name, ChannelType.Category);
            await ctx.RespondAsync($"Category {Formatter.Bold(name)} successfully created.");
        }
        #endregion

        #region COMMAND_CHANNEL_CREATETEXT
        [Command("createtext")]
        [Description("Create new txt channel.")]
        [Aliases("createtxt", "createt", "+", "+t", "maket", "newt", "addt")]
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
        [Aliases("createv", "+v", "makev", "newv", "addv")]
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
        [Description("Delete channel.")]
        [Aliases("-", "del", "d", "remove")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task DeleteChannel(CommandContext ctx, 
                                       [Description("Channel.")] DiscordChannel c = null)
        {
            if (c == null)
                c = ctx.Channel;

            string name = c.Name;
            await c.DeleteAsync();
            await ctx.RespondAsync($"Channel {Formatter.Bold(name)} successfully deleted.");
        }
        #endregion

        #region COMMAND_CHANNEL_INFO
        [Command("info")]
        [Description("Get channel information.")]
        [Aliases("i", "information")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ChannelInfo(CommandContext ctx,
                                     [Description("Channel.")] DiscordChannel c = null)
        {
            if (c == null)
                c = ctx.Channel;

            var em = new DiscordEmbedBuilder() {
                Title = "Details for channel: " + c.Name,
                Description = c.Topic,
                Color = DiscordColor.Goldenrod
            };
            em.AddField("Type", c.Type.ToString(), inline: true);
            em.AddField("NSFW", c.IsNSFW ? "Yes" : "No", inline: true);
            em.AddField("Private", c.IsPrivate ? "Yes" : "No", inline: true);
            if (c.Type == ChannelType.Voice)
                em.AddField("Bitrate", c.Bitrate.ToString(), inline: true);
            em.AddField("User limit", c.UserLimit == 0 ? "No limit." : c.UserLimit.ToString(), inline: true);
            em.AddField("Created", c.CreationTimestamp.ToString(), inline: true);

            await ctx.RespondAsync(embed: em);
        }
        #endregion

        #region COMMAND_CHANNEL_RENAME
        [Command("rename")]
        [Description("Rename channel.")]
        [Aliases("r", "name", "setname")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task RenameChannel(CommandContext ctx, 
                                       [Description("New name.")] string name = null,
                                       [Description("Channel.")] DiscordChannel c = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing new channel name.");
            if (name.Contains(" "))
                throw new InvalidCommandUsageException("Name cannot contain spaces.");
            if (c == null)
                c = ctx.Channel;

            try {
                await c.ModifyAsync(name);
            } catch (BadRequestException e) {
                throw new CommandFailedException("Error occured. Possibly the name entered contains invalid characters...", e);
            }

            await ctx.RespondAsync("Channel successfully renamed.");
        }
        #endregion

        #region COMMAND_CHANNEL_SETTOPIC
        [Command("settopic")]
        [Description("Set channel topic.")]
        [Aliases("t", "topic")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task SetChannelTopic(CommandContext ctx,
                                         [Description("New topic.")] string topic = null, 
                                         [Description("Channel.")] DiscordChannel c = null)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new InvalidCommandUsageException("Missing topic.");
            if (c == null)
                c = ctx.Channel;

            await c.ModifyAsync(topic: topic);
            await ctx.RespondAsync("Channel topic successfully changed.");
        }
        #endregion
    }
}
