using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration
{
    [Group("channel"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("channels", "chn", "ch", "c")]
    [RequireGuild, RequirePermissions(Permissions.ManageChannels)]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed partial class ChannelModule : TheGodfatherModule
    {
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-chn-info")] DiscordChannel? channel = null)
            => this.InfoAsync(ctx, channel);


        #region channel add
        [Group("add")]
        [Aliases("create", "cr", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public sealed class ChannelAddModule : TheGodfatherModule
        {
            #region category
            [Command("category"), UsesInteractivity]
            [Aliases("addcategory", "cat", "c", "cc", "+category", "+cat", "+c", "<c", "<<c")]
            public async Task CreateTextChannelAsync(CommandContext ctx,
                                                    [RemainingText, Description("desc-chn-cat-name")] string name)
            {
                await CheckPotentialChannelNameAsync(ctx, name);
                DiscordChannel c = await ctx.Guild.CreateChannelCategoryAsync(name, reason: ctx.BuildInvocationDetailsString());
                await ctx.InfoAsync(this.ModuleColor, "fmt-chn-category", Formatter.Bold(c.Name));
            }
            #endregion

            #region text
            [Command("text"), Priority(0)]
            [Aliases("addtext", "addtxt", "txt", "ctxt", "ct", "+", "+txt", "+t", "<t", "<<t")]
            public Task CreateTextChannelAsync(CommandContext ctx,
                                              [Description("desc-chn-parent")] DiscordChannel parent,
                                              [Description("desc-chn-name")] string name,
                                              [Description("desc-chn-nsfw")] bool nsfw = false)
               => this.CreateTextChannelAsync(ctx, name, parent, nsfw);

            [Command("text"), Priority(1)]
            public Task CreateTextChannelAsync(CommandContext ctx,
                                              [Description("desc-chn-name")] string name,
                                              [Description("desc-chn-nsfw")] bool nsfw = false,
                                              [Description("desc-chn-parent")] DiscordChannel? parent = null)
               => this.CreateTextChannelAsync(ctx, name, parent, nsfw);

            [Command("text"), Priority(2), UsesInteractivity]
            public async Task CreateTextChannelAsync(CommandContext ctx,
                                                    [Description("desc-chn-name")] string name,
                                                    [Description("desc-chn-parent")] DiscordChannel? parent = null,
                                                    [Description("desc-chn-nsfw")] bool nsfw = false)
            {
                await CheckPotentialChannelNameAsync(ctx, name, true, true);

                if (parent is { } && parent.Type != ChannelType.Category)
                    throw new CommandFailedException(ctx, "cmd-err-chn-parent");

                DiscordChannel c = await ctx.Guild.CreateTextChannelAsync(name, parent, nsfw: nsfw, reason: ctx.BuildInvocationDetailsString());
                await ctx.InfoAsync(this.ModuleColor, "fmt-chn-text", c.Mention);
            }
            #endregion

            #region voice
            [Command("voice"), Priority(2), UsesInteractivity]
            [Aliases("addvoice", "addv", "cvoice", "cv", "+voice", "+v", "<v", "<<v")]
            public async Task CreateVoiceChannelAsync(CommandContext ctx,
                                                     [Description("desc-chn-voice-name")] string name,
                                                     [Description("desc-chn-parent")] DiscordChannel? parent = null,
                                                     [Description("desc-chn-userlimit")] int? userlimit = null,
                                                     [Description("desc-chn-bitrate")] int? bitrate = null)
            {
                if (userlimit is { } && (userlimit < 1 || userlimit > DiscordLimits.VoiceChannelUserLimit))
                    throw new InvalidCommandUsageException(ctx, "cmd-err-chn-userlimit", 1, DiscordLimits.VoiceChannelUserLimit);
                if (bitrate is { } && (bitrate < DiscordLimits.VoiceChannelMinBitrate || bitrate > DiscordLimits.VoiceChannelMaxBitrate))
                    throw new InvalidCommandUsageException(ctx, "cmd-err-chn-bitrate", DiscordLimits.VoiceChannelMinBitrate, DiscordLimits.VoiceChannelMinBitrate);
                await CheckPotentialChannelNameAsync(ctx, name);
                DiscordChannel c = await ctx.Guild.CreateVoiceChannelAsync(name, parent, bitrate, userlimit, reason: ctx.BuildInvocationDetailsString());
                await ctx.InfoAsync(this.ModuleColor, "fmt-chn-voice", Formatter.Bold(c.Name));
            }

            [Command("voice"), Priority(1)]
            public Task CreateVoiceChannelAsync(CommandContext ctx,
                                               [Description("desc-chn-voice-name")] string name,
                                               [Description("desc-chn-userlimit")] int? userlimit = null,
                                               [Description("desc-chn-bitrate")] int? bitrate = null,
                                               [Description("desc-chn-parent")] DiscordChannel? parent = null)
                => this.CreateVoiceChannelAsync(ctx, name, parent, userlimit, bitrate);

            [Command("voice"), Priority(0)]
            public Task CreateVoiceChannelAsync(CommandContext ctx,
                                               [Description("desc-chn-parent")] DiscordChannel parent,
                                               [Description("desc-chn-voice-name")] string name,
                                               [Description("desc-chn-userlimit")] int? userlimit = null,
                                               [Description("desc-chn-bitrate")] int? bitrate = null)
                => this.CreateVoiceChannelAsync(ctx, name, parent, userlimit, bitrate);
            #endregion
        }
        #endregion

        #region channel clone
        [Command("clone"), Priority(1), UsesInteractivity]
        [Aliases("copy", "cp", "cln")]
        public async Task CloneAsync(CommandContext ctx,
                                    [Description("desc-chn-clone")] DiscordChannel channel,
                                    [RemainingText, Description("desc-chn-clone-name")] string? name = null)
        {
            DiscordChannel cloned = await channel.CloneAsync(ctx.BuildInvocationDetailsString());

            await CheckPotentialChannelNameAsync(ctx, name, false, channel.Type == ChannelType.Text);
            if (!string.IsNullOrWhiteSpace(name)) 
                await cloned.ModifyAsync(new Action<ChannelEditModel>(m => m.Name = name));

            await ctx.InfoAsync(this.ModuleColor, "fmt-chn-clone", Formatter.Bold(channel.Name), Formatter.Bold(cloned.Name));
        }

        [Command("clone")]
        public Task CloneAsync(CommandContext ctx,
                              [RemainingText, Description("desc-chn-clone-name")] string? name = null)
            => this.CloneAsync(ctx, ctx.Channel, name);
        #endregion

        #region channel delete
        [Command("delete"), Priority(1), UsesInteractivity]
        [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("desc-chn-delete")] DiscordChannel channel,
                                     [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            channel ??= ctx.Channel;
            reason = ctx.BuildInvocationDetailsString(reason);

            if (channel.Type == ChannelType.Category && channel.Children.Any()) {
                if (await ctx.WaitForBoolReplyAsync("q-chn-cat-del")) {
                    foreach (DiscordChannel child in channel.Children) {
                        await child.DeleteAsync(reason);
                        await Task.Delay(250);
                    }
                }
            } else {
                if (!await ctx.WaitForBoolReplyAsync("q-chn-del", args: new object[] { Formatter.Bold(channel.Name), channel.Id }))
                    return;
            }

            await channel.DeleteAsync(reason);
            if (channel != ctx.Channel)
                await ctx.InfoAsync(this.ModuleColor, "fmt-chn-del", channel.Id);
        }

        [Command("delete"), Priority(0)]
        public Task DeleteAsync(CommandContext ctx,
                               [RemainingText, Description("desc-rsn")] string? reason)
            => this.DeleteAsync(ctx, ctx.Channel, reason);
        #endregion

        #region channel info
        [Command("info")]
        [Aliases("information", "details", "about", "i")]
        public Task InfoAsync(CommandContext ctx,
                             [Description("desc-chn-info")] DiscordChannel? channel = null)
        {
            channel ??= ctx.Channel;

            if (ctx.Channel.IsPrivate || !ctx.Member.PermissionsIn(channel).HasPermission(Permissions.AccessChannels))
                throw new CommandFailedException(ctx, "cmd-err-chn-perms");

            var emb = new LocalizedEmbedBuilder(ctx.Services.GetRequiredService<LocalizationService>(), ctx.Guild.Id);
            emb.WithTitle(channel.ToString());
            emb.WithColor(this.ModuleColor);
            if (!string.IsNullOrWhiteSpace(channel.Topic))
                emb.WithDescription(Formatter.Italic(Formatter.Strip(channel.Topic)));
            emb.AddLocalizedTitleField("str-type", channel.Type, inline: true);
            emb.AddLocalizedTitleField("str-nsfw", channel.IsNSFW, inline: true);
            emb.AddLocalizedTitleField("str-pos", channel.Position, inline: true);
            emb.AddLocalizedTitleField("str-ratelimit", channel.PerUserRateLimit, inline: true, unknown: false);
            if (channel.Type == ChannelType.Voice) {
                emb.AddLocalizedTitleField("str-bitrate", channel.Bitrate, inline: true);
                if (channel.UserLimit > 0)
                    emb.AddLocalizedTitleField("str-user-limit", channel.UserLimit, inline: true);
            }
            emb.AddLocalizedTimestampField("str-created-at", channel.CreationTimestamp, inline: true);

            return ctx.RespondAsync(embed: emb.Build());
        }
        #endregion

        #region COMMAND_CHANNEL_MODIFY
        [Command("modify"), Priority(1)]
        [Description("Modify a given voice channel. Give 0 as an argument if you wish to keep the value unchanged.")]
        [Aliases("edit", "mod", "m", "e")]
        public async Task ModifyAsync(CommandContext ctx,
                                     [Description("Voice channel to edit")] DiscordChannel channel,
                                     [Description("User limit.")] int limit = 0,
                                     [Description("Bitrate.")] int bitrate = 0,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            if (channel.Type != ChannelType.Voice)
                throw new InvalidCommandUsageException("You can only modify voice channels!");

            if (limit == 0 && bitrate == 0)
                throw new InvalidCommandUsageException("You need to specify atleast one change for either bitrate or user limit.");

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                if (limit > 0)
                    m.Userlimit = limit;
                if (bitrate > 0)
                    m.Bitrate = bitrate;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            })).ConfigureAwait(false);

            await this.InformAsync(ctx, $"Successfully modified voice channel {Formatter.Bold(channel.Name)}:\n\nUser limit: {(limit > 0 ? limit.ToString() : "Not changed")}\nBitrate: {(bitrate > 0 ? bitrate.ToString() : "Not changed")}", important: false);
        }

        [Command("modify"), Priority(0)]
        public Task ModifyAsync(CommandContext ctx,
                               [Description("User limit.")] int limit = 0,
                               [Description("Bitrate.")] int bitrate = 0,
                               [RemainingText, Description("Reason.")] string reason = null)
            => this.ModifyAsync(ctx, null, limit, bitrate, reason);
        #endregion

        #region COMMAND_CHANNEL_RENAME
        [Command("rename"), Priority(2)]
        [Description("Rename given channel. If the channel is not given, renames the current one.")]
        [Aliases("r", "name", "setname", "rn")]

        public async Task RenameAsync(CommandContext ctx,
                                     [Description("Reason.")] string reason,
                                     [Description("Channel to rename.")] DiscordChannel channel,
                                     [RemainingText, Description("New name.")] string newname)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("Missing new channel name.");

            if (newname.Length < 2 || newname.Length > 100)
                throw new InvalidCommandUsageException("Channel name must be longer than 2 and shorter than 100 characters.");

            channel = channel ?? ctx.Channel;

            if (channel.Type == ChannelType.Text && newname.Contains(' '))
                throw new InvalidCommandUsageException("Text channel name cannot contain spaces.");

            string name = channel.Name;
            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Name = newname;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await this.InformAsync(ctx, $"Successfully renamed channel {Formatter.Bold(name)} to {Formatter.Bold(channel.Name)}", important: false);
        }

        [Command("rename"), Priority(1)]
        public Task RenameAsync(CommandContext ctx,
                               [Description("Channel to rename.")] DiscordChannel channel,
                               [RemainingText, Description("New name.")] string newname)
            => this.RenameAsync(ctx, null, channel, newname);

        [Command("rename"), Priority(0)]
        public Task RenameAsync(CommandContext ctx,
                               [RemainingText, Description("New name.")] string newname)
            => this.RenameAsync(ctx, null, null, newname);
        #endregion

        #region COMMAND_CHANNEL_SETNSFW
        [Command("setnsfw"), Priority(2)]
        [Description("Set whether this channel is NSFW or not. You can also provide a reason for the change.")]
        [Aliases("nsfw")]

        public async Task ChangeNsfwAsync(CommandContext ctx,
                                         [Description("Set NSFW?")] bool nsfw,
                                         [Description("Channel.")] DiscordChannel channel = null,
                                         [RemainingText, Description("Reason.")] string reason = null)
        {
            channel = channel ?? ctx.Channel;

            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException("Only text channels can be flagged as NSFW.");

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Nsfw = nsfw;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await this.InformAsync(ctx, $"Successfully set the NSFW var of channel {Formatter.Bold(channel.Name)} to {Formatter.Bold(nsfw.ToString())}", important: false);
        }

        [Command("setnsfw"), Priority(1)]
        public Task ChangeNsfwAsync(CommandContext ctx,
                                   [Description("Channel.")] DiscordChannel channel,
                                   [Description("Set NSFW?")] bool nsfw,
                                   [RemainingText, Description("Reason.")] string reason = null)
            => this.ChangeNsfwAsync(ctx, nsfw, channel, reason);

        [Command("setnsfw"), Priority(0)]
        public Task ChangeNsfwAsync(CommandContext ctx,
                                     [Description("Channel.")] DiscordChannel channel = null,
                                     [RemainingText, Description("Reason.")] string reason = null)
            => this.ChangeNsfwAsync(ctx, true, channel, reason);
        #endregion

        #region COMMAND_CHANNEL_SETPARENT
        [Command("setparent"), Priority(1)]
        [Description("Change the given channel's parent. If the channel is not given, uses the current one. " +
                     "You can also provide a reason.")]
        [Aliases("setpar", "par", "parent")]

        public async Task ChangeParentAsync(CommandContext ctx,
                                           [Description("Child channel.")] DiscordChannel channel,
                                           [Description("Parent category.")] DiscordChannel parent,
                                           [RemainingText, Description("Reason.")] string reason = null)
        {
            if (parent.Type != ChannelType.Category)
                throw new CommandFailedException("Parent must be a category.");

            channel = channel ?? ctx.Channel;

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Parent = parent;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await this.InformAsync(ctx, $"Successfully set the parent of channel {Formatter.Bold(channel.Name)} to {Formatter.Bold(parent.Name)}", important: false);
        }

        [Command("setparent"), Priority(0)]
        public Task ChangeParentAsync(CommandContext ctx,
                                     [Description("Parent category.")] DiscordChannel parent,
                                     [RemainingText, Description("Reason.")] string reason = null)
            => this.ChangeParentAsync(ctx, null, parent, reason);
        #endregion

        #region COMMAND_CHANNEL_SETPOSITION
        [Command("setposition"), Priority(2)]
        [Description("Change the position of the given channel in the guild channel list. If the channel " +
                     "is not given, repositions the current one. You can also provide reason.")]
        [Aliases("setpos", "pos", "position")]

        public async Task ReorderChannelAsync(CommandContext ctx,
                                             [Description("Channel to reposition.")] DiscordChannel channel,
                                             [Description("New position.")] int position,
                                             [RemainingText, Description("Reason.")] string reason = null)
        {
            if (position < 0)
                throw new ArgumentException("Position cannot be negative.", nameof(position));

            channel = channel ?? ctx.Channel;

            await channel.ModifyPositionAsync(position, ctx.BuildInvocationDetailsString(reason));
            await this.InformAsync(ctx, $"Changed the position of channel {Formatter.Bold(channel.Name)} to {Formatter.Bold(position.ToString())}", important: false);
        }

        [Command("setposition"), Priority(1)]
        public Task ReorderChannelAsync(CommandContext ctx,
                                       [Description("Position.")] int position,
                                       [Description("Channel to reorder.")] DiscordChannel channel,
                                       [RemainingText, Description("Reason.")] string reason = null)
            => this.ReorderChannelAsync(ctx, channel, position, reason);

        [Command("setposition"), Priority(0)]
        public Task ReorderChannelAsync(CommandContext ctx,
                                       [Description("Position.")] int position,
                                       [RemainingText, Description("Reason.")] string reason = null)
            => this.ReorderChannelAsync(ctx, null, position, reason);
        #endregion

        #region COMMAND_CHANNEL_SETRATELIMIT
        [Command("setratelimit"), Priority(1)]
        [Description("Set the per-user ratelimit for given channel. Setting the value to 0 will disable ratelimit.")]
        [Aliases("setrl", "setrate", "setrlimit")]

        public async Task SetRatelimitAsync(CommandContext ctx,
                                           [Description("Channel to affect.")] DiscordChannel channel,
                                           [Description("New ratelimit.")] int ratelimit,
                                           [RemainingText, Description("Reason.")] string reason = null)
        {
            if (ratelimit < 0)
                throw new InvalidCommandUsageException("Ratelimit value cannot be negative.");

            //if (!_ratelimitValues.Contains(ratelimit))
            //    throw new InvalidCommandUsageException($"Ratelimit value must be one of the following: {Formatter.InlineCode(string.Join(", ", _ratelimitValues))}");

            channel = channel ?? ctx.Channel;

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.PerUserRateLimit = ratelimit;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await this.InformAsync(ctx, $"Changed the ratemilit setting of channel {Formatter.Bold(channel.Name)} to {Formatter.Bold(ratelimit.ToString())}", important: false);
        }

        [Command("setratelimit"), Priority(0)]
        public Task SetRatelimitAsync(CommandContext ctx,
                                     [Description("New ratelimit.")] int ratelimit,
                                     [Description("Channel to affect.")] DiscordChannel channel,
                                     [RemainingText, Description("Reason.")] string reason = null)
            => this.SetRatelimitAsync(ctx, channel, ratelimit, reason);

        [Command("setratelimit"), Priority(0)]
        public Task SetRatelimitAsync(CommandContext ctx,
                                     [Description("New ratelimit.")] int ratelimit,
                                     [RemainingText, Description("Reason.")] string reason = null)
            => this.SetRatelimitAsync(ctx, ctx.Channel, ratelimit, reason);
        #endregion

        #region COMMAND_CHANNEL_SETTOPIC
        [Command("settopic"), Priority(2)]
        [Description("Set channel topic. If the channel is not given, uses the current one.")]
        [Aliases("t", "topic", "sett")]

        public async Task SetChannelTopicAsync(CommandContext ctx,
                                              [Description("Reason.")] string reason,
                                              [Description("Channel.")] DiscordChannel channel,
                                              [RemainingText, Description("New topic.")] string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new InvalidCommandUsageException("Missing topic.");

            if (topic.Length > 1024)
                throw new InvalidCommandUsageException("Topic cannot exceed 1024 characters!");

            channel = channel ?? ctx.Channel;

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Topic = topic;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await this.InformAsync(ctx, $"Successfully changed the topic for channel {Formatter.Bold(channel.Name)}", important: false);
        }

        [Command("settopic"), Priority(1)]
        public Task SetChannelTopicAsync(CommandContext ctx,
                                        [Description("Channel.")] DiscordChannel channel,
                                        [RemainingText, Description("New Topic.")] string topic)
            => this.SetChannelTopicAsync(ctx, null, channel, topic);

        [Command("settopic"), Priority(0)]
        public Task SetChannelTopicAsync(CommandContext ctx,
                                        [RemainingText, Description("New Topic.")] string topic)
            => this.SetChannelTopicAsync(ctx, null, null, topic);
        #endregion

        #region COMMAND_CHANNEL_VIEWPERMS
        [Command("viewperms"), Priority(3)]
        [Description("View permissions for a member or role in the given channel. If the member is not " +
                     "given, lists the sender's permissions. If the channel is not given, uses the current one.")]
        [Aliases("tp", "perms", "permsfor", "testperms", "listperms")]

        [RequireBotPermissions(Permissions.Administrator)]
        public Task PrintPermsAsync(CommandContext ctx,
                                         [Description("Member.")] DiscordMember member = null,
                                         [Description("Channel.")] DiscordChannel channel = null)
        {
            member = member ?? ctx.Member;
            channel = channel ?? ctx.Channel;

            string perms = $"{Formatter.Bold(member.DisplayName)} cannot access channel {Formatter.Bold(channel.Name)}.";
            if (member.PermissionsIn(channel).HasPermission(Permissions.AccessChannels))
                perms = member.PermissionsIn(channel).ToPermissionString();

            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = $"Permissions for {member.ToString()} in {channel.ToString()}:",
                Description = perms,
                Color = this.ModuleColor
            }.Build());
        }

        [Command("viewperms"), Priority(2)]
        public Task PrintPermsAsync(CommandContext ctx,
                                   [Description("Channel.")] DiscordChannel channel,
                                   [Description("Member.")] DiscordMember member = null)
            => this.PrintPermsAsync(ctx, member, channel);

        [Command("viewperms"), Priority(1)]
        public async Task PrintPermsAsync(CommandContext ctx,
                                         [Description("Role.")] DiscordRole role,
                                         [Description("Channel.")] DiscordChannel channel = null)
        {
            if (role.Position > ctx.Member.Hierarchy)
                throw new CommandFailedException("You cannot view permissions for roles which have position above your highest role position.");

            channel = channel ?? ctx.Channel;

            DiscordOverwrite overwrite = null;
            foreach (DiscordOverwrite o in channel.PermissionOverwrites.Where(o => o.Type == OverwriteType.Role)) {
                DiscordRole r = await o.GetRoleAsync();
                if (r.Id == role.Id) {
                    overwrite = o;
                    break;
                }
            }

            var emb = new DiscordEmbedBuilder {
                Title = $"Permissions for {role.ToString()} in {channel.ToString()}:",
                Color = this.ModuleColor
            };

            if (!(overwrite is null)) {
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
            => this.PrintPermsAsync(ctx, role, channel);
        #endregion



        private static async Task CheckPotentialChannelNameAsync(CommandContext ctx, string? name, bool throwIfNull = true, bool textChannel = false)
        {
            if (!string.IsNullOrWhiteSpace(name)) {
                if (name.Length > DiscordLimits.ChannelNameLimit)
                    throw new InvalidCommandUsageException(ctx, "cmd-err-chn-name", DiscordLimits.ChannelNameLimit);

                if (textChannel && name.Any(char.IsWhiteSpace))
                    throw new CommandFailedException(ctx, "cmd-err-chn-name-space");

                if (ctx.Guild.Channels.Select(kvp => kvp.Value.Name.ToLowerInvariant()).Contains(name.ToLowerInvariant())) {
                    if (!await ctx.WaitForBoolReplyAsync("q-chn-exists"))
                        return;
                }
            } else if (throwIfNull) {
                throw new InvalidCommandUsageException(ctx, "cmd-err-missing-name");
            }
        }
    }
}
