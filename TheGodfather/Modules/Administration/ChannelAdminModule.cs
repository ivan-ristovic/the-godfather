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
using DSharpPlus.Interactivity;
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
            
            await ctx.Guild.CreateChannelAsync(name, ChannelType.Category, reason: GetReasonString(ctx))
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx, $"Category {Formatter.Bold(name)} successfully created.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHANNEL_CREATETEXT
        [Command("createtext")]
        [Description("Create new txt channel.")]
        [Aliases("createtxt", "createt", "ctxt", "ct", "+", "+t", "+txt")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateTextChannelAsync(CommandContext ctx,
                                                [Description("Name.")] string name,
                                                [Description("Parent category.")] DiscordChannel parent = null)
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

            if (!await TryCreateChannelAsync(ctx, name, parent, ChannelType.Text).ConfigureAwait(false))
                throw new CommandFailedException("Channel parent must be a category!");

            await ReplySuccessAsync(ctx, $"Channel {Formatter.Bold(name)} successfully created.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHANNEL_CREATEVOICE
        [Command("createvoice")]
        [Description("Create new voice channel.")]
        [Aliases("createv", "cvoice", "cv", "+voice", "+v")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateVoiceChannelAsync(CommandContext ctx,
                                                 [Description("Name.")] string name,
                                                 [Description("Parent category.")] DiscordChannel parent = null)
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

            if (!await TryCreateChannelAsync(ctx, name, parent, ChannelType.Voice).ConfigureAwait(false))
                throw new CommandFailedException("Channel parent must be a category!");

            await ctx.RespondAsync($"Channel {Formatter.Bold(name)} successfully created.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHANNEL_DELETE
        [Command("delete")]
        [Description("Delete a given channel or category.")]
        [Aliases("-", "del", "d", "remove", "rm")]
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
        #endregion

        #region COMMAND_CHANNEL_RENAME
        [Command("rename")]
        [Description("Rename channel.")]
        [Aliases("r", "name", "setname")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task RenameChannelAsync(CommandContext ctx,
                                            [Description("New name.")] string newname,
                                            [Description("Channel to rename.")] DiscordChannel channel = null,
                                            [Description("Reason.")] string reason = null)
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
        #endregion
        
        #region COMMAND_CHANNEL_SETPOSITION
        [Command("setposition")]
        [Description("Change the position of the given channel in the guild channel list.")]
        [Aliases("setpos", "sp", "pos", "setp")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ReorderChannelAsync(CommandContext ctx,
                                            [Description("Position.")] int position,
                                            [Description("Channel to rename.")] DiscordChannel channel = null,
                                            [Description("Reason.")] string reason = null)
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
        #endregion

        #region COMMAND_CHANNEL_SETTOPIC
        [Command("settopic")]
        [Description("Set channel topic.")]
        [Aliases("t", "topic", "sett")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task SetChannelTopicAsync(CommandContext ctx,
                                              [Description("New topic.")] string topic,
                                              [Description("Channel.")] DiscordChannel channel = null,
                                              [Description("Reason.")] string reason = null)
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
        #endregion


        #region HELPER_FUNCTIONS
        private async Task<bool> TryCreateChannelAsync(CommandContext ctx, string cname, DiscordChannel parent, ChannelType type)
        {
            if (parent != null && parent.Type != ChannelType.Category)
                return false;

            await ctx.Guild.CreateChannelAsync(cname, type, parent: parent, reason: GetReasonString(ctx))
                .ConfigureAwait(false);
            return true;
        }
        #endregion
    }
}
