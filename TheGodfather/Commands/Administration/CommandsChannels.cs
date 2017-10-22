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
        public async Task CreateCategoryAsync(CommandContext ctx,
                                             [RemainingText, Description("Name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing category name.");

            await ctx.Guild.CreateChannelAsync(name, ChannelType.Category, reason: $"Created by Godfather: {ctx.User.Username} ({ctx.User.Id})")
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Category {Formatter.Bold(name)} successfully created.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHANNEL_CREATETEXT
        [Command("createtext")]
        [Description("Create new txt channel.")]
        [Aliases("createtxt", "createt", "+", "+t", "maket", "newt", "addt")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateTextChannelAsync(CommandContext ctx, 
                                                [Description("Name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing channel name.");
            if (name.Contains(" "))
                throw new InvalidCommandUsageException("Name cannot contain spaces.");

            await ctx.Guild.CreateChannelAsync(name, ChannelType.Text, reason: $"Created by Godfather: {ctx.User.Username} ({ctx.User.Id})")
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Channel {Formatter.Bold(name)} successfully created.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHANNEL_CREATEVOICE
        [Command("createvoice")]
        [Description("Create new voice channel.")]
        [Aliases("createv", "+v", "makev", "newv", "addv")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateVoiceChannelAsync(CommandContext ctx, 
                                                 [RemainingText, Description("Name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing channel name.");

            await ctx.Guild.CreateChannelAsync(name, ChannelType.Voice, reason: $"Created by Godfather: {ctx.User.Username} ({ctx.User.Id})")
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Channel {Formatter.Bold(name)} successfully created.")
                .ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_CHANNEL_DELETE
        [Command("delete")]
        [Description("Delete channel.")]
        [Aliases("-", "del", "d", "remove")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task DeleteChannelAsync(CommandContext ctx, 
                                            [Description("Channel.")] DiscordChannel c = null)
        {
            if (c == null)
                c = ctx.Channel;

            string name = c.Name;
            await c.DeleteAsync(reason: $"Deleted by Godfather: {ctx.User.Username} ({ctx.User.Id})")
                .ConfigureAwait(false);
            if (c.Id != ctx.Channel.Id)
                await ctx.RespondAsync($"Channel {Formatter.Bold(name)} successfully deleted.")
                    .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHANNEL_INFO
        [Command("info")]
        [Description("Get channel information.")]
        [Aliases("i", "information")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ChannelInfoAsync(CommandContext ctx,
                                          [Description("Channel.")] DiscordChannel c = null)
        {
            if (c == null)
                c = ctx.Channel;

            var em = new DiscordEmbedBuilder() {
                Title = "Details for channel: " + c.Name,
                Description = "Topic: " + Formatter.Italic(c.Topic),
                Color = DiscordColor.Goldenrod
            };
            em.AddField("Type", c.Type.ToString(), inline: true);
            em.AddField("NSFW", c.IsNSFW ? "Yes" : "No", inline: true);
            em.AddField("Private", c.IsPrivate ? "Yes" : "No", inline: true);
            if (c.Type == ChannelType.Voice)
                em.AddField("Bitrate", c.Bitrate.ToString(), inline: true);
            em.AddField("User limit", c.UserLimit == 0 ? "No limit." : c.UserLimit.ToString(), inline: true);
            em.AddField("Created", c.CreationTimestamp.ToString(), inline: true);

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHANNEL_RENAME
        [Command("rename")]
        [Description("Rename channel.")]
        [Aliases("r", "name", "setname")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task RenameChannelAsync(CommandContext ctx, 
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
                await c.ModifyAsync(name, reason: $"Renamed by Godfather: {ctx.User.Username} ({ctx.User.Id})")
                    .ConfigureAwait(false);
            } catch (BadRequestException e) {
                throw new CommandFailedException("Error occured. Possibly the name entered contains invalid characters...", e);
            }

            await ctx.RespondAsync("Channel successfully renamed.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHANNEL_SETTOPIC
        [Command("settopic")]
        [Description("Set channel topic.")]
        [Aliases("t", "topic", "sett")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task SetChannelTopicAsync(CommandContext ctx,
                                              [Description("New topic.")] string topic = null, 
                                              [Description("Channel.")] DiscordChannel c = null)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new InvalidCommandUsageException("Missing topic.");
            if (c == null)
                c = ctx.Channel;

            await c.ModifyAsync(topic: topic, reason: $"Modified by Godfather: {ctx.User.Username} ({ctx.User.Id})")
                .ConfigureAwait(false);
            await ctx.RespondAsync("Channel topic successfully changed.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
