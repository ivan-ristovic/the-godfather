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
using DSharpPlus.Net.Models;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class GuildModule
    {
        [Group("configure"), Module(ModuleType.Administration)]
        [Description("Allows manipulation of guild settings for this bot. If invoked without subcommands, starts an interactive settings setup for this guild.")]
        [Aliases("config", "cfg", "setup")]
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
                // TODO
            }


            #region GROUP_CONFIG_SUGGESTIONS
            [Group("suggestions"), Module(ModuleType.Administration)]
            [Description("Suggestions configuration commands.")]
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
                [Description("Gets or sets current welcome message channel.")]
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
                            await ctx.RespondWithIconEmbedAsync($"Welcome message channel: {Formatter.Bold(ctx.Guild.GetChannel(cid).Name)}.")
                                .ConfigureAwait(false);
                        } else {
                            await ctx.RespondWithIconEmbedAsync("Welcome message channel isn't set for this guild.")
                                .ConfigureAwait(false);
                        }
                    } else {
                        if (channel == null)
                            channel = ctx.Channel;

                        if (channel.Type != ChannelType.Text)
                            throw new CommandFailedException("Welcome channel must be a text channel.");

                        await Database.SetWelcomeChannelAsync(ctx.Guild.Id, channel.Id)
                            .ConfigureAwait(false);
                        await ctx.RespondWithIconEmbedAsync($"Welcome message channel set to {Formatter.Bold(channel.Name)}.")
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
                [Description("Gets or sets current leave message channel.")]
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
                            await ctx.RespondWithIconEmbedAsync($"Leave message channel: {Formatter.Bold(ctx.Guild.GetChannel(cid).Name)}.")
                                .ConfigureAwait(false);
                        } else {
                            await ctx.RespondWithIconEmbedAsync("Leave message channel isn't set for this guild.")
                                .ConfigureAwait(false);
                        }
                    } else {
                        if (channel == null)
                            channel = ctx.Channel;

                        if (channel.Type != ChannelType.Text)
                            throw new CommandFailedException("Leave channel must be a text channel.");

                        await Database.SetLeaveChannelAsync(ctx.Guild.Id, channel.Id)
                            .ConfigureAwait(false);
                        await ctx.RespondWithIconEmbedAsync($"Leave message channel set to {Formatter.Bold(channel.Name)}.")
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
