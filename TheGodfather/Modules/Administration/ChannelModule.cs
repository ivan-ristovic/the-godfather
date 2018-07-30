#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("channel"), Module(ModuleType.Administration), NotBlocked]
    [Description("Channel administration. Group call prints channel information.")]
    [Aliases("channels", "chn", "ch", "c")]
    [UsageExamples("!channel", 
                   "!channel #general")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class ChannelModule : TheGodfatherModule
    {

        public ChannelModule(DBService db) 
            : base(db: db)
        {
            this.ModuleColor = DiscordColor.Turquoise;
        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Channel to scan.")] DiscordChannel channel = null)
            => InfoAsync(ctx, channel);


        #region COMMAND_CHANNEL_CREATECATEGORY
        [Command("createcategory"), Module(ModuleType.Administration), UsesInteractivity]
        [Description("Create new channel category.")]
        [Aliases("addcategory", "createcat", "createc", "ccat", "cc", "+category", "+cat", "+c", "<c", "<<c")]
        [UsageExamples("!channel createcategory My New Category")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateCategoryAsync(CommandContext ctx,
                                             [RemainingText, Description("Name for the category.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing category name.");
            
            if (name.Length > 100)
                throw new InvalidCommandUsageException("Channel name must be shorter than 100 characters.");

            if (ctx.Guild.Channels.Any(chn => chn.Name == name.ToLowerInvariant())) {
                if (!await ctx.WaitForBoolReplyAsync("A category with that name already exists. Continue? (y/n)"))
                    return;
            }

            await ctx.Guild.CreateChannelCategoryAsync(name, reason: ctx.BuildReasonString());
            await ctx.InformSuccessAsync();
        }
        #endregion

        #region COMMAND_CHANNEL_CREATETEXT
        [Command("createtext"), Priority(2), Module(ModuleType.Administration), UsesInteractivity]
        [Description("Create new text channel. You can also specify channel parent, user limit and bitrate.")]
        [Aliases("addtext", "addtxt", "createtxt", "createt", "ctxt", "ct", "+", "+txt", "+t", "<t", "<<t")]
        [UsageExamples("!channel addtext newtextchannel ParentCategory no",
                       "!channel addtext newtextchannel no",
                       "!channel addtext ParentCategory newtextchannel")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateTextChannelAsync(CommandContext ctx,
                                                [Description("Name for the channel.")] string name,
                                                [Description("Parent category.")] DiscordChannel parent = null,
                                                [Description("NSFW?")] bool nsfw = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing channel name.");

            if (name.Contains(' '))
                throw new InvalidCommandUsageException("Text channel name cannot contain spaces.");
            
            if (name.Length > 100)
                throw new InvalidCommandUsageException("Channel name must be shorter than 100 characters.");

            if (parent != null && parent.Type != ChannelType.Category)
                throw new CommandFailedException("Channel parent must be a category!");
            
            if (ctx.Guild.Channels.Any(chn => string.Compare(name, chn.Name, true) == 0)) {
                if (!await ctx.WaitForBoolReplyAsync("A channel with that name already exists. Continue? (y/n)"))
                    return;
            }

            await ctx.Guild.CreateTextChannelAsync(name, parent, nsfw: nsfw, reason: ctx.BuildReasonString());
            await ctx.InformSuccessAsync();
        }

        [Command("createtext"), Priority(1)]
        public Task CreateTextChannelAsync(CommandContext ctx,
                                          [Description("Name for the channel.")] string name,
                                          [Description("NSFW?")] bool nsfw = false,
                                          [Description("Parent category.")] DiscordChannel parent = null)
           => CreateTextChannelAsync(ctx, name, parent, nsfw);

        [Command("createtext"), Priority(0)]
        public Task CreateTextChannelAsync(CommandContext ctx,
                                          [Description("Parent category.")] DiscordChannel parent,
                                          [Description("Name for the channel.")] string name,
                                          [Description("NSFW?")] bool nsfw = false)
           => CreateTextChannelAsync(ctx, name, parent, nsfw);
        #endregion

        #region COMMAND_CHANNEL_CREATEVOICE
        [Command("createvoice"), Priority(2), Module(ModuleType.Administration), UsesInteractivity]
        [Description("Create new voice channel. You can also specify channel parent, user limit and bitrate.")]
        [Aliases("addvoice", "addv", "createv", "cvoice", "cv", "+voice", "+v", "<v", "<<v")]
        [UsageExamples("!channel createtext \"My voice channel\" ParentCategory 0 96000",
                       "!channel createtext \"My voice channel\" 10 96000",
                       "!channel createtext ParentCategory \"My voice channel\" 10 96000")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateVoiceChannelAsync(CommandContext ctx,
                                                 [Description("Name for the channel.")] string name,
                                                 [Description("Parent category.")] DiscordChannel parent = null,
                                                 [Description("User limit.")] int? userlimit = null,
                                                 [Description("Bitrate.")] int? bitrate = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing channel name.");
            
            if (name.Length > 100)
                throw new InvalidCommandUsageException("Name must be shorter than 100 characters.");

            if (parent != null && parent.Type != ChannelType.Category)
                throw new CommandFailedException("Channel parent must be a category!");
            
            if (ctx.Guild.Channels.Any(chn => string.Compare(name, chn.Name, true) == 0)) {
                if (!await ctx.WaitForBoolReplyAsync("A channel with that name already exists. Continue? (y/n)"))
                    return;
            }

            await ctx.Guild.CreateVoiceChannelAsync(name, parent, bitrate, userlimit, reason: ctx.BuildReasonString());
            await ctx.InformSuccessAsync();
        }


        [Command("createvoice"), Priority(1)]
        public Task CreateVoiceChannelAsync(CommandContext ctx,
                                           [Description("Name for the channel.")] string name,
                                           [Description("User limit.")] int? userlimit = null,
                                           [Description("Bitrate.")] int? bitrate = null,
                                           [Description("Parent category.")] DiscordChannel parent = null)
            => CreateVoiceChannelAsync(ctx, name, parent, userlimit, bitrate);

        [Command("createvoice"), Priority(0)]
        public Task CreateVoiceChannelAsync(CommandContext ctx,
                                           [Description("Parent category.")] DiscordChannel parent,
                                           [Description("Name for the channel.")] string name,
                                           [Description("User limit.")] int? userlimit = null,
                                           [Description("Bitrate.")] int? bitrate = null)
            => CreateVoiceChannelAsync(ctx, name, parent, userlimit, bitrate);
        #endregion

        #region COMMAND_CHANNEL_DELETE
        [Command("delete"), Priority(1), Module(ModuleType.Administration), UsesInteractivity]
        [Description("Delete a given channel or category. If the channel isn't given, deletes the current one. " +
                     "You can also specify reason for deletion.")]
        [Aliases("-", "del", "d", "remove", "rm")]
        [UsageExamples("!channel delete", 
                       "!channel delete \"My voice channel\"", 
                       "!channel delete \"My voice channel\" Because I can!")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Channel to delete.")] DiscordChannel channel = null,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            if (channel == null)
                channel = ctx.Channel;

            if (channel.Type == ChannelType.Category && channel.Children.Any()) {
                if (await ctx.WaitForBoolReplyAsync("The channel specified is a non-empty category and deleting it will delete child channels as well. Continue? (y/n)")) {
                    foreach (DiscordChannel child in channel.Children.ToList())
                        await child.DeleteAsync(reason: ctx.BuildReasonString(reason));
                }
            } else {
                if (!await ctx.WaitForBoolReplyAsync($"Are you sure you want to delete channel {Formatter.Bold(channel.Name)} (ID: {Formatter.InlineCode(channel.Id.ToString())})? This cannot be undone! (y/n)"))
                    return;
            }

            await channel.DeleteAsync(reason: ctx.BuildReasonString(reason));
            if (channel.Id != ctx.Channel.Id)
                await ctx.InformSuccessAsync();
        }

        [Command("delete"), Priority(0)]
        public Task DeleteAsync(CommandContext ctx,
                               [RemainingText, Description("Reason.")] string reason)
            => DeleteAsync(ctx, null, reason);
        #endregion

        #region COMMAND_CHANNEL_INFO
        [Command("info"), Module(ModuleType.Administration)]
        [Description("Print information about a given channel. If the channel is not given, uses the current one.")]
        [Aliases("i", "information")]
        [UsageExamples("!channel info", 
                       "!channel info \"My voice channel\"")]
        public Task InfoAsync(CommandContext ctx,
                             [Description("Channel.")] DiscordChannel channel = null)
        {
            if (channel == null)
                channel = ctx.Channel;
            
            if (!ctx.Member.PermissionsIn(channel).HasPermission(Permissions.AccessChannels))
                throw new CommandFailedException("You are not allowed to see this channel! (You thought you are smart, right?)");

            var emb = new DiscordEmbedBuilder() {
                Title = channel.ToString(),
                Description = $"Current channel topic: {Formatter.Italic(string.IsNullOrWhiteSpace(channel.Topic) ? "None" : channel.Topic)}",
                Color = this.ModuleColor
            };

            emb.AddField("Type", channel.Type.ToString(), inline: true);
            emb.AddField("NSFW", channel.IsNSFW.ToString(), inline: true);
            emb.AddField("Private", channel.IsPrivate.ToString(), inline: true);
            emb.AddField("Position", channel.Position.ToString(), inline: true);
            if (channel.Type == ChannelType.Voice) {
                emb.AddField("Bitrate", channel.Bitrate.ToString(), inline: true);
                emb.AddField("User limit", channel.UserLimit == 0 ? "No limit." : channel.UserLimit.ToString(), inline: true);
            }
            emb.AddField("Creation time", channel.CreationTimestamp.ToString(), inline: true);

            return ctx.RespondAsync(embed: emb.Build());
        }
        #endregion

        #region COMMAND_CHANNEL_MODIFY
        [Command("modify"), Priority(1), Module(ModuleType.Administration)]
        [Description("Modify a given voice channel. Give 0 as an argument if you wish to keep the value unchanged.")]
        [Aliases("edit", "mod", "m", "e")]
        [UsageExamples("!channel modify \"My voice channel\" 20 96000 Some reason")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ModifyAsync(CommandContext ctx,
                                     [Description("Voice channel to edit")] DiscordChannel channel,
                                     [Description("User limit.")] int limit = 0,
                                     [Description("Bitrate.")] int bitrate = 0,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            if (channel.Type != ChannelType.Voice)
                throw new InvalidCommandUsageException("You can only modify voice channels!");

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                if (limit > 0)
                    m.Userlimit = limit;
                if (bitrate > 0)
                    m.Bitrate = bitrate;
                m.AuditLogReason = ctx.BuildReasonString(reason);
            })).ConfigureAwait(false);

            await ctx.InformSuccessAsync();
        }

        [Command("modify"), Priority(0)]
        public Task ModifyAsync(CommandContext ctx,
                               [Description("User limit.")] int limit = 0,
                               [Description("Bitrate.")] int bitrate = 0,
                               [RemainingText, Description("Reason.")] string reason = null)
            => ModifyAsync(ctx, null, limit, bitrate, reason);
        #endregion

        #region COMMAND_CHANNEL_RENAME
        [Command("rename"), Priority(2), Module(ModuleType.Administration)]
        [Description("Rename given channel. If the channel is not given, renames the current one.")]
        [Aliases("r", "name", "setname", "rn")]
        [UsageExamples("!channel rename New name for this channel",
                       "!channel rename \"My voice channel\" \"My old voice channel\"",
                       "!channel rename \"My reason\" \"My voice channel\" \"My old voice channel\"")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task RenameAsync(CommandContext ctx,
                                     [Description("Reason.")] string reason,
                                     [Description("Channel to rename.")] DiscordChannel channel,
                                     [RemainingText, Description("New name.")] string newname)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("Missing new channel name.");

            if (newname.Length < 2 || newname.Length > 100)
                throw new InvalidCommandUsageException("Channel name must be longer than 2 and shorter than 100 characters.");

            if (channel == null)
                channel = ctx.Channel;

            if (channel.Type == ChannelType.Text && newname.Contains(' '))
                throw new InvalidCommandUsageException("Text channel name cannot contain spaces.");
            
            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Name = newname;
                m.AuditLogReason = ctx.BuildReasonString(reason);
            }));
            await ctx.InformSuccessAsync();
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
        [Command("setparent"), Priority(1), Module(ModuleType.Administration)]
        [Description("Change the given channel's parent. If the channel is not given, uses the current one. " +
                     "You can also provide a reason.")]
        [Aliases("setpar", "par", "parent")]
        [UsageExamples("!channel setparent \"My channel\" ParentCategory",
                       "!channel setparent ParentCategory I set a new parent for this channel!")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ChangeParentAsync(CommandContext ctx,
                                           [Description("Child channel.")] DiscordChannel channel,
                                           [Description("Parent category.")] DiscordChannel parent,
                                           [RemainingText, Description("Reason.")] string reason = null)
        {
            if (parent.Type != ChannelType.Category)
                throw new CommandFailedException("Parent must be a category.");

            if (channel == null)
                channel = ctx.Channel;

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Parent = parent;
                m.AuditLogReason = ctx.BuildReasonString(reason);
            }));
            await ctx.InformSuccessAsync();
        }

        [Command("setparent"), Priority(0)]
        public Task ChangeParentAsync(CommandContext ctx,
                                     [Description("Parent category.")] DiscordChannel parent,
                                     [RemainingText, Description("Reason.")] string reason = null)
            => ChangeParentAsync(ctx, null, parent, reason);
        #endregion

        #region COMMAND_CHANNEL_SETPOSITION
        [Command("setposition"), Priority(2), Module(ModuleType.Administration)]
        [Description("Change the position of the given channel in the guild channel list. If the channel " +
                     "is not given, repositions the current one. You can also provide reason.")]
        [Aliases("setpos", "pos", "position")]
        [UsageExamples("!channel setposition 4", 
                       "!channel setposition \"My channel\" 1",
                       "!channel setposition \"My channel\" 4 I changed position :)")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ReorderChannelAsync(CommandContext ctx,
                                             [Description("Channel to reposition.")] DiscordChannel channel,
                                             [Description("New position.")] int position,
                                             [RemainingText, Description("Reason.")] string reason = null)
        {
            if (position < 0)
                throw new ArgumentException("Position cannot be negative.", nameof(position));

            if (channel == null)
                channel = ctx.Channel;

            await channel.ModifyPositionAsync(position, ctx.BuildReasonString(reason));
            await ctx.InformSuccessAsync();
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
        [Command("settopic"), Priority(2), Module(ModuleType.Administration)]
        [Description("Set channel topic. If the channel is not given, uses the current one.")]
        [Aliases("t", "topic", "sett")]
        [UsageExamples("!channel settopic New channel topic", 
                       "!channel settopic \"My channel\" New channel topic")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task SetChannelTopicAsync(CommandContext ctx,
                                              [Description("Reason.")] string reason,
                                              [Description("Channel.")] DiscordChannel channel,
                                              [RemainingText, Description("New topic.")] string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new InvalidCommandUsageException("Missing topic.");

            if (topic.Length > 1024)
                throw new InvalidCommandUsageException("Topic cannot exceed 1024 characters!");

            if (channel == null)
                channel = ctx.Channel;

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Topic = topic;
                m.AuditLogReason = ctx.BuildReasonString();
            }));
            await ctx.InformSuccessAsync();
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
        [Command("viewperms"), Priority(3), Module(ModuleType.Administration)]
        [Description("View permissions for a member or role in the given channel. If the member is not " +
                     "given, lists the sender's permissions. If the channel is not given, uses the current one.")]
        [Aliases("tp", "perms", "permsfor", "testperms", "listperms")]
        [UsageExamples("!channel viewperms @Someone",
                       "!channel viewperms Admins",
                       "!channel viewperms #private everyone",
                       "!channel viewperms everyone #private")]
        [RequireBotPermissions(Permissions.Administrator)]
        public Task PrintPermsAsync(CommandContext ctx,
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

            return ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Permissions for {member.ToString()} in {channel.ToString()}:",
                Description = perms,
                Color = this.ModuleColor
            }.Build());
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
            foreach (DiscordOverwrite o in channel.PermissionOverwrites.Where(o => o.Type == OverwriteType.Role)) {
                DiscordRole r = await o.GetRoleAsync();
                if (r.Id == role.Id) {
                    overwrite = o;
                    break;
                }
            }

            var emb = new DiscordEmbedBuilder() {
                Title = $"Permissions for {role.ToString()} in {channel.ToString()}:",
                Color = this.ModuleColor
            };

            if (overwrite != null) {
                emb.AddField("Allowed", overwrite.Allowed.ToPermissionString())
                   .AddField("Denied", overwrite.Denied.ToPermissionString());
            } else {
                emb.AddField("No overwrites found, listing default role permissions:\n", role.Permissions.ToPermissionString());
            }

            await ctx.RespondAsync(embed: emb.Build());
        }

        [Command("viewperms"), Priority(0)]
        public Task PrintPermsAsync(CommandContext ctx,
                                   [Description("Channel.")] DiscordChannel channel,
                                   [Description("Role.")] DiscordRole role)
            => PrintPermsAsync(ctx, role, channel);
        #endregion
    }
}
