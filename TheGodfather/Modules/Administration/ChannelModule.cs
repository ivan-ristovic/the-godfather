#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Models;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("channel"), Module(ModuleType.Administration)]
    [Description("Miscellaneous channel control commands. If invoked without subcommands, prints out channel information.")]
    [Aliases("channels", "c", "chn")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class ChannelModule : TheGodfatherBaseModule
    {

        public ChannelModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Channel.")] DiscordChannel channel = null)
            => InfoAsync(ctx, channel);


        #region COMMAND_CHANNEL_CREATECATEGORY
        [Command("createcategory"), Module(ModuleType.Administration)]
        [Description("Create new channel category.")]
        [Aliases("createcat", "createc", "ccat", "cc", "+cat", "+c", "+category")]
        [UsageExample("!channel createcategory My New Category")]
        [RequirePermissions(Permissions.ManageChannels)]
        [UsesInteractivity]
        public async Task CreateCategoryAsync(CommandContext ctx,
                                             [RemainingText, Description("Name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing category name.");

            if (name.Length < 2 || name.Length > 100)
                throw new InvalidCommandUsageException("Name must be longer than 2 and shorter than 100 characters.");

            if (ctx.Guild.Channels.Any(chn => chn.Name == name.ToLowerInvariant()))
                if (!await ctx.AskYesNoQuestionAsync("A channel with that name already exists. Continue?").ConfigureAwait(false))
                    return;

            await ctx.Guild.CreateChannelCategoryAsync(name, reason: ctx.BuildReasonString())
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Category {Formatter.Bold(name)} successfully created.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHANNEL_CREATETEXT
        [Command("createtext"), Priority(2)]
        [Module(ModuleType.Administration)]
        [Description("Create new text channel.")]
        [Aliases("createtxt", "createt", "ctxt", "ct", "+", "+t", "+txt")]
        [UsageExample("!channel createtext newtextchannel ParentCategory no")]
        [UsageExample("!channel createtext newtextchannel no")]
        [UsageExample("!channel createtext ParentCategory newtextchannel")]
        [RequirePermissions(Permissions.ManageChannels)]
        [UsesInteractivity]
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

            if (ctx.Guild.Channels.Any(chn => chn.Name == name.ToLowerInvariant())) {
                if (!await ctx.AskYesNoQuestionAsync("A channel with that name already exists. Continue anyway?").ConfigureAwait(false))
                    return;
            }

            if (parent != null && parent.Type != ChannelType.Category)
                throw new CommandFailedException("Channel parent must be a category!");

            await ctx.Guild.CreateTextChannelAsync(name, parent, nsfw: nsfw, reason: ctx.BuildReasonString())
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync($"Channel {Formatter.Bold(name)} successfully created.")
                .ConfigureAwait(false);
        }

        [Command("createtext"), Priority(1)]
        public Task CreateTextChannelAsync(CommandContext ctx,
                                          [Description("Name.")] string name,
                                          [Description("NSFW?")] bool nsfw = false,
                                          [Description("Parent category.")] DiscordChannel parent = null)
           => CreateTextChannelAsync(ctx, name, parent, nsfw);

        [Command("createtext"), Priority(0)]
        public Task CreateTextChannelAsync(CommandContext ctx,
                                          [Description("Parent category.")] DiscordChannel parent,
                                          [Description("Name.")] string name,
                                          [Description("NSFW?")] bool nsfw = false)
           => CreateTextChannelAsync(ctx, name, parent, nsfw);
        #endregion

        #region COMMAND_CHANNEL_CREATEVOICE
        [Command("createvoice"), Priority(2)]
        [Module(ModuleType.Administration)]
        [Description("Create new voice channel.")]
        [Aliases("createv", "cvoice", "cv", "+voice", "+v")]
        [UsageExample("!channel createtext \"My voice channel\" ParentCategory 0 96000")]
        [UsageExample("!channel createtext \"My voice channel\" 10 96000")]
        [UsageExample("!channel createtext ParentCategory \"My voice channel\" 10 96000")]
        [RequirePermissions(Permissions.ManageChannels)]
        [UsesInteractivity]
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

            if (ctx.Guild.Channels.Any(chn => chn.Name == name.ToLowerInvariant())) {
                if (!await ctx.AskYesNoQuestionAsync("A channel with that name already exists. Continue anyway?").ConfigureAwait(false))
                    return;
            }

            if (parent != null && parent.Type != ChannelType.Category)
                throw new CommandFailedException("Channel parent must be a category!");

            await ctx.Guild.CreateVoiceChannelAsync(name, parent, bitrate, userlimit, reason: ctx.BuildReasonString())
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync($"Channel {Formatter.Bold(name)} successfully created.")
                .ConfigureAwait(false);
        }


        [Command("createvoice"), Priority(1)]
        public Task CreateVoiceChannelAsync(CommandContext ctx,
                                           [Description("Name.")] string name,
                                           [Description("User limit.")] int? userlimit = null,
                                           [Description("Bitrate.")] int? bitrate = null,
                                           [Description("Parent category.")] DiscordChannel parent = null)
            => CreateVoiceChannelAsync(ctx, name, parent, userlimit, bitrate);

        [Command("createvoice"), Priority(0)]
        public Task CreateVoiceChannelAsync(CommandContext ctx,
                                           [Description("Parent category.")] DiscordChannel parent,
                                           [Description("Name.")] string name,
                                           [Description("User limit.")] int? userlimit = null,
                                           [Description("Bitrate.")] int? bitrate = null)
            => CreateVoiceChannelAsync(ctx, name, parent, userlimit, bitrate);
        #endregion

        #region COMMAND_CHANNEL_DELETE
        [Command("delete"), Priority(1)]
        [Module(ModuleType.Administration)]
        [Description("Delete a given channel or category. If the channel isn't given, deletes the current one.")]
        [Aliases("-", "del", "d", "remove", "rm")]
        [UsageExample("!channel delete")]
        [UsageExample("!channel delete \"My voice channel\"")]
        [UsageExample("!channel delete \"My voice channel\" Because I can!")]
        [RequirePermissions(Permissions.ManageChannels)]
        [UsesInteractivity]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Channel to delete.")] DiscordChannel channel = null,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            if (channel == null)
                channel = ctx.Channel;

            if (channel.Type == ChannelType.Category && channel.Children.Count() > 0) {
                if (await ctx.AskYesNoQuestionAsync("The channel specified is a non-empty category. Delete all channels in it recursively?")) {
                    foreach (var chn in channel.Children.ToList()) {
                        await chn.DeleteAsync(reason: ctx.BuildReasonString(reason))
                            .ConfigureAwait(false);
                    }
                }
            }

            if (!await ctx.AskYesNoQuestionAsync($"Are you sure you want to delete channel {channel.Mention}? This cannot be undone! (y/n)"))
                return;

            string name = channel.Name;
            await channel.DeleteAsync(reason: ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            if (channel.Id != ctx.Channel.Id)
                await ctx.RespondWithIconEmbedAsync($"Channel {Formatter.Bold(name)} successfully deleted.")
                    .ConfigureAwait(false);
        }

        [Command("delete"), Priority(0)]
        public Task DeleteAsync(CommandContext ctx,
                               [RemainingText, Description("Reason.")] string reason)
            => DeleteAsync(ctx, null, reason);
        #endregion

        #region COMMAND_CHANNEL_INFO
        [Command("info"), Module(ModuleType.Administration)]
        [Description("Get information about a given channel. If the channel isn't given, uses the current one.")]
        [Aliases("i", "information")]
        [UsageExample("!channel info")]
        [UsageExample("!channel info \"My voice channel\"")]
        [RequirePermissions(Permissions.AccessChannels)]
        public async Task InfoAsync(CommandContext ctx,
                                          [Description("Channel.")] DiscordChannel channel = null)
        {
            if (channel == null)
                channel = ctx.Channel;

            if (!ctx.Member.PermissionsIn(channel).HasFlag(Permissions.AccessChannels))
                throw new CommandFailedException("You are not allowed to see this channel! (nice try smartass)");

            var emb = new DiscordEmbedBuilder() {
                Title = "Information for: " + channel.ToString(),
                Description = "Current channel topic: " + (string.IsNullOrWhiteSpace(channel.Topic) ? "None." : Formatter.Italic(channel.Topic)),
                Color = DiscordColor.Goldenrod
            };
            emb.AddField("Type", channel.Type.ToString(), inline: true)
               .AddField("NSFW", channel.IsNSFW ? "Yes" : "No", inline: true)
               .AddField("Private", channel.IsPrivate ? "Yes" : "No", inline: true)
               .AddField("Position", channel.Position.ToString());
            if (channel.Type == ChannelType.Voice) {
                emb.AddField("Bitrate", channel.Bitrate.ToString(), inline: true)
                   .AddField("User limit", channel.UserLimit == 0 ? "No limit." : channel.UserLimit.ToString(), inline: true);
            }
            emb.AddField("Creation time", channel.CreationTimestamp.ToString(), inline: true);

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHANNEL_MODIFY
        [Command("modify"), Priority(1)]
        [Module(ModuleType.Administration)]
        [Description("Modify a given voice channel. Set 0 if you wish to keep the value as it is.")]
        [Aliases("edit", "mod", "m", "e")]
        [UsageExample("!channel modify \"My voice channel\" 20 96000 Some reason")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ModifyAsync(CommandContext ctx,
                                     [Description("Voice channel to edit")] DiscordChannel channel,
                                     [Description("User limit.")] int limit = 0,
                                     [Description("Bitrate.")] int bitrate = 0,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            if (channel == null)
                throw new InvalidCommandUsageException("Channel missing.");

            if (channel.Type != ChannelType.Voice)
                throw new InvalidCommandUsageException("That isn't a voice channel!");

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                if (limit > 0)
                    m.Userlimit = limit;
                if (channel.Type == ChannelType.Voice && bitrate > 0)
                    m.Bitrate = bitrate;
                m.AuditLogReason = ctx.BuildReasonString(reason);
            })).ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }

        [Command("modify"), Priority(0)]
        public Task ModifyAsync(CommandContext ctx,
                               [Description("User limit.")] int limit = 0,
                               [Description("Bitrate.")] int bitrate = 0,
                               [RemainingText, Description("Reason.")] string reason = null)
            => ModifyAsync(ctx, null, limit, bitrate, reason);
        #endregion

        #region COMMAND_CHANNEL_RENAME
        [Command("rename"), Priority(2)]
        [Module(ModuleType.Administration)]
        [Description("Rename channel. If the channel isn't given, uses the current one.")]
        [Aliases("r", "name", "setname")]
        [UsageExample("!channel rename New name for this channel")]
        [UsageExample("!channel rename \"My voice channel\" \"My old voice channel\"")]
        [UsageExample("!channel rename \"My reason\" \"My voice channel\" \"My old voice channel\"")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task RenameAsync(CommandContext ctx,
                                     [Description("Reason.")] string reason,
                                     [Description("Channel to rename.")] DiscordChannel channel,
                                     [RemainingText, Description("New name.")] string newname)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("Missing new channel name.");

            if (newname.Length < 2 || newname.Length > 100)
                throw new InvalidCommandUsageException("Name must be longer than 2 and shorter than 100 characters.");

            if (channel == null)
                channel = ctx.Channel;

            if (channel.Type == ChannelType.Text && newname.Contains(' '))
                throw new InvalidCommandUsageException("Name cannot contain spaces for a text channel.");

            try {
                await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                    m.Name = newname;
                    m.AuditLogReason = ctx.BuildReasonString(reason);
                })).ConfigureAwait(false);
            } catch (BadRequestException e) {
                throw new CommandFailedException("An error occured. Maybe the name entered contains invalid characters?", e);
            }

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }

        [Command("rename"), Priority(1)]
        public Task RenameAsync(CommandContext ctx,
                               [Description("Channel to rename.")] DiscordChannel channel,
                               [RemainingText, Description("New name.")] string newname)
            => RenameAsync(ctx, null, channel, newname);

        [Command("rename"), Priority(0)]
        public Task RenameAsync(CommandContext ctx,
                               [RemainingText, Description("New name.")] string newname)
            => RenameAsync(ctx, null, null, newname);
        #endregion

        #region COMMAND_CHANNEL_SETPARENT
        [Command("setparent"), Priority(1)]
        [Module(ModuleType.Administration)]
        [Description("Change the parent of the given channel. If the channel isn't given, uses the current one.")]
        [Aliases("setpar", "par", "parent")]
        [UsageExample("!channel setparent \"My channel\" ParentCategory")]
        [UsageExample("!channel setparent ParentCategory I set a new parent for this channel!")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ChangeParentAsync(CommandContext ctx,
                                           [Description("Child channel.")] DiscordChannel channel,
                                           [Description("Parent category.")] DiscordChannel parent,
                                           [RemainingText, Description("Reason.")] string reason = null)
        {
            if (parent.Type != ChannelType.Category)
                throw new CommandFailedException("Parent channel must be a category.");

            if (channel == null)
                channel = ctx.Channel;

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Parent = parent;
                m.AuditLogReason = ctx.BuildReasonString(reason);
            })).ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }

        [Command("setparent"), Priority(0)]
        public Task ChangeParentAsync(CommandContext ctx,
                                     [Description("Parent category.")] DiscordChannel parent,
                                     [RemainingText, Description("Reason.")] string reason = null)
            => ChangeParentAsync(ctx, null, parent, reason);
        #endregion

        #region COMMAND_CHANNEL_SETPOSITION
        [Command("setposition"), Priority(2)]
        [Module(ModuleType.Administration)]
        [Description("Change the position of the given channel in the guild channel list. If the channel isn't given, uses the current one.")]
        [Aliases("setpos", "pos", "position")]
        [UsageExample("!channel setposition 4")]
        [UsageExample("!channel setposition \"My channel\" 1")]
        [UsageExample("!channel setposition \"My channel\" 4 I changed position :)")]
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

            await channel.ModifyPositionAsync(position, ctx.BuildReasonString(reason))
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }

        [Command("setposition"), Priority(1)]
        public Task ReorderChannelAsync(CommandContext ctx,
                                       [Description("Position.")] int position,
                                       [Description("Channel to reorder.")] DiscordChannel channel,
                                       [RemainingText, Description("Reason.")] string reason = null)
            => ReorderChannelAsync(ctx, channel, position, reason);

        [Command("setposition"), Priority(0)]
        public Task ReorderChannelAsync(CommandContext ctx,
                                       [Description("Position.")] int position,
                                       [RemainingText, Description("Reason.")] string reason = null)
            => ReorderChannelAsync(ctx, null, position, reason);
        #endregion

        #region COMMAND_CHANNEL_SETTOPIC
        [Command("settopic"), Priority(2)]
        [Module(ModuleType.Administration)]
        [Description("Set channel topic. If the channel isn't given, uses the current one.")]
        [Aliases("t", "topic", "sett")]
        [UsageExample("!channel settopic New channel topic")]
        [UsageExample("!channel settopic \"My channel\" New channel topic")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task SetChannelTopicAsync(CommandContext ctx,
                                              [Description("Reason.")] string reason,
                                              [Description("Channel.")] DiscordChannel channel,
                                              [RemainingText, Description("New topic.")] string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new InvalidCommandUsageException("Missing topic.");
            if (channel == null)
                channel = ctx.Channel;

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Topic = topic;
                m.AuditLogReason = ctx.BuildReasonString();
            })).ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }

        [Command("settopic"), Priority(1)]
        public Task SetChannelTopicAsync(CommandContext ctx,
                                        [Description("Channel.")] DiscordChannel channel,
                                        [RemainingText, Description("New Topic.")] string topic)
            => SetChannelTopicAsync(ctx, null, channel, topic);

        [Command("settopic"), Priority(0)]
        public Task SetChannelTopicAsync(CommandContext ctx,
                                        [RemainingText, Description("New Topic.")] string topic)
            => SetChannelTopicAsync(ctx, null, null, topic);
        #endregion

        #region COMMAND_CHANNEL_VIEWPERMS
        [Command("viewperms"), Priority(3)]
        [Module(ModuleType.Administration)]
        [Description("View permissions for a member or role in the given channel. If the member is not given, lists the sender's permissions. If the channel is not given, uses current one.")]
        [Aliases("tp", "perms", "permsfor", "testperms", "listperms")]
        [UsageExample("!channel viewperms @Someone")]
        [UsageExample("!channel viewperms Admins")]
        [UsageExample("!channel viewperms #private everyone")]
        [UsageExample("!channel viewperms everyone #private")]
        public async Task PrintPermsAsync(CommandContext ctx,
                                         [Description("Member.")] DiscordMember member = null,
                                         [Description("Channel.")] DiscordChannel channel = null)
        {
            if (member == null)
                member = ctx.Member;

            if (channel == null)
                channel = ctx.Channel;

            string perms = $"{Formatter.Bold(member.DisplayName)} cannot access channel {Formatter.Bold(channel.Name)}.";
            if (member.PermissionsIn(channel).HasPermission(Permissions.AccessChannels))
                perms = member.PermissionsIn(channel).ToPermissionString();

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Permissions for member {member.Username} in channel {channel.Name}:",
                Description = perms,
                Color = DiscordColor.Turquoise
            }.Build()).ConfigureAwait(false);
        }

        [Command("viewperms"), Priority(2)]
        public Task PrintPermsAsync(CommandContext ctx,
                                   [Description("Channel.")] DiscordChannel channel,
                                   [Description("Member.")] DiscordMember member = null)
            => PrintPermsAsync(ctx, member, channel);

        [Command("viewperms"), Priority(1)]
        public async Task PrintPermsAsync(CommandContext ctx,
                                         [Description("Role.")] DiscordRole role,
                                         [Description("Channel.")] DiscordChannel channel = null)
        {
            if (!ctx.Member.Roles.Any(r => r.Position >= role.Position))
                throw new CommandFailedException("You cannot view permissions for roles which have position above your highest role position.");

            if (channel == null)
                channel = ctx.Channel;

            DiscordOverwrite overwrite = null;
            foreach (var o in ctx.Channel.PermissionOverwrites) {
                var r = await o.GetRoleAsync()
                    .ConfigureAwait(false);
                if (r.Id == role.Id) {
                    overwrite = o;
                    break;
                }
            }

            var emb = new DiscordEmbedBuilder() {
                Title = $"Permissions for role {role.Name} in channel {channel.Name}:",
                Color = DiscordColor.Turquoise
            };

            if (overwrite != null) {
                emb.AddField("Allowed", overwrite.Allowed.ToPermissionString())
                   .AddField("Denied", overwrite.Denied.ToPermissionString());
            } else {
                emb.AddField("No overwrites found, listing default role permissions:\n", role.Permissions.ToPermissionString());
            }

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }

        [Command("viewperms"), Priority(0)]
        public Task PrintPermsAsync(CommandContext ctx,
                                   [Description("Channel.")] DiscordChannel channel,
                                   [Description("Role.")] DiscordRole role)
            => PrintPermsAsync(ctx, role, channel);
        #endregion
    }
}
