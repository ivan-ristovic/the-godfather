#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class GuildModule
    {
        [Group("configure"), Module(ModuleType.Administration)]
        [Description("Allows manipulation of guild settings for this bot. If invoked without subcommands, lists the current guild configuration.")]
        [Aliases("config", "cfg")]
        [UsageExample("!guild configure")]
        [Cooldown(3, 5, CooldownBucketType.Guild)]
        [RequireUserPermissions(Permissions.ManageGuild)]
        [ListeningCheck]
        public partial class ConfigModule : TheGodfatherBaseModule
        {

            public ConfigModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Configuration for this guild:",
                    Description = ctx.Guild.ToString(),
                    Color = DiscordColor.Aquamarine,
                    ThumbnailUrl = ctx.Guild.IconUrl
                };

                var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);

                emb.AddField("Prefix", Shared.GetGuildPrefix(ctx.Guild.Id), inline: true);
                emb.AddField("Command suggestions", gcfg.SuggestionsEnabled ? "on" : "off", inline: true);
                emb.AddField("Action logging", gcfg.LoggingEnabled ? "on" : "off", inline: true);

                // TODO w/l

                await ctx.RespondAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }


            #region COMMAND_CONFIG_WIZARD
            [Command("setup"), Module(ModuleType.Administration)]
            [Description("Starts an interactive wizard for configuring the guild settings.")]
            [Aliases("wizard")]
            [UsageExample("!guild cfg setup")]
            public async Task SetupAsync(CommandContext ctx)
            {
                // TODO
            }
            #endregion


            #region GROUP_CONFIG_SUGGESTIONS
            [Group("suggestions"), Module(ModuleType.Administration)]
            [Description("Command suggestions configuration.")]
            [Aliases("suggestion", "sugg", "sug", "s")]
            [UsageExample("!guild cfg suggestions")]
            public class Suggestions : TheGodfatherBaseModule
            {

                public Suggestions(SharedData shared, DBService db) : base(shared, db) { }


                [GroupCommand]
                public async Task ExecuteGroupAsync(CommandContext ctx)
                {
                    var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                    await ctx.RespondWithIconEmbedAsync($"Command suggestions for this guild are {Formatter.Bold(gcfg.SuggestionsEnabled ? "enabled" : "disabled")}!")
                        .ConfigureAwait(false);
                }


                #region COMMAND_SUGGESTIONS_ENABLE
                [Command("enable"), Module(ModuleType.Administration)]
                [Description("Enables command suggestions for this guild.")]
                [Aliases("on")]
                [UsageExample("!guild cfg suggestions on")]
                public async Task EnableAsync(CommandContext ctx)
                {
                    var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.SuggestionsEnabled = true;
                    await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                        .ConfigureAwait(false);
                    await ctx.RespondWithIconEmbedAsync("Enabled command suggestions!")
                        .ConfigureAwait(false);
                }
                #endregion

                #region COMMAND_SUGGESTIONS_DISABLE
                [Command("disable"), Module(ModuleType.Administration)]
                [Description("Disables command suggestions for this guild.")]
                [Aliases("off")]
                [UsageExample("!guild cfg suggestions off")]
                public async Task DisableAsync(CommandContext ctx)
                {
                    var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.SuggestionsEnabled = false;
                    await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                        .ConfigureAwait(false);
                    await ctx.RespondWithIconEmbedAsync("Disabled command suggestions!")
                        .ConfigureAwait(false);
                }
                #endregion
            }
            #endregion

            #region GROUP_CONFIG_LOGGING
            [Group("logging"), Module(ModuleType.Administration)]
            [Description("Command action logging configuration.")]
            [Aliases("log", "modlog")]
            [UsageExample("!guild cfg logging")]
            public class Logging : TheGodfatherBaseModule
            {

                public Logging(SharedData shared, DBService db) : base(shared, db) { }


                [GroupCommand]
                public async Task ExecuteGroupAsync(CommandContext ctx)
                {
                    var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                    await ctx.RespondWithIconEmbedAsync($"Action logging for this guild is {Formatter.Bold(gcfg.LoggingEnabled ? "enabled" : "disabled")}!")
                        .ConfigureAwait(false);
                }


                #region COMMAND_LOGGING_ENABLE
                [Command("enable"), Module(ModuleType.Administration)]
                [Description("Enables action logging for this guild in the given channel.")]
                [Aliases("on")]
                [UsageExample("!guild cfg logging on")]
                public async Task EnableAsync(CommandContext ctx,
                                             [Description("Channel.")] DiscordChannel channel = null)
                {
                    if (channel == null)
                        channel = ctx.Channel;

                    var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.LogChannelId = channel.Id;
                    await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                        .ConfigureAwait(false);
                    await ctx.RespondWithIconEmbedAsync($"Enabled action log in channel {channel.Mention}!")
                        .ConfigureAwait(false);
                }
                #endregion

                #region COMMAND_LOGGING_DISABLE
                [Command("disable"), Module(ModuleType.Administration)]
                [Description("Disables action logging for this guild.")]
                [Aliases("off")]
                [UsageExample("!guild cfg logging off")]
                public async Task DisableAsync(CommandContext ctx)
                {
                    var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.LogChannelId = 0;
                    await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                        .ConfigureAwait(false);
                    await ctx.RespondWithIconEmbedAsync("Disabled action logging!")
                        .ConfigureAwait(false);
                }
                #endregion
                
                #region COMMAND_LOGGING_CHANNEL
                [Command("channel"), Module(ModuleType.Administration)]
                [Description("Gets or sets current action log channel.")]
                [Aliases("chn", "c")]
                [UsageExample("!guild cfg logging channel")]
                [UsageExample("!guild cfg logging channel #modlog")]
                public async Task ChannelAsync(CommandContext ctx,
                                              [Description("Channel.")] DiscordChannel channel = null)
                {
                    var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                    if (channel == null) {
                        if (gcfg.LoggingEnabled) {
                            var c = ctx.Guild.GetChannel(gcfg.LogChannelId);
                            if (c == null)
                                throw new CommandFailedException($"Action logging channel was set but does not exist anymore (id: {gcfg.LogChannelId}).");
                            await ctx.RespondWithIconEmbedAsync($"Action logging channel: {c.Mention}.")
                                .ConfigureAwait(false);
                        } else {
                            await ctx.RespondWithIconEmbedAsync("Action logging channel isn't set for this guild.")
                                .ConfigureAwait(false);
                        }
                    } else {
                        if (channel.Type != ChannelType.Text)
                            throw new CommandFailedException("Action logging channel must be a text channel.");

                        gcfg.LogChannelId = channel.Id;
                        await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                            .ConfigureAwait(false);
                        await ctx.RespondWithIconEmbedAsync($"Action logging channel set to {channel.Mention}.")
                            .ConfigureAwait(false);
                    }
                }
                #endregion
            }
            #endregion

            #region GROUP_CONFIG_WELCOME
            [Group("welcome"), Module(ModuleType.Administration)]
            [Description("Allows user welcoming configuration.")]
            [Aliases("enter", "join", "wlc", "w")]
            [UsageExample("!guild cfg welcome")]
            public class Enter : TheGodfatherBaseModule
            {

                public Enter(SharedData shared, DBService db) : base(shared, db) { }


                [GroupCommand]
                public async Task ExecuteGroupAsync(CommandContext ctx)
                {
                    ulong cid = await Database.GetWelcomeChannelIdAsync(ctx.Guild.Id)
                        .ConfigureAwait(false);
                    await ctx.RespondWithIconEmbedAsync($"Member welcome messages for this guild are: {Formatter.Bold(cid != 0 ? "enabled" : "disabled")}!")
                        .ConfigureAwait(false);
                }


                #region COMMAND_WELCOME_CHANNEL
                [Command("channel"), Module(ModuleType.Administration)]
                [Description("Gets or sets welcome message channel.")]
                [Aliases("chn", "c")]
                [UsageExample("!guild cfg welcome channel")]
                [UsageExample("!guild cfg welcome channel #lobby")]
                public async Task ChannelAsync(CommandContext ctx,
                                              [Description("Channel.")] DiscordChannel channel = null)
                {
                    if (channel == null) {
                        ulong cid = await Database.GetWelcomeChannelIdAsync(ctx.Guild.Id)
                            .ConfigureAwait(false);
                        if (cid != 0) {
                            var c = ctx.Guild.GetChannel(cid);
                            if (c == null)
                                throw new CommandFailedException($"Welcome channel was set but does not exist anymore (id: {cid}).");
                            await ctx.RespondWithIconEmbedAsync($"Welcome message channel: {c.Mention}.")
                                .ConfigureAwait(false);
                        } else {
                            await ctx.RespondWithIconEmbedAsync("Welcome message channel isn't set for this guild.")
                                .ConfigureAwait(false);
                        }
                    } else {
                        if (channel.Type != ChannelType.Text)
                            throw new CommandFailedException("Welcome channel must be a text channel.");

                        await Database.SetWelcomeChannelAsync(ctx.Guild.Id, channel.Id)
                            .ConfigureAwait(false);
                        await ctx.RespondWithIconEmbedAsync($"Welcome message channel set to {channel.Mention}.")
                            .ConfigureAwait(false);
                    }
                }
                #endregion

                #region COMMAND_WELCOME_MESSAGE
                [Command("message"), Module(ModuleType.Administration)]
                [Description("Gets or sets current welcome message.")]
                [Aliases("msg", "m")]
                [UsageExample("!guild cfg welcome message")]
                [UsageExample("!guild cfg welcome message Welcome, %user%!")]
                public async Task MessageAsync(CommandContext ctx,
                                              [RemainingText, Description("Welcome message.")] string message = null)
                {
                    if (string.IsNullOrWhiteSpace(message)) {
                        var msg = await Database.GetWelcomeMessageAsync(ctx.Guild.Id)
                            .ConfigureAwait(false);

                        await ctx.RespondWithIconEmbedAsync($"Welcome message:\n\n{Formatter.Italic(msg ?? "Not set.")}")
                            .ConfigureAwait(false);
                    } else {
                        if (message.Length < 3 || message.Length > 120)
                            throw new CommandFailedException("Message cannot be shorter than 3 or longer than 120 characters!");

                        await Database.SetWelcomeMessageAsync(ctx.Guild.Id, message)
                            .ConfigureAwait(false);

                        await ctx.RespondWithIconEmbedAsync($"Welcome message set to: {Formatter.Bold(message ?? "Default message")}.")
                            .ConfigureAwait(false);
                    }
                }
                #endregion

                #region COMMAND_WELCOME_ENABLE
                [Command("enable"), Module(ModuleType.Administration)]
                [Description("Enables member welcoming for this guild. Provide a channel to send the messages to and optional custom welcome message. Any occurances of ``%user%`` inside the message will be replaced with appropriate mention.")]
                [Aliases("on")]
                [UsageExample("!guild cfg welcome on")]
                [UsageExample("!guild cfg welcome on #lobby Welcome, %user%!")]
                public async Task EnableAsync(CommandContext ctx,
                                             [Description("Channel.")] DiscordChannel channel = null,
                                             [RemainingText, Description("Welcome message.")] string message = null)
                {
                    if (channel == null)
                        channel = ctx.Channel;

                    if (channel.Type != ChannelType.Text)
                        throw new CommandFailedException("Welcome channel must be a text channel.");

                    await Database.SetWelcomeChannelAsync(ctx.Guild.Id, channel.Id)
                        .ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(message)) {
                        if (message.Length < 3 || message.Length > 120)
                            throw new CommandFailedException("Message cannot be shorter than 3 or longer than 120 characters!");
                        await Database.SetWelcomeMessageAsync(ctx.Guild.Id, message)
                            .ConfigureAwait(false);
                    }

                    await ctx.RespondWithIconEmbedAsync($"Welcome message channel set to {Formatter.Bold(channel.Name)} with message: {Formatter.Bold(string.IsNullOrWhiteSpace(message) ? "<previously set>" : message)}.")
                        .ConfigureAwait(false);
                }
                #endregion

                #region COMMAND_WELCOME_DISABLE
                [Command("disable"), Module(ModuleType.Administration)]
                [Description("Disables member welcome messages for this guild.")]
                [Aliases("off")]
                [UsageExample("!guild cfg welcome off")]
                public async Task DisableAsync(CommandContext ctx)
                {
                    await Database.RemoveWelcomeChannelAsync(ctx.Guild.Id)
                        .ConfigureAwait(false);
                    await Database.RemoveWelcomeMessageAsync(ctx.Guild.Id)
                        .ConfigureAwait(false);
                }
                #endregion
            }
            #endregion

            #region GROUP_CONFIG_LEAVE
            [Group("leave"), Module(ModuleType.Administration)]
            [Description("Allows user leaving message configuration.")]
            [Aliases("exit", "drop", "lv", "l")]
            [UsageExample("!guild cfg leave")]
            public class Leave : TheGodfatherBaseModule
            {

                public Leave(SharedData shared, DBService db) : base(shared, db) { }


                [GroupCommand]
                public async Task ExecuteGroupAsync(CommandContext ctx)
                {
                    ulong cid = await Database.GetLeaveChannelIdAsync(ctx.Guild.Id)
                        .ConfigureAwait(false);
                    await ctx.RespondWithIconEmbedAsync($"Member leave messages for this guild are: {Formatter.Bold(cid != 0 ? "enabled" : "disabled")}!")
                        .ConfigureAwait(false);
                }


                #region COMMAND_LEAVE_CHANNEL
                [Command("channel"), Module(ModuleType.Administration)]
                [Description("Gets or sets leave message channel.")]
                [Aliases("chn", "c")]
                [UsageExample("!guild cfg leave channel")]
                [UsageExample("!guild cfg leave channel #lobby")]
                public async Task ChannelAsync(CommandContext ctx,
                                              [Description("Channel.")] DiscordChannel channel = null)
                {
                    if (channel == null) {
                        ulong cid = await Database.GetLeaveChannelIdAsync(ctx.Guild.Id)
                            .ConfigureAwait(false);
                        if (cid != 0) {
                            var c = ctx.Guild.GetChannel(cid);
                            if (c == null)
                                throw new CommandFailedException($"Leave channel was set but does not exist anymore (id: {cid}).");
                            await ctx.RespondWithIconEmbedAsync($"Leave message channel: {c.Mention}.")
                                .ConfigureAwait(false);
                        } else {
                            await ctx.RespondWithIconEmbedAsync("Leave message channel isn't set for this guild.")
                                .ConfigureAwait(false);
                        }
                    } else {
                        if (channel.Type != ChannelType.Text)
                            throw new CommandFailedException("Leave channel must be a text channel.");

                        await Database.SetLeaveChannelAsync(ctx.Guild.Id, channel.Id)
                            .ConfigureAwait(false);
                        await ctx.RespondWithIconEmbedAsync($"Leave message channel set to {channel.Mention}.")
                            .ConfigureAwait(false);
                    }
                }
                #endregion

                #region COMMAND_LEAVE_MESSAGE
                [Command("message"), Module(ModuleType.Administration)]
                [Description("Gets or sets current leave message.")]
                [Aliases("msg", "m")]
                [UsageExample("!guild cfg leave message")]
                [UsageExample("!guild cfg leave message Bye, %user%!")]
                public async Task MessageAsync(CommandContext ctx,
                                              [RemainingText, Description("Leave message.")] string message = null)
                {
                    if (string.IsNullOrWhiteSpace(message)) {
                        var msg = await Database.GetLeaveMessageAsync(ctx.Guild.Id)
                            .ConfigureAwait(false);

                        await ctx.RespondWithIconEmbedAsync($"Leave message:\n\n{Formatter.Italic(msg ?? "Not set.")}")
                            .ConfigureAwait(false);
                    } else {
                        if (message.Length < 3 || message.Length > 120)
                            throw new CommandFailedException("Message cannot be shorter than 3 or longer than 120 characters!");

                        await Database.SetLeaveMessageAsync(ctx.Guild.Id, message)
                            .ConfigureAwait(false);

                        await ctx.RespondWithIconEmbedAsync($"Leave message set to: {Formatter.Bold(message ?? "Default message")}.")
                            .ConfigureAwait(false);
                    }
                }
                #endregion

                #region COMMAND_LEAVE_ENABLE
                [Command("enable"), Module(ModuleType.Administration)]
                [Description("Enables member leave messages for this guild. Provide a channel to send the messages to and optional custom leave message. Any occurances of ``%user%`` inside the message will be replaced with appropriate mention.")]
                [Aliases("on")]
                [UsageExample("!guild cfg leave on")]
                [UsageExample("!guild cfg leave on #lobby Welcome, %user%!")]
                public async Task EnableAsync(CommandContext ctx,
                                             [Description("Channel.")] DiscordChannel channel = null,
                                             [RemainingText, Description("Leave message.")] string message = null)
                {
                    if (channel == null)
                        channel = ctx.Channel;

                    if (channel.Type != ChannelType.Text)
                        throw new CommandFailedException("Leave channel must be a text channel.");

                    await Database.SetLeaveChannelAsync(ctx.Guild.Id, channel.Id)
                        .ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(message)) {
                        if (message.Length < 3 || message.Length > 120)
                            throw new CommandFailedException("Message cannot be shorter than 3 or longer than 120 characters!");
                        await Database.SetLeaveMessageAsync(ctx.Guild.Id, message)
                            .ConfigureAwait(false);
                    }

                    await ctx.RespondWithIconEmbedAsync($"Leave message channel set to {Formatter.Bold(channel.Name)} with message: {Formatter.Bold(string.IsNullOrWhiteSpace(message) ? "<previously set>" : message)}.")
                        .ConfigureAwait(false);
                }
                #endregion

                #region COMMAND_LEAVE_DISABLE
                [Command("disable"), Module(ModuleType.Administration)]
                [Description("Disables member leave messages for this guild.")]
                [Aliases("off")]
                [UsageExample("!guild cfg leave off")]
                public async Task DisableAsync(CommandContext ctx)
                {
                    await Database.RemoveLeaveChannelAsync(ctx.Guild.Id)
                        .ConfigureAwait(false);
                    await Database.RemoveLeaveMessageAsync(ctx.Guild.Id)
                        .ConfigureAwait(false);
                }
                #endregion
            }
            #endregion
        }
    }
}
