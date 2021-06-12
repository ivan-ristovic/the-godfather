using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

namespace TheGodfather.Modules.Administration
{
    [Group("channel"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("channels", "chn", "ch", "c")]
    [RequireGuild, RequirePermissions(Permissions.ManageChannels)]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed partial class ChannelModule : TheGodfatherModule
    {
        private static readonly DiscordColor EmbColor = ModuleType.Administration.ToDiscordColor();


        #region channel
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-chn-info")] DiscordChannel? channel = null)
            => this.InfoAsync(ctx, channel);
        #endregion

        #region channel add
        [Group("add")]
        [Aliases("create", "cr", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public sealed class ChannelAddModule : TheGodfatherModule
        {
            #region channel add category
            [Command("category"), UsesInteractivity]
            [Aliases("addcategory", "cat", "c", "cc", "+category", "+cat", "+c", "<c", "<<c")]
            public async Task CreateCategoryAsync(CommandContext ctx,
                                                 [RemainingText, Description("desc-chn-cat-name")] string name)
            {
                await CheckPotentialChannelNameAsync(ctx, name);
                DiscordChannel c = await ctx.Guild.CreateChannelCategoryAsync(name, reason: ctx.BuildInvocationDetailsString());
                await ctx.InfoAsync(this.ModuleColor, "fmt-chn-category", Formatter.Bold(c.Name));
            }
            #endregion

            #region channel add text
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

            #region channel add voice
            [Command("voice"), Priority(2), UsesInteractivity]
            [Aliases("addvoice", "addv", "cvoice", "cv", "+voice", "+v", "<v", "<<v")]
            public async Task CreateVoiceChannelAsync(CommandContext ctx,
                                                     [Description("desc-chn-voice-name")] string name,
                                                     [Description("desc-chn-parent")] DiscordChannel? parent = null,
                                                     [Description("desc-chn-userlimit")] int? userlimit = null,
                                                     [Description("desc-chn-bitrate")] int? bitrate = null)
            {
                CheckBitrate(ctx, bitrate);
                CheckUserLimit(ctx, userlimit);
                if (userlimit is { } and (< 1 or > DiscordLimits.VoiceChannelUserLimit))
                    throw new InvalidCommandUsageException(ctx, "cmd-err-chn-userlimit", 1, DiscordLimits.VoiceChannelUserLimit);
                if (bitrate is { } and (< DiscordLimits.VoiceChannelMinBitrate or > DiscordLimits.VoiceChannelMaxBitrate))
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

        #region channel bitrate
        [Command("bitrate"), Priority(2)]
        [Aliases("setbr", "setbitr", "setbrate", "setb", "br", "setbitrate", "bitr", "brate")]
        public Task BitrateAsync(CommandContext ctx,
                                [Description("desc-chn-mod")] DiscordChannel channel,
                                [Description("str-bitrate")] int bitrate,
                                [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalGetOrSetBitrateAsync(ctx, channel, bitrate, reason);

        [Command("bitrate"), Priority(1)]
        public Task BitrateAsync(CommandContext ctx,
                                [Description("str-bitrate")] int bitrate,
                                [Description("desc-chn-mod")] DiscordChannel channel,
                                [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalGetOrSetBitrateAsync(ctx, channel, bitrate, reason);

        [Command("bitrate"), Priority(0)]
        public Task BitrateAsync(CommandContext ctx,
                                [Description("desc-chn-mod")] DiscordChannel? channel = null)
            => InternalGetOrSetBitrateAsync(ctx, channel ?? ctx.Channel, null, null);
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

            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle(channel.ToString());
                emb.WithColor(this.ModuleColor);
                if (!string.IsNullOrWhiteSpace(channel.Topic))
                    emb.WithDescription(Formatter.Italic(Formatter.Strip(channel.Topic)));
                emb.AddLocalizedTitleField("str-chn-type", channel.Type, inline: true);
                emb.AddLocalizedTitleField("str-nsfw", channel.IsNSFW, inline: true);
                emb.AddLocalizedTitleField("str-pos", channel.Position, inline: true);
                emb.AddLocalizedTitleField("str-ratelimit", channel.PerUserRateLimit, inline: true, unknown: false);
                if (channel.Type == ChannelType.Voice) {
                    emb.AddLocalizedTitleField("str-bitrate", channel.Bitrate, inline: true);
                    if (channel.UserLimit > 0)
                        emb.AddLocalizedTitleField("str-user-limit", channel.UserLimit, inline: true);
                }
                emb.AddLocalizedTimestampField("str-created-at", channel.CreationTimestamp, inline: true);
            });
        }
        #endregion

        #region channel modify
        [Group("modify")]
        [Aliases("edit", "mod", "m", "e", "set", "change")]
        public sealed class ChannelModifyModule : TheGodfatherModule
        {
            #region channel modify bitrate
            [Command("bitrate"), Priority(1)]
            [Aliases("br", "bitr", "brate", "b")]
            public Task ModifyBitrateAsync(CommandContext ctx,
                                          [Description("desc-chn-mod")] DiscordChannel channel,
                                          [Description("str-bitrate")] int bitrate,
                                          [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalGetOrSetBitrateAsync(ctx, channel, bitrate, reason);

            [Command("bitrate"), Priority(0)]
            public Task ModifyBitrateAsync(CommandContext ctx,
                                          [Description("str-bitrate")] int bitrate,
                                          [Description("desc-chn-mod")] DiscordChannel channel,
                                          [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalGetOrSetBitrateAsync(ctx, channel, bitrate, reason);
            #endregion

            #region channel modify userlimit
            [Command("userlimit"), Priority(1)]
            [Aliases("ul", "ulimit", "limit", "l")]
            public Task ModifyUserLimitAsync(CommandContext ctx,
                                            [Description("desc-chn-mod")] DiscordChannel channel,
                                            [Description("str-user-limit")] int userlimit,
                                            [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalGetOrSetUserLimitAsync(ctx, channel, userlimit, reason);

            [Command("userlimit"), Priority(0)]
            public Task ModifyUserLimitAsync(CommandContext ctx,
                                            [Description("str-user-limit")] int userlimit,
                                            [Description("desc-chn-mod")] DiscordChannel channel,
                                            [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalGetOrSetUserLimitAsync(ctx, channel, userlimit, reason);
            #endregion

            #region channel modify name
            [Command("name"), Priority(2)]
            [Aliases("title", "nm", "n")]
            public Task ModifyNameAsync(CommandContext ctx,
                                       [Description("desc-chn-mod")] DiscordChannel channel,
                                       [Description("str-name")] string name,
                                       [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalSetNameAsync(ctx, channel, name, reason);

            [Command("name"), Priority(1)]
            public Task ModifyNameAsync(CommandContext ctx,
                                       [Description("str-name")] string name,
                                       [Description("desc-chn-mod")] DiscordChannel channel,
                                       [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalSetNameAsync(ctx, channel, name, reason);

            [Command("name"), Priority(0)]
            public Task ModifyNameAsync(CommandContext ctx,
                                       [RemainingText, Description("desc-rsn")] string name)
                => InternalSetNameAsync(ctx, ctx.Channel, name, null);
            #endregion

            #region channel modify nsfw
            [Command("nsfw"), Priority(2)]
            public Task ModifyNsfwAsync(CommandContext ctx,
                                       [Description("desc-chn-mod")] DiscordChannel channel,
                                       [Description("str-name")] bool nsfw,
                                       [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalGetOrSetNsfwAsync(ctx, channel, nsfw, reason);

            [Command("nsfw"), Priority(1)]
            public Task ModifyNsfwAsync(CommandContext ctx,
                                       [Description("str-name")] bool nsfw,
                                       [Description("desc-chn-mod")] DiscordChannel channel,
                                       [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalGetOrSetNsfwAsync(ctx, channel, nsfw, reason);

            [Command("nsfw"), Priority(0)]
            public Task ModifyNsfwAsync(CommandContext ctx,
                                       [Description("desc-rsn")] bool nsfw,
                                       [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalGetOrSetNsfwAsync(ctx, ctx.Channel, nsfw, reason);
            #endregion

            #region channel modify parent
            [Command("parent"), Priority(1)]
            [Aliases("par")]
            public Task ModifyParentAsync(CommandContext ctx,
                                         [Description("desc-chn-parent-children")] params DiscordChannel[] channels)
                => InternalSetParentAsync(ctx, channels, null);

            [Command("parent"), Priority(0)]
            public Task ModifyParentAsync(CommandContext ctx,
                                         [Description("desc-rsn")] string reason,
                                         [Description("desc-chn-parent-children")] params DiscordChannel[] channels)
                => InternalSetParentAsync(ctx, channels, reason);
            #endregion

            #region channel modify position
            [Command("position"), Priority(2)]
            [Aliases("pos", "p", "order")]
            public Task ModifyPositionAsync(CommandContext ctx,
                                           [Description("desc-chn-mod")] DiscordChannel channel,
                                           [Description("str-pos")] int position,
                                           [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalModifyPositionAsync(ctx, channel, position, reason);

            [Command("position"), Priority(1)]
            public Task ModifyPositionAsync(CommandContext ctx,
                                           [Description("str-pos")] int position,
                                           [Description("desc-chn-mod")] DiscordChannel channel,
                                           [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalModifyPositionAsync(ctx, channel, position, reason);

            [Command("position"), Priority(0)]
            public Task ModifyPositionAsync(CommandContext ctx,
                                           [Description("str-pos")] int position,
                                           [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalModifyPositionAsync(ctx, ctx.Channel, position, reason);
            #endregion

            #region channel modify slowmode
            [Command("slowmode"), Priority(2)]
            [Aliases("rlimit", "rl", "ratel", "rate", "ratelimit", "slow", "sm", "smode")]
            public Task ModifySlowmodeAsync(CommandContext ctx,
                                           [Description("desc-chn-mod")] DiscordChannel channel,
                                           [Description("desc-chn-slowmode")] int slowmode,
                                           [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalGetOrSetSlowmodeAsync(ctx, channel, slowmode, reason);

            [Command("slowmode"), Priority(1)]
            public Task ModifySlowmodeAsync(CommandContext ctx,
                                           [Description("desc-chn-slowmode")] int slowmode,
                                           [Description("desc-chn-mod")] DiscordChannel channel,
                                           [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalGetOrSetSlowmodeAsync(ctx, channel, slowmode, reason);

            [Command("slowmode"), Priority(0)]
            public Task ModifySlowmodeAsync(CommandContext ctx,
                                           [Description("desc-chn-slowmode")] int slowmode,
                                           [RemainingText, Description("desc-rsn")] string? reason = null)
                => InternalGetOrSetSlowmodeAsync(ctx, ctx.Channel, slowmode, reason);
            #endregion

            #region channel modify topic
            [Command("topic"), Priority(2)]
            [Aliases("t", "desc", "description")]

            public Task ModifyTopicAsync(CommandContext ctx,
                                        [Description("desc-rsn")] string reason,
                                        [Description("desc-chn-mod")] DiscordChannel channel,
                                        [RemainingText, Description("desc-chn-topic")] string topic)
                => InternalGetOrSetTopicAsync(ctx, channel, topic, reason);

            [Command("topic"), Priority(1)]
            public Task ModifyTopicAsync(CommandContext ctx,
                                        [Description("desc-chn-mod")] DiscordChannel channel,
                                        [RemainingText, Description("desc-chn-topic")] string topic)
                => InternalGetOrSetTopicAsync(ctx, channel, topic, null);

            [Command("topic"), Priority(0)]
            public Task ModifyTopicAsync(CommandContext ctx,
                                        [RemainingText, Description("desc-chn-topic")] string topic)
                => InternalGetOrSetTopicAsync(ctx, ctx.Channel, topic, null);
            #endregion
        }
        #endregion

        #region channel nsfw
        [Command("nsfw"), Priority(3)]
        [Aliases("setnsfw")]
        public Task NsfwAsync(CommandContext ctx,
                             [Description("desc-chn-mod")] DiscordChannel channel,
                             [Description("str-name")] bool nsfw,
                             [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalGetOrSetNsfwAsync(ctx, channel, nsfw, reason);

        [Command("nsfw"), Priority(2)]
        public Task NsfwAsync(CommandContext ctx,
                             [Description("str-name")] bool nsfw,
                             [Description("desc-chn-mod")] DiscordChannel channel,
                             [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalGetOrSetNsfwAsync(ctx, channel, nsfw, reason);

        [Command("nsfw"), Priority(1)]
        public Task NsfwAsync(CommandContext ctx,
                             [Description("desc-rsn")] bool nsfw,
                             [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalGetOrSetNsfwAsync(ctx, ctx.Channel, nsfw, reason);

        [Command("nsfw"), Priority(0)]
        public Task NsfwAsync(CommandContext ctx,
                             [Description("desc-chn-mod")] DiscordChannel? channel = null)
            => InternalGetOrSetNsfwAsync(ctx, channel ?? ctx.Channel, null, null);
        #endregion

        #region channel rename
        [Command("rename"), Priority(2)]
        [Aliases("settitle", "setname", "changename", "rn", "rnm", "name", "mv")]
        public Task RenameAsync(CommandContext ctx,
                               [Description("desc-chn-mod")] DiscordChannel channel,
                               [Description("str-name")] string name,
                               [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalSetNameAsync(ctx, channel, name, reason);

        [Command("rename"), Priority(1)]
        public Task RenameAsync(CommandContext ctx,
                               [Description("str-name")] string name,
                               [Description("desc-chn-mod")] DiscordChannel channel,
                               [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalSetNameAsync(ctx, channel, name, reason);

        [Command("rename"), Priority(0)]
        public Task RenameAsync(CommandContext ctx,
                               [RemainingText, Description("desc-rsn")] string name)
            => InternalSetNameAsync(ctx, ctx.Channel, name, null);
        #endregion

        #region channel setparent
        [Command("setparent"), Priority(1)]
        [Aliases("setpar", "parent", "par")]
        public Task ModifyParentAsync(CommandContext ctx,
                                     [Description("desc-chn-parent-children")] params DiscordChannel[] channels)
            => InternalSetParentAsync(ctx, channels, null);

        [Command("setparent"), Priority(0)]
        public Task ModifyParentAsync(CommandContext ctx,
                                     [Description("desc-rsn")] string reason,
                                     [Description("desc-chn-parent-children")] params DiscordChannel[] channels)
            => InternalSetParentAsync(ctx, channels, reason);
        #endregion

        #region channel setposition
        [Command("setposition"), Priority(2)]
        [Aliases("setpos", "setp", "order", "setorder", "position", "pos")]
        public Task SetPositionAsync(CommandContext ctx,
                                    [Description("desc-chn-mod")] DiscordChannel channel,
                                    [Description("str-pos")] int position,
                                    [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalModifyPositionAsync(ctx, channel, position, reason);

        [Command("setposition"), Priority(1)]
        public Task SetPositionAsync(CommandContext ctx,
                                    [Description("str-pos")] int position,
                                    [Description("desc-chn-mod")] DiscordChannel channel,
                                    [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalModifyPositionAsync(ctx, channel, position, reason);

        [Command("setposition"), Priority(0)]
        public Task SetPositionAsync(CommandContext ctx,
                                    [Description("str-pos")] int position,
                                    [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalModifyPositionAsync(ctx, ctx.Channel, position, reason);
        #endregion

        #region channel slowmode
        [Command("slowmode"), Priority(3)]
        [Aliases("setratel", "setrl", "setrate", "setratelimit", "setslow", "setslowmode", "slow", "sm", "setsmode", "smode")]
        public Task SlowmodeAsync(CommandContext ctx,
                                 [Description("desc-chn-mod")] DiscordChannel channel,
                                 [Description("desc-chn-slowmode")] int slowmode,
                                 [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalGetOrSetSlowmodeAsync(ctx, channel, slowmode, reason);

        [Command("slowmode"), Priority(2)]
        public Task SlowmodeAsync(CommandContext ctx,
                                 [Description("desc-chn-slowmode")] int slowmode,
                                 [Description("desc-chn-mod")] DiscordChannel channel,
                                 [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalGetOrSetSlowmodeAsync(ctx, channel, slowmode, reason);

        [Command("slowmode"), Priority(1)]
        public Task SlowmodeAsync(CommandContext ctx,
                                 [Description("desc-chn-slowmode")] int slowmode,
                                 [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalGetOrSetSlowmodeAsync(ctx, ctx.Channel, slowmode, reason);

        [Command("slowmode"), Priority(0)]
        public Task SlowmodeAsync(CommandContext ctx,
                                 [Description("desc-chn-mod")] DiscordChannel? channel = null)
            => InternalGetOrSetSlowmodeAsync(ctx, channel ?? ctx.Channel, null, null);
        #endregion

        #region channel topic
        [Command("topic"), Priority(3)]
        [Aliases("t", "settopic", "sett", "desc", "setdesc", "description", "setdescription")]
        public Task TopicAsync(CommandContext ctx,
                              [Description("desc-rsn")] string reason,
                              [Description("desc-chn-mod")] DiscordChannel channel,
                              [RemainingText, Description("desc-chn-topic")] string topic)
            => InternalGetOrSetTopicAsync(ctx, channel, topic, reason);

        [Command("topic"), Priority(2)]
        public Task TopicAsync(CommandContext ctx,
                              [Description("desc-chn-mod")] DiscordChannel channel,
                              [RemainingText, Description("desc-chn-topic")] string topic)
            => InternalGetOrSetTopicAsync(ctx, channel, topic, null);

        [Command("topic"), Priority(1)]
        public Task TopicAsync(CommandContext ctx,
                              [RemainingText, Description("desc-chn-topic")] string topic)
            => InternalGetOrSetTopicAsync(ctx, ctx.Channel, topic, null);

        [Command("topic"), Priority(0)]
        public Task TopicAsync(CommandContext ctx)
            => InternalGetOrSetTopicAsync(ctx, ctx.Channel, null, null);
        #endregion

        #region channel userlimit
        [Command("userlimit"), Priority(2)]
        [Aliases("setul", "setulimit", "setlimit", "setl", "setuserlimit", "ul", "ulimig", "userl")]
        public Task UserLimitAsync(CommandContext ctx,
                                  [Description("desc-chn-mod")] DiscordChannel channel,
                                  [Description("str-user-limit")] int userlimit,
                                  [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalGetOrSetUserLimitAsync(ctx, channel, userlimit, reason);

        [Command("userlimit"), Priority(1)]
        public Task UserLimitAsync(CommandContext ctx,
                                  [Description("str-user-limit")] int userlimit,
                                  [Description("desc-chn-mod")] DiscordChannel channel,
                                  [RemainingText, Description("desc-rsn")] string? reason = null)
            => InternalGetOrSetUserLimitAsync(ctx, channel, userlimit, reason);

        [Command("userlimit"), Priority(0)]
        public Task UserLimitAsync(CommandContext ctx,
                                  [Description("desc-chn-mod")] DiscordChannel? channel = null)
            => InternalGetOrSetUserLimitAsync(ctx, channel ?? ctx.Channel, null, null);
        #endregion

        #region channel viewperms
        [Command("viewperms"), Priority(3)]
        [Aliases("perms", "permsfor", "testperms", "listperms", "permissions")]
        [RequireBotPermissions(Permissions.Administrator)]
        public Task PrintPermsAsync(CommandContext ctx,
                                   [Description("desc-member")] DiscordMember? member = null,
                                   [Description("desc-chn-mod")] DiscordChannel? channel = null)
        {
            member ??= ctx.Member;
            channel ??= ctx.Channel;
            Permissions perms = member.PermissionsIn(channel);

            string permsStr = this.Localization.GetString(ctx.Guild.Id, "fmt-chn-perms-none", member.DisplayName, channel.Name);
            if (perms.HasPermission(Permissions.AccessChannels))
                permsStr = perms.ToPermissionString();

            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithLocalizedTitle("fmt-chn-perms", member.ToDiscriminatorString(), channel);
                emb.WithDescription(permsStr);
                emb.WithColor(this.ModuleColor);
            });
        }

        [Command("viewperms"), Priority(2)]
        public Task PrintPermsAsync(CommandContext ctx,
                                   [Description("desc-chn-mod")] DiscordChannel channel,
                                   [Description("desc-member")] DiscordMember? member = null)
            => this.PrintPermsAsync(ctx, member, channel);

        [Command("viewperms"), Priority(1)]
        public async Task PrintPermsAsync(CommandContext ctx,
                                         [Description("desc-role")] DiscordRole role,
                                         [Description("desc-chn-mod")] DiscordChannel? channel = null)
        {
            if (role.Position > ctx.Member.Hierarchy)
                throw new CommandFailedException(ctx, "fmt-role-perms-none");

            channel ??= ctx.Channel;
            DiscordOverwrite? ow = await channel.FindOverwriteForRoleAsync(role);

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithLocalizedTitle("fmt-chn-perms", role.Mention, channel);
                emb.WithColor(this.ModuleColor);
                if (ow is { }) {
                    emb.AddLocalizedTitleField("str-allowed", ow.Allowed.ToPermissionString())
                       .AddLocalizedTitleField("str-denied", ow.Denied.ToPermissionString());
                } else {
                    emb.WithLocalizedDescription("fmt-chn-ow-none", role.Permissions.ToPermissionString());
                }
            });
        }

        [Command("viewperms"), Priority(0)]
        public Task PrintPermsAsync(CommandContext ctx,
                                   [Description("desc-chn-mod")] DiscordChannel channel,
                                   [Description("desc-role")] DiscordRole role)
            => this.PrintPermsAsync(ctx, role, channel);
        #endregion


        #region checks
        private static ImmutableArray<int> _ratelimitValues = new[] { 0, 5, 10, 15, 30, 45, 60, 75, 90, 120 }.ToImmutableArray();


        private static async Task CheckPotentialChannelNameAsync(CommandContext ctx, string? name, bool throwIfNull = true, bool textChannel = false)
        {
            if (!string.IsNullOrWhiteSpace(name)) {
                if (name.Length > DiscordLimits.ChannelNameLimit)
                    throw new InvalidCommandUsageException(ctx, "cmd-err-chn-name", DiscordLimits.ChannelNameLimit);

                if (textChannel && name.Any(char.IsWhiteSpace))
                    throw new CommandFailedException(ctx, "cmd-err-chn-name-space");

                if (ctx.Guild.Channels.Select(kvp => kvp.Value.Name.ToLowerInvariant()).Contains(name.ToLowerInvariant())) {
                    if (!await ctx.WaitForBoolReplyAsync("q-chn-exists"))
                        throw new CommandCancelledException();
                }
            } else if (throwIfNull) {
                throw new InvalidCommandUsageException(ctx, "cmd-err-missing-name");
            }
        }

        private static void CheckBitrate(CommandContext ctx, int? bitrate)
        {
            if (bitrate is { } and (< DiscordLimits.VoiceChannelMinBitrate or > DiscordLimits.VoiceChannelMaxBitrate))
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-bitrate", DiscordLimits.VoiceChannelMinBitrate, DiscordLimits.VoiceChannelMinBitrate);
        }

        private static void CheckUserLimit(CommandContext ctx, int? userlimit)
        {
            if (userlimit is { } and (< 1 or > DiscordLimits.VoiceChannelUserLimit))
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-userlimit", 1, DiscordLimits.VoiceChannelUserLimit);
        }
        #endregion

        #region internals
        private static async Task InternalGetOrSetBitrateAsync(CommandContext ctx, DiscordChannel channel, int? bitrate, string? reason)
        {
            if (channel.Type != ChannelType.Voice)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-voice");

            if (bitrate is null) {
                CheckIfMemberIsAllowedToAccessChannel(ctx, channel);
                await ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithLocalizedTitle("fmt-chn-bitrate", channel.Name);
                    emb.WithDescription(channel.Bitrate);
                    emb.WithColor(EmbColor);
                });
                return;
            }

            CheckBitrate(ctx, bitrate);
            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Bitrate = bitrate;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await ctx.InfoAsync(EmbColor, "fmt-chn-mod-bitrate", Formatter.Bold(channel.Name), bitrate);
        }

        private static async Task InternalGetOrSetUserLimitAsync(CommandContext ctx, DiscordChannel channel, int? userlimit, string? reason)
        {
            if (channel.Type != ChannelType.Voice)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-voice");

            if (userlimit is null) {
                CheckIfMemberIsAllowedToAccessChannel(ctx, channel);
                await ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithLocalizedTitle("fmt-chn-userlimit", channel.Name);
                    emb.WithDescription(channel.UserLimit);
                    emb.WithColor(EmbColor);
                });
                return;
            }

            CheckUserLimit(ctx, userlimit);
            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Userlimit = userlimit;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await ctx.InfoAsync(EmbColor, "fmt-chn-mod-userlimit", Formatter.Bold(channel.Name), userlimit);
        }

        private static async Task InternalSetNameAsync(CommandContext ctx, DiscordChannel channel, string name, string? reason)
        {
            await CheckPotentialChannelNameAsync(ctx, name, textChannel: channel.Type == ChannelType.Text);
            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Name = name;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await ctx.InfoAsync(EmbColor, "fmt-chn-mod-name", Formatter.InlineCode(channel.Id.ToString()), name);
        }

        private static async Task InternalGetOrSetNsfwAsync(CommandContext ctx, DiscordChannel channel, bool? nsfw, string? reason)
        {
            if (channel.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-text");

            if (nsfw is null) {
                CheckIfMemberIsAllowedToAccessChannel(ctx, channel);
                await ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithLocalizedTitle("fmt-chn-nsfw", channel.Name);
                    emb.WithDescription(channel.IsNSFW);
                    emb.WithColor(EmbColor);
                });
                return;
            }

            if (channel.IsNSFW == nsfw)
                return;

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Nsfw = nsfw;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await ctx.InfoAsync(EmbColor, "fmt-chn-mod-nsfw", Formatter.Bold(channel.Name), nsfw);
        }

        private static async Task InternalSetParentAsync(CommandContext ctx, DiscordChannel[]? channels, string? reason)
        {
            DiscordChannel? parent = channels?.SingleOrDefault(c => c.IsCategory);
            if (parent is null)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-cat");

            IEnumerable<DiscordChannel>? children = channels?.Where(c => c != parent);
            if (children?.Any() ?? false) {
                foreach (DiscordChannel channel in children)
                    await ModifyParent(channel);
            } else {
                await ModifyParent(ctx.Channel);
            }

            await ctx.InfoAsync(EmbColor, "fmt-chn-mod-parent", Formatter.Bold(parent.Name), children?.JoinWith() ?? ctx.Channel.ToString());


            Task ModifyParent(DiscordChannel channel)
            {
                return channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                    m.Parent = channel;
                    m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
                }));
            }
        }

        private static async Task InternalModifyPositionAsync(CommandContext ctx, DiscordChannel channel, int position, string? reason)
        {
            if (position is { } and <= 0)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-pos");

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Position = position;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await ctx.InfoAsync(EmbColor, "fmt-chn-mod-pos",
                                channel.Type == ChannelType.Text ? channel.Mention : Formatter.Bold(channel.Name), position
            );
        }

        private static async Task InternalGetOrSetSlowmodeAsync(CommandContext ctx, DiscordChannel channel, int? slowmode, string? reason)
        {
            if (channel.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-text");

            if (slowmode is null) {
                CheckIfMemberIsAllowedToAccessChannel(ctx, channel);
                await ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithLocalizedTitle("fmt-chn-slowmode", channel.Name);
                    emb.WithDescription(channel.PerUserRateLimit);
                    emb.WithColor(EmbColor);
                });
                return;
            }

            if (slowmode is { } && !_ratelimitValues.Contains(slowmode.Value))
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-ratelimit", _ratelimitValues.JoinWith(", "));

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.PerUserRateLimit = slowmode;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await ctx.InfoAsync(EmbColor, "fmt-chn-mod-ratelimit", channel.Mention, slowmode);
        }

        private static async Task InternalGetOrSetTopicAsync(CommandContext ctx, DiscordChannel channel, string? topic, string? reason)
        {
            if (channel.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-text");

            if (string.IsNullOrEmpty(topic)) {
                CheckIfMemberIsAllowedToAccessChannel(ctx, channel);
                await ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithLocalizedTitle("fmt-chn-topic", channel.Name);
                    if (string.IsNullOrWhiteSpace(channel.Topic))
                        emb.WithLocalizedDescription("str-none");
                    else
                        emb.WithDescription(Formatter.BlockCode(Formatter.Sanitize(channel.Topic)));
                    emb.WithColor(EmbColor);
                });
                return;
            }

            if (topic.Length > DiscordLimits.ChannelTopicLimit)
                throw new CommandFailedException(ctx, "cmd-err-chn-topic-size", DiscordLimits.ChannelTopicLimit);

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Topic = topic;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await ctx.InfoAsync(EmbColor, "fmt-chn-mod-topic", channel.Mention, Formatter.Sanitize(Formatter.BlockCode(topic)));
        }

        private static void CheckIfMemberIsAllowedToAccessChannel(CommandContext ctx, DiscordChannel channel)
        {
            if (!channel.PermissionsFor(ctx.Member).HasPermission(Permissions.AccessChannels))
                throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireUserPermissionsAttribute(Permissions.AccessChannels) });
        }
        #endregion
    }
}
