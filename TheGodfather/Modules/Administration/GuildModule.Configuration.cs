#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text;
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
using DSharpPlus.Interactivity;
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
        [NotBlocked]
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

                ulong wcid = await Database.GetWelcomeChannelIdAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
                emb.AddField("Welcome messages", wcid != 0 ? "on" : "off", inline: true);

                ulong lcid = await Database.GetLeaveChannelIdAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
                emb.AddField("Leave messages", lcid != 0 ? "on" : "off", inline: true);

                if (gcfg.LinkfilterEnabled) {
                    var sb = new StringBuilder();
                    if (gcfg.BlockDiscordInvites)
                        sb.AppendLine("Invite blocker");
                    if (gcfg.BlockBooterWebsites)
                        sb.AppendLine("DDoS/Booter website blocker");
                    if (gcfg.BlockDisturbingWebsites)
                        sb.AppendLine("Disturbing website blocker");
                    if (gcfg.BlockIpLoggingWebsites)
                        sb.AppendLine("IP logging website blocker");
                    if (gcfg.BlockUrlShorteners)
                        sb.AppendLine("URL shortening website blocker");
                    emb.AddField("Linkfilter modules active", sb.Length > 0 ? sb.ToString() : "None", inline: true);
                } else {
                    emb.AddField("Linkfilter", "off", inline: true);
                }

                await ctx.RespondAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }


            #region COMMAND_CONFIG_WIZARD
            [Command("setup"), Module(ModuleType.Administration)]
            [Description("Starts an interactive wizard for configuring the guild settings.")]
            [Aliases("wizard")]
            [UsageExample("!guild cfg setup")]
            [InteractivitySensitive]
            public async Task SetupAsync(CommandContext ctx)
            {
                var channel = ctx.Guild.Channels.FirstOrDefault(c => c.Name == "gf_setup");
                if (channel == null) {
                    if (await ctx.AskYesNoQuestionAsync($"Before we start, if you want to move this somewhere else, would you like me to create a temporary public blank channel for the setup? Alternatively, if you do not want that channel to be public, you can create the channel yourself with name {Formatter.Bold("gf_setup")} and whatever permissions you like, just let me access it. Please reply with yes if you wish for me to create the channel or with no if you plan to do it yourself.").ConfigureAwait(false)) {
                        try {
                            channel = await ctx.Guild.CreateChannelAsync("gf_setup", ChannelType.Text, reason: "TheGodfather setup channel creation.")
                                .ConfigureAwait(false);
                            await ctx.RespondWithIconEmbedAsync($"Alright, let's move the setup to {channel.Mention}")
                                .ConfigureAwait(false);
                        } catch {
                            await ctx.RespondWithFailedEmbedAsync($"I have failed to create a setup channel. Could you kindly create the channel called {Formatter.Bold("gf_setup")} and then re-run the command or give me the permission to create channels? The wizard will now exit...")
                                .ConfigureAwait(false);
                            return;
                        }
                    } else {
                        channel = ctx.Channel;
                    }
                }

                var gcfg = CachedGuildConfig.Default;
                await channel.SendIconEmbedAsync("Welcome to the guild configuration wizard!\n\nI will guide you through the configuration. You can always re-run this setup or manually change the settings so do not worry if you don't do everything like you wanted.\n\nThat being said, let's start the fun! Note that the changes will apply after the wizard finishes.")
                    .ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(10))
                    .ConfigureAwait(false);

                if (await channel.AskYesNoQuestionAsync(ctx, "Do you wish to change the prefix for the bot? (y/n)")) {
                    await channel.SendIconEmbedAsync("What will the new prefix be?")
                        .ConfigureAwait(false);
                    var mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(m => m.ChannelId == channel.Id && m.Author.Id == ctx.User.Id)
                        .ConfigureAwait(false);
                    gcfg.Prefix = mctx?.Message.Content;
                }

                if (await channel.AskYesNoQuestionAsync(ctx, "Do you wish to enable command suggestions for those nasty times when you just can't remember the command name? (y/n)"))
                    gcfg.SuggestionsEnabled = true;

                if (await channel.AskYesNoQuestionAsync(ctx, "I can log the actions that happen in the guild (such as message deletion, channel updates etc.), so you always know what is going on in the guild. Do you wish to enable the action log? (y/n)")) {
                    await channel.SendIconEmbedAsync($"Alright, cool. In order for action logs to work you will need to tell me where to dump the log. Please reply with a channel mention, for example {Formatter.Bold("#logs")} .")
                        .ConfigureAwait(false);
                    var mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(m => m.ChannelId == channel.Id && m.Author.Id == ctx.User.Id && m.MentionedChannels.Count == 1)
                        .ConfigureAwait(false);
                    gcfg.LogChannelId = mctx.MentionedChannels.FirstOrDefault()?.Id ?? 0;
                }

                ulong wcid = 0;
                string wmessage = null;
                if (await channel.AskYesNoQuestionAsync(ctx, "I can also send a welcome message when someone joins the guild. Do you wish to enable this feature? (y/n)")) {
                    await channel.SendIconEmbedAsync($"I will need a channel where to send the welcome messages. Please reply with a channel mention, for example {Formatter.Bold("#logs")} .")
                        .ConfigureAwait(false);
                    var mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(m => m.ChannelId == channel.Id && m.Author.Id == ctx.User.Id && m.MentionedChannels.Count == 1)
                        .ConfigureAwait(false);

                    wcid = mctx?.MentionedChannels.FirstOrDefault()?.Id ?? 0;
                    if (wcid != 0 && ctx.Guild.GetChannel(wcid).Type != ChannelType.Text) {
                        await channel.SendFailedEmbedAsync("You need to provide a text channel!")
                            .ConfigureAwait(false);
                        wcid = 0;
                    }

                    if (await channel.AskYesNoQuestionAsync(ctx, "You can also customize the welcome message. Do you want to do that now? (y/n)")) {
                        await channel.SendIconEmbedAsync($"Tell me what message you want me to send when someone joins the guild. Note that you can use the wildcard {Formatter.Bold("%user%")} and I will replace it with the mention for the member who joined.")
                            .ConfigureAwait(false);
                        mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(m => m.ChannelId == channel.Id && m.Author.Id == ctx.User.Id)
                            .ConfigureAwait(false);
                        wmessage = mctx?.Message?.Content;
                    }
                }

                ulong lcid = 0;
                string lmessage = null;
                if (await channel.AskYesNoQuestionAsync(ctx, "The same applies for member leave messages. Do you wish to enable this feature? (y/n)")) {
                    await channel.SendIconEmbedAsync($"I will need a channel where to send the leave messages. Please reply with a channel mention, for example {Formatter.Bold("#logs")} .")
                        .ConfigureAwait(false);
                    var mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(m => m.ChannelId == channel.Id && m.Author.Id == ctx.User.Id && m.MentionedChannels.Count == 1)
                        .ConfigureAwait(false);

                    lcid = mctx?.MentionedChannels.FirstOrDefault()?.Id ?? 0;
                    if (lcid != 0 && ctx.Guild.GetChannel(lcid).Type != ChannelType.Text) {
                        await channel.SendFailedEmbedAsync("You need to provide a text channel!")
                            .ConfigureAwait(false);
                        lcid = 0;
                    }

                    if (await channel.AskYesNoQuestionAsync(ctx, "You can also customize the leave message. Do you want to do that now? (y/n)")) {
                        await channel.SendIconEmbedAsync($"Tell me what message you want me to send when someone leaves the guild. Note that you can use the wildcard {Formatter.Bold("%user%")} and I will replace it with the mention for the member who left.")
                            .ConfigureAwait(false);
                        mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(m => m.ChannelId == channel.Id && m.Author.Id == ctx.User.Id)
                            .ConfigureAwait(false);
                        lmessage = mctx?.Message?.Content;
                    }
                }

                if (await channel.AskYesNoQuestionAsync(ctx, "Do you wish to enable link filtering? (y/n)")) {
                    gcfg.LinkfilterEnabled = true;
                    if (await channel.AskYesNoQuestionAsync(ctx, "Do you wish to enable Discord invite links filtering? (y/n)"))
                        gcfg.BlockDiscordInvites = true;
                    else
                        gcfg.BlockDiscordInvites = false;
                    if (await channel.AskYesNoQuestionAsync(ctx, "Do you wish to enable DDoS/Booter websites filtering? (y/n)"))
                        gcfg.BlockBooterWebsites = true;
                    else
                        gcfg.BlockBooterWebsites = false;
                    if (await channel.AskYesNoQuestionAsync(ctx, "Do you wish to enable IP logging websites filtering? (y/n)"))
                        gcfg.BlockIpLoggingWebsites = true;
                    else
                        gcfg.BlockIpLoggingWebsites = false;
                    if (await channel.AskYesNoQuestionAsync(ctx, "Do you wish to enable disturbing/shock/gore websites filtering? (y/n)"))
                        gcfg.BlockDisturbingWebsites = true;
                    else
                        gcfg.BlockDisturbingWebsites = false;
                    if (await channel.AskYesNoQuestionAsync(ctx, "Do you wish to enable URL shorteners filtering? (y/n)"))
                        gcfg.BlockUrlShorteners = true;
                    else
                        gcfg.BlockUrlShorteners = false;
                }

                var sb = new StringBuilder();
                sb.Append("Prefix: ").AppendLine(Formatter.Bold(gcfg.Prefix ?? Shared.BotConfiguration.DefaultPrefix));
                sb.Append("Command suggestions: ").AppendLine(Formatter.Bold((gcfg.SuggestionsEnabled ? "on" : "off")));
                sb.Append("Action logging: ");
                if (gcfg.LoggingEnabled) {
                    sb.Append(Formatter.Bold("on")).Append(" in channel ").AppendLine(ctx.Guild.GetChannel(gcfg.LogChannelId).Mention);
                } else {
                    sb.AppendLine(Formatter.Bold("off"));
                }
                if (wcid != 0) {
                    sb.AppendLine($"Welcome messages {Formatter.Bold("enabled")} in {ctx.Guild.GetChannel(wcid).Mention}");
                    sb.AppendLine($"Welcome message: {Formatter.BlockCode(wmessage ?? "default")}");
                } else {
                    sb.AppendLine($"Welcome messages: {Formatter.Bold("disabled")}");
                }
                if (lcid != 0) {
                    sb.AppendLine($"Leave messages {Formatter.Bold("enabled")} in {ctx.Guild.GetChannel(lcid).Mention}");
                    sb.AppendLine($"Leave message: {Formatter.BlockCode(lmessage ?? "default")}");
                } else {
                    sb.AppendLine($"Leave messages: {Formatter.Bold("disabled")}");
                }
                if (gcfg.LinkfilterEnabled) {
                    sb.AppendLine(Formatter.Bold("enabled"));
                    sb.Append(" - Discord invites blocker: ").AppendLine(gcfg.BlockDiscordInvites ? "on" : "off");
                    sb.Append(" - DDoS/Booter websites blocker: ").AppendLine(gcfg.BlockBooterWebsites ? "on" : "off");
                    sb.Append(" - IP logging websites blocker: ").AppendLine(gcfg.BlockIpLoggingWebsites ? "on" : "off");
                    sb.Append(" - Disturbing websites blocker: ").AppendLine(gcfg.BlockDisturbingWebsites ? "on" : "off");
                    sb.Append(" - URL shorteners blocker: ").AppendLine(gcfg.BlockUrlShorteners ? "on" : "off");
                    sb.AppendLine();
                } else {
                    sb.AppendLine(Formatter.Bold("disabled"));
                }

                await channel.SendIconEmbedAsync($"Selected settings:\n\n{sb.ToString()}")
                    .ConfigureAwait(false);

                if (await channel.AskYesNoQuestionAsync(ctx, "We are almost done! Please review the settings above and say whether you want me to apply them. (y/n)")) {
                    await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                        .ConfigureAwait(false);
                    await Database.SetWelcomeChannelAsync(ctx.Guild.Id, wcid)
                        .ConfigureAwait(false);
                    await Database.SetWelcomeMessageAsync(ctx.Guild.Id, wmessage)
                        .ConfigureAwait(false);
                    await Database.SetLeaveChannelAsync(ctx.Guild.Id, lcid)
                        .ConfigureAwait(false);
                    await Database.SetLeaveMessageAsync(ctx.Guild.Id, lmessage)
                        .ConfigureAwait(false);
                }

                var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                .ConfigureAwait(false);
                if (logchn != null) {
                    var emb = new DiscordEmbedBuilder() {
                        Title = "Guild config changed",
                        Color = DiscordColor.Brown
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Prefix", Shared.GetGuildPrefix(ctx.Guild.Id), inline: true);
                    emb.AddField("Command suggestions", gcfg.SuggestionsEnabled ? "on" : "off", inline: true);
                    emb.AddField("Action logging", gcfg.LoggingEnabled ? "on" : "off", inline: true);
                    emb.AddField("Welcome messages", wcid != 0 ? "on" : "off", inline: true);
                    emb.AddField("Leave messages", lcid != 0 ? "on" : "off", inline: true);
                    emb.AddField("Linkfilter", gcfg.LinkfilterEnabled ? "on" : "off", inline: true);
                    emb.AddField("Linkfilter - Block invites", gcfg.BlockDiscordInvites ? "on" : "off", inline: true);
                    await logchn.SendMessageAsync(embed: emb.Build())
                        .ConfigureAwait(false);
                }

                await channel.SendIconEmbedAsync($"All done! Have a nice day!", StaticDiscordEmoji.CheckMarkSuccess)
                    .ConfigureAwait(false);
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

                    var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                    .ConfigureAwait(false);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = DiscordColor.Brown
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Command suggestions", gcfg.SuggestionsEnabled ? "on" : "off", inline: true);
                        await logchn.SendMessageAsync(embed: emb.Build())
                            .ConfigureAwait(false);
                    }

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

                    var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                    .ConfigureAwait(false);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = DiscordColor.Brown
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Command suggestions", gcfg.SuggestionsEnabled ? "on" : "off", inline: true);
                        await logchn.SendMessageAsync(embed: emb.Build())
                            .ConfigureAwait(false);
                    }

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

                    var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                     .ConfigureAwait(false);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = DiscordColor.Brown
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Logging", "Enabled", inline: true);
                        emb.AddField("Logging channel", gcfg.LogChannelId.ToString(), inline: true);
                        await logchn.SendMessageAsync(embed: emb.Build())
                            .ConfigureAwait(false);
                    }

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

                    var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                     .ConfigureAwait(false);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = DiscordColor.Brown
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Logging", "disabled", inline: true);
                        emb.AddField("Logging channel", gcfg.LogChannelId.ToString(), inline: true);
                        await logchn.SendMessageAsync(embed: emb.Build())
                            .ConfigureAwait(false);
                    }

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

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                         .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("Logging channel", gcfg.LogChannelId.ToString(), inline: true);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

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

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                         .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("Welcome message channel", channel.Mention, inline: true);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

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

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                         .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("Welcome message", message);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

                        await ctx.RespondWithIconEmbedAsync($"Welcome message set to:\n{Formatter.Bold(message ?? "Default message")}.")
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

                    var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                     .ConfigureAwait(false);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = DiscordColor.Brown
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Welcome messages", "Enabled", inline: true);
                        emb.AddField("Welcome message channel", channel.Mention, inline: true);
                        emb.AddField("Welcome message", message ?? Formatter.Italic("default"));
                        await logchn.SendMessageAsync(embed: emb.Build())
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

                    var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                     .ConfigureAwait(false);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = DiscordColor.Brown
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Welcome messages", "disabled", inline: true);
                        await logchn.SendMessageAsync(embed: emb.Build())
                            .ConfigureAwait(false);
                    }
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
                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                         .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("Leave message channel", channel.Mention, inline: true);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

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

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                         .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("Leave message", message);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

                        await ctx.RespondWithIconEmbedAsync($"Leave message set to:\n{Formatter.Bold(message ?? "Default message")}.")
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

                    var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                     .ConfigureAwait(false);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = DiscordColor.Brown
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Leave messages", "Enabled", inline: true);
                        emb.AddField("Leave message channel", channel.Mention, inline: true);
                        emb.AddField("Leave message", message ?? Formatter.Italic("default"));
                        await logchn.SendMessageAsync(embed: emb.Build())
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

                    var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                     .ConfigureAwait(false);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = DiscordColor.Brown
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Leave messages", "disabled", inline: true);
                        await logchn.SendMessageAsync(embed: emb.Build())
                            .ConfigureAwait(false);
                    }
                }
                #endregion
            }
            #endregion
        }
    }
}
