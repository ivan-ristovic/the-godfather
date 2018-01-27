#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Models;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("channel", CanInvokeWithoutSubcommand = false)]
    [Description("Miscellaneous channel control commands.")]
    [Aliases("channels", "c", "chn")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public class ChannelAdminModule : GodfatherBaseModule
    {

        public ChannelAdminModule(SharedData shared, DatabaseService db) : base(shared, db) { }
        

        #region COMMAND_CHANNEL_CREATECATEGORY
        [Command("createcategory")]
        [Description("Create new channel category.")]
        [Aliases("createcat", "createc", "ccat", "cc", "+cat", "+c", "+category")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateCategoryAsync(CommandContext ctx,
                                             [RemainingText, Description("Name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing category name.");

            if (name.Length < 2 || name.Length > 100)
                throw new InvalidCommandUsageException("Name must be longer than 2 and shorter than 100 characters.");

            if (ctx.Guild.Channels.Any(chn => chn.Name == name.ToLower())) {
                await ctx.RespondAsync("A channel with that name already exists. Continue?")
                    .ConfigureAwait(false);
                if (!await InteractivityUtil.WaitForConfirmationAsync(ctx).ConfigureAwait(false))
                    return;
            }
            
            await ctx.Guild.CreateChannelCategoryAsync(name, reason: GetReasonString(ctx))
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx, $"Category {Formatter.Bold(name)} successfully created.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHANNEL_CREATETEXT
        [Command("createtext")]
        [Description("Create new txt channel.")]
        [Aliases("createtxt", "createt", "ctxt", "ct", "+", "+t", "+txt")]
        [Priority(2)]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateTextChannelAsync(CommandContext ctx,
                                                [Description("Name.")] string name,
                                                [Description("Parent category.")] DiscordChannel parent = null,
                                                [Description("NSFW?")] bool nsfw = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing channel name.");

            if (name.Contains(' '))
                throw new InvalidCommandUsageException("Name cannot contain spaces.");

            if (name.Length < 2 || name.Length > 100)
                throw new InvalidCommandUsageException("Name must be longer than 2 and shorter than 100 characters.");

            if (ctx.Guild.Channels.Any(chn => chn.Name == name.ToLower())) {
                await ctx.RespondAsync("A channel with that name already exists. Continue anyway?")
                    .ConfigureAwait(false);
                if (!await InteractivityUtil.WaitForConfirmationAsync(ctx).ConfigureAwait(false))
                    return;
            }

            if (parent != null && parent.Type != ChannelType.Category)
                throw new CommandFailedException("Channel parent must be a category!");

            await ctx.Guild.CreateTextChannelAsync(name, parent, nsfw: nsfw, reason: GetReasonString(ctx))
                .ConfigureAwait(false);

            await ReplySuccessAsync(ctx, $"Channel {Formatter.Bold(name)} successfully created.")
                .ConfigureAwait(false);
        }

        [Command("createtext")]
        [Priority(1)]
        public async Task CreateTextChannelAsync(CommandContext ctx,
                                                [Description("Name.")] string name,
                                                [Description("NSFW?")] bool nsfw = false,
                                                [Description("Parent category.")] DiscordChannel parent = null)
           => await CreateTextChannelAsync(ctx, name, parent, nsfw);

        [Command("createtext")]
        [Priority(0)]
        public async Task CreateTextChannelAsync(CommandContext ctx, 
                                                [Description("Parent category.")] DiscordChannel parent,
                                                [Description("Name.")] string name,
                                                [Description("NSFW?")] bool nsfw = false)
           => await CreateTextChannelAsync(ctx, name, parent, nsfw);
        #endregion

        #region COMMAND_CHANNEL_CREATEVOICE
        [Command("createvoice")]
        [Description("Create new voice channel.")]
        [Aliases("createv", "cvoice", "cv", "+voice", "+v")]
        [RequirePermissions(Permissions.ManageChannels)]
        [Priority(2)]
        public async Task CreateVoiceChannelAsync(CommandContext ctx,
                                                 [Description("Name.")] string name,
                                                 [Description("Parent category.")] DiscordChannel parent = null,
                                                 [Description("User limit.")] int? userlimit = null,
                                                 [Description("Bitrate.")] int? bitrate = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing channel name.");

            if (name.Length < 2 || name.Length > 100)
                throw new InvalidCommandUsageException("Name must be longer than 2 and shorter than 100 characters.");

            if (ctx.Guild.Channels.Any(chn => chn.Name == name.ToLower())) {
                await ctx.RespondAsync("A channel with that name already exists. Continue anyway?")
                    .ConfigureAwait(false);
                if (!await InteractivityUtil.WaitForConfirmationAsync(ctx).ConfigureAwait(false))
                    return;
            }

            if (parent != null && parent.Type != ChannelType.Category)
                throw new CommandFailedException("Channel parent must be a category!");

            await ctx.Guild.CreateVoiceChannelAsync(name, parent, bitrate, userlimit, reason: GetReasonString(ctx))
                .ConfigureAwait(false);

            await ctx.RespondAsync($"Channel {Formatter.Bold(name)} successfully created.")
                .ConfigureAwait(false);
        }


        [Command("createvoice")]
        [Priority(1)]
        public async Task CreateVoiceChannelAsync(CommandContext ctx,
                                                 [Description("Name.")] string name,
                                                 [Description("User limit.")] int? userlimit = null,
                                                 [Description("Bitrate.")] int? bitrate = null,
                                                 [Description("Parent category.")] DiscordChannel parent = null)
            => await CreateVoiceChannelAsync(ctx, name, parent, userlimit, bitrate);

        [Command("createvoice")]
        [Priority(0)]
        public async Task CreateVoiceChannelAsync(CommandContext ctx,
                                                 [Description("Parent category.")] DiscordChannel parent,
                                                 [Description("Name.")] string name,
                                                 [Description("User limit.")] int? userlimit = null,
                                                 [Description("Bitrate.")] int? bitrate = null)
            => await CreateVoiceChannelAsync(ctx, name, parent, userlimit, bitrate);
        #endregion

        #region COMMAND_CHANNEL_DELETE
        [Command("delete")]
        [Description("Delete a given channel or category.")]
        [Aliases("-", "del", "d", "remove", "rm")]
        [Priority(1)]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task DeleteChannelAsync(CommandContext ctx,
                                            [Description("Channel to delete.")] DiscordChannel channel = null,
                                            [RemainingText, Description("Reason.")] string reason = null)
        {
            if (channel == null)
                channel = ctx.Channel;

            if (channel.Type == ChannelType.Category && channel.Children.Count() > 0) {
                await ctx.RespondAsync("The channel specified is a non-empty category. Delete all channels in it recursively?");
                if (await InteractivityUtil.WaitForConfirmationAsync(ctx)) {
                    foreach (var chn in channel.Children.ToList()) {
                        await chn.DeleteAsync(reason: GetReasonString(ctx, reason))
                            .ConfigureAwait(false);
                    }
                }
            }

            string name = channel.Name;
            await channel.DeleteAsync(reason: GetReasonString(ctx, reason))
                .ConfigureAwait(false);
            if (channel.Id != ctx.Channel.Id)
                await ReplySuccessAsync(ctx, $"Channel {Formatter.Bold(name)} successfully deleted.")
                    .ConfigureAwait(false);
        }

        [Command("delete")]
        [Priority(0)]
        public async Task DeleteChannelAsync(CommandContext ctx,
                                            [RemainingText, Description("Reason.")] string reason)
            => await DeleteChannelAsync(ctx, null, reason);
        #endregion

        #region COMMAND_CHANNEL_INFO
        [Command("info")]
        [Description("Get information about a given channel.")]
        [Aliases("i", "information")]
        [RequirePermissions(Permissions.AccessChannels)]
        public async Task ChannelInfoAsync(CommandContext ctx,
                                          [Description("Channel.")] DiscordChannel channel = null)
        {
            if (channel == null)
                channel = ctx.Channel;

            if (!ctx.Member.PermissionsIn(channel).HasFlag(Permissions.AccessChannels))
                throw new CommandFailedException("You are not allowed to see this channel! (nice try smartass)");

            var em = new DiscordEmbedBuilder() {
                Title = "Information for: " + channel.ToString(),
                Description = "Current channel topic: " + (string.IsNullOrWhiteSpace(channel.Topic) ? "None." : Formatter.Italic(channel.Topic)),
                Color = DiscordColor.Goldenrod
            };
            em.AddField("Type", channel.Type.ToString(), inline: true);
            em.AddField("NSFW", channel.IsNSFW ? "Yes" : "No", inline: true);
            em.AddField("Private", channel.IsPrivate ? "Yes" : "No", inline: true);
            em.AddField("Position", channel.Position.ToString());
            if (channel.Type == ChannelType.Voice) {
                em.AddField("Bitrate", channel.Bitrate.ToString(), inline: true);
                em.AddField("User limit", channel.UserLimit == 0 ? "No limit." : channel.UserLimit.ToString(), inline: true);
            }
            em.AddField("Creation time", channel.CreationTimestamp.ToString(), inline: true);

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHANNEL_MODIFY
        [Command("modify")]
        [Description("Modify a given voice channel. Set 0 if you wish to keep the value as it is.")]
        [Aliases("edit", "mod", "m", "e")]
        [Priority(1)]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ModifyChannelAsync(CommandContext ctx,
                                            [Description("Voice channel to edit")] DiscordChannel channel,
                                            [Description("User limit.")] int limit = 0,
                                            [Description("Bitrate.")] int bitrate = 0,
                                            [RemainingText, Description("Reason.")] string reason = null)
        {
            if (channel == null)
                throw new InvalidCommandUsageException("Channel missing.");

            if (channel.Type != ChannelType.Voice)
                throw new InvalidCommandUsageException("That isn't a voice channel");

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                if (limit > 0)
                    m.Userlimit = limit;
                if (channel.Type == ChannelType.Voice && bitrate > 0)
                    m.Bitrate = bitrate;
                m.AuditLogReason = GetReasonString(ctx, reason);
            })).ConfigureAwait(false);

            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }

        [Command("modify")]
        [Priority(0)]
        public async Task ModifyChannelAsync(CommandContext ctx,
                                            [Description("User limit.")] int limit = 0,
                                            [Description("Bitrate.")] int bitrate = 0,
                                            [RemainingText, Description("Reason.")] string reason = null)
            => await ModifyChannelAsync(ctx, null, limit, bitrate, reason);
        #endregion

        #region COMMAND_CHANNEL_RENAME
        [Command("rename")]
        [Description("Rename channel.")]
        [Aliases("r", "name", "setname")]
        [Priority(2)]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task RenameChannelAsync(CommandContext ctx,
                                            [Description("Channel to rename.")] DiscordChannel channel,
                                            [Description("New name.")] string newname,
                                            [RemainingText, Description("Reason.")] string reason = null)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("Missing new channel name.");

            if (newname.Contains(' '))
                throw new InvalidCommandUsageException("Name cannot contain spaces.");

            if (newname.Length < 2 || newname.Length > 100)
                throw new InvalidCommandUsageException("Name must be longer than 2 and shorter than 100 characters.");

            if (channel == null)
                channel = ctx.Channel;

            try {
                await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                    m.Name = newname;
                    m.AuditLogReason = GetReasonString(ctx, reason);
                })).ConfigureAwait(false);
            } catch (BadRequestException e) {
                throw new CommandFailedException("An error occured. Maybe the name entered contains invalid characters?", e);
            }

            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }

        [Command("rename")]
        [Priority(1)]
        public async Task RenameChannelAsync(CommandContext ctx,
                                            [Description("New name.")] string newname,
                                            [Description("Channel to rename.")] DiscordChannel channel,
                                            [RemainingText, Description("Reason.")] string reason = null)
            => await RenameChannelAsync(ctx, channel, newname, reason);

        [Command("rename")]
        [Priority(0)]
        public async Task RenameChannelAsync(CommandContext ctx,
                                            [Description("New name.")] string newname,
                                            [RemainingText, Description("Reason.")] string reason = null)
            => await RenameChannelAsync(ctx, null, newname, reason);
        #endregion

        #region COMMAND_CHANNEL_SETPARENT
        [Command("setparent")]
        [Description("Change the parent of the given channel.")]
        [Aliases("setpar", "par", "parent")]
        [Priority(1)]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ChangeParentAsync(CommandContext ctx,
                                           [Description("Child channel.")] DiscordChannel channel,
                                           [Description("Parent category.")] DiscordChannel parent,
                                           [RemainingText, Description("Reason.")] string reason = null)
        {
            if (parent == null)
                throw new InvalidCommandUsageException("Parent category missing.");

            if (parent.Type != ChannelType.Category)
                throw new CommandFailedException("Parent channel must be a category.");

            if (channel == null)
                channel = ctx.Channel;

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Parent = parent;
                m.AuditLogReason = GetReasonString(ctx, reason);
            })).ConfigureAwait(false);

            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }

        [Command("setparent")]
        [Priority(0)]
        public async Task ChangeParentAsync(CommandContext ctx,
                                           [Description("Parent category.")] DiscordChannel parent,
                                           [RemainingText, Description("Reason.")] string reason = null)
            => await ChangeParentAsync(ctx, null, parent, reason);
        #endregion

        #region COMMAND_CHANNEL_SETPOSITION
        [Command("setposition")]
        [Description("Change the position of the given channel in the guild channel list.")]
        [Aliases("setpos", "pos", "position")]
        [Priority(2)]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ReorderChannelAsync(CommandContext ctx,
                                             [Description("Channel to reorder.")] DiscordChannel channel,
                                             [Description("Position.")] int position,
                                             [RemainingText, Description("Reason.")] string reason = null)
        {
            if (position < 0)
                throw new InvalidCommandUsageException("Position cannot be negative...");

            if (channel == null)
                channel = ctx.Channel;

            await channel.ModifyPositionAsync(position, GetReasonString(ctx, reason))
                .ConfigureAwait(false);

            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }

        [Command("setposition")]
        [Priority(1)]
        public async Task ReorderChannelAsync(CommandContext ctx,
                                             [Description("Position.")] int position,
                                             [Description("Channel to reorder.")] DiscordChannel channel,
                                             [RemainingText, Description("Reason.")] string reason = null)
            => await ReorderChannelAsync(ctx, channel, position, reason);

        [Command("setposition")]
        [Priority(0)]
        public async Task ReorderChannelAsync(CommandContext ctx,
                                             [Description("Position.")] int position,
                                             [RemainingText, Description("Reason.")] string reason = null)
            => await ReorderChannelAsync(ctx, null, position, reason);
        #endregion

        #region COMMAND_CHANNEL_SETTOPIC
        [Command("settopic")]
        [Description("Set channel topic.")]
        [Aliases("t", "topic", "sett")]
        [Priority(2)]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task SetChannelTopicAsync(CommandContext ctx,
                                              [Description("Channel.")] DiscordChannel channel,
                                              [Description("New topic.")] string topic,
                                              [RemainingText, Description("Reason.")] string reason = null)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new InvalidCommandUsageException("Missing topic.");
            if (channel == null)
                channel = ctx.Channel;

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Topic = topic;
                m.AuditLogReason = GetReasonString(ctx, reason);
            })).ConfigureAwait(false);
            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }

        [Command("settopic")]
        [Priority(1)]
        public async Task SetChannelTopicAsync(CommandContext ctx,
                                              [Description("New topic.")] string topic,
                                              [Description("Channel.")] DiscordChannel channel,
                                              [RemainingText, Description("Reason.")] string reason = null)
           => await SetChannelTopicAsync(ctx, channel, topic, reason);

        [Command("settopic")]
        [Priority(0)]
        public async Task SetChannelTopicAsync(CommandContext ctx,
                                              [Description("New topic.")] string topic,
                                              [RemainingText, Description("Reason.")] string reason = null)
            => await SetChannelTopicAsync(ctx, null, topic, reason);
        #endregion
    }
}
