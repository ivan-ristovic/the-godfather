#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Common.Converters;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class GuildModule
    {
        [Group("configure")]
        [Description("Allows manipulation of guild settings for this bot. If invoked without subcommands, lists the current guild configuration.")]
        [Aliases("configuration", "config", "cfg")]
        [UsageExamples("!guild configure")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public partial class GuildConfigModule : TheGodfatherModule
        {

            public GuildConfigModule(SharedData shared, DBService db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.SapGreen;
            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.PrintGuildConfigAsync(ctx.Guild, ctx.Channel);


            #region COMMAND_CONFIG_WIZARD
            [Command("setup"), UsesInteractivity]
            [Description("Starts an interactive wizard for configuring the guild settings.")]
            [Aliases("wizard")]
            [UsageExamples("!guild cfg setup")]
            public async Task SetupAsync(CommandContext ctx)
            {
                DiscordChannel channel = await this.ChooseSetupChannelAsync(ctx);

                var gcfg = CachedGuildConfig.Default;
                await channel.EmbedAsync("Welcome to the guild configuration wizard!\n\nI will guide you " +
                                         "through the configuration. You can always re-run this setup or " +
                                         "manually change the settings so do not worry if you don't do " +
                                         "everything like you wanted.\n\nThat being said, let's start the " +
                                         "fun! Note that the changes will apply after the wizard finishes.");
                await Task.Delay(TimeSpan.FromSeconds(10));

                await this.SetupVerboseRepliesAsync(gcfg, ctx, channel);
                await this.SetupPrefixAsync(gcfg, ctx, channel);
                await this.SetupCommandSuggestionsAsync(gcfg, ctx, channel);
                await this.SetupLoggingAsync(gcfg, ctx, channel);
                JoinLeaveSettings msgSettings = await this.SetupMemberUpdateMessagesAsync(gcfg, ctx, channel);
                DiscordRole muteRole = await this.SetupMuteRoleAsync(gcfg, ctx, channel);
                await this.SetupLinkfilterAsync(gcfg, ctx, channel);
                await this.SetupRatelimitAsync(gcfg, ctx, channel);
                AntifloodSettings antifloodSettings = await this.SetupAntifloodAsync(gcfg, ctx, channel);
                await this.SetupCurrencyAsync(gcfg, ctx, channel);
                AntiInstantLeaveSettings antiInstantLeaveSettings = await this.SetupAntiInstantLeaveAsync(gcfg, ctx, channel);

                await this.PreviewSettingsAsync(gcfg, ctx, channel, muteRole, msgSettings, antifloodSettings, antiInstantLeaveSettings);
                if (await channel.WaitForBoolResponseAsync(ctx, "We are almost done! Please review the settings above and say whether you want me to apply them.")) {
                    await this.ApplySettingsAsync(ctx.Guild.Id, gcfg, muteRole, msgSettings, antifloodSettings, antiInstantLeaveSettings);

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (logchn != null)
                        await this.PrintGuildConfigAsync(ctx.Guild, logchn, changed: true);

                    await channel.EmbedAsync($"All done! Have a nice day!", StaticDiscordEmoji.CheckMarkSuccess);
                }
            }
            #endregion

            #region COMMAND_CONFIG_VERBOSE
            [Command("verbose"), Priority(1)]
            [Description("Configuration of bot's responding options.")]
            [Aliases("fullresponse", "verbosereact", "verboseresponse", "v", "vr")]
            [UsageExamples("!guild cfg verbose",
                           "!guild cfg verbose on")]
            public async Task SilentResponseAsync(CommandContext ctx,
                                                 [Description("Enable silent response?")] bool enable)
            {
                CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                gcfg.ReactionResponse = !enable;

                await this.Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg);

                DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                if (logchn != null) {
                    var emb = new DiscordEmbedBuilder() {
                        Title = "Guild config changed",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Verbose response", gcfg.ReactionResponse ? "off" : "on", inline: true);
                    await logchn.SendMessageAsync(embed: emb.Build());
                }

                await this.InformAsync(ctx, $"{Formatter.Bold(gcfg.ReactionResponse ? "Disabled" : "Enabled")} verbose responses.", important: false);
            }

            [Command("verbose"), Priority(0)]
            public Task SilentResponseAsync(CommandContext ctx)
            {
                CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                return this.InformAsync(ctx, $"Verbose responses for this guild are {Formatter.Bold(gcfg.ReactionResponse ? "disabled" : "enabled")}!");
            }
            #endregion

            #region COMMAND_CONFIG_SUGGESTIONS
            [Command("suggestions"), Priority(1)]
            [Description("Command suggestions configuration.")]
            [Aliases("suggestion", "cmdsug", "sugg", "sug", "cs", "s")]
            [UsageExamples("!guild cfg suggestions",
                           "!guild cfg suggestions on")]
            public async Task SuggestionsAsync(CommandContext ctx,
                                              [Description("Enable suggestions?")] bool enable)
            {
                CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                gcfg.SuggestionsEnabled = enable;

                await this.Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg);

                DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                if (logchn != null) {
                    var emb = new DiscordEmbedBuilder() {
                        Title = "Guild config changed",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Command suggestions", gcfg.SuggestionsEnabled ? "on" : "off", inline: true);
                    await logchn.SendMessageAsync(embed: emb.Build());
                }

                await this.InformAsync(ctx, $"{Formatter.Bold(gcfg.SuggestionsEnabled ? "Enabled" : "Disabled")} command suggestions.", important: false);
            }

            [Command("suggestions"), Priority(0)]
            public Task SuggestionsAsync(CommandContext ctx)
            {
                CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                return this.InformAsync(ctx, $"Command suggestions for this guild are {Formatter.Bold(gcfg.SuggestionsEnabled ? "enabled" : "disabled")}!");
            }
            #endregion

            #region COMMAND_CONFIG_WELCOME
            [Command("welcome"), Priority(3)]
            [Description("Allows user welcoming configuration.")]
            [Aliases("enter", "join", "wlc", "wm", "w")]
            [UsageExamples("!guild cfg welcome",
                           "!guild cfg welcome on #general",
                           "!guild cfg welcome Welcome, %user%!",
                           "!guild cfg welcome off")]
            public async Task WelcomeAsync(CommandContext ctx)
            {
                DiscordChannel wchn = await this.Database.GetWelcomeChannelAsync(ctx.Guild);
                await this.InformAsync(ctx, $"Member welcome messages for this guild are: {Formatter.Bold(wchn != null ? $"enabled @ {wchn.Mention}" : "disabled")}!");
            }

            [Command("welcome"), Priority(2)]
            public async Task WelcomeAsync(CommandContext ctx,
                                          [Description("Enable welcoming?")] bool enable,
                                          [Description("Channel.")] DiscordChannel wchn = null,
                                          [RemainingText, Description("Welcome message.")] string message = null)
            {
                wchn = wchn ?? ctx.Channel;

                if (wchn.Type != ChannelType.Text)
                    throw new CommandFailedException("Welcome channel must be a text channel.");

                if (!string.IsNullOrWhiteSpace(message)) {
                    if (message.Length < 3 || message.Length > 120)
                        throw new CommandFailedException("Message cannot be shorter than 3 or longer than 120 characters!");
                    await this.Database.SetWelcomeMessageAsync(ctx.Guild.Id, message);
                }

                if (enable)
                    await this.Database.SetWelcomeChannelAsync(ctx.Guild.Id, wchn.Id);
                else
                    await this.Database.RemoveWelcomeChannelAsync(ctx.Guild.Id);

                DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                if (logchn != null) {
                    var emb = new DiscordEmbedBuilder() {
                        Title = "Guild config changed",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("User welcoming", enable ? $"on @ {wchn.Mention}" : "off", inline: true);
                    emb.AddField("Welcome message", message ?? Formatter.Italic("default"));
                    await logchn.SendMessageAsync(embed: emb.Build());
                }

                if (enable)
                    await this.InformAsync(ctx, $"Welcome message channel set to {Formatter.Bold(wchn.Name)} with message: {Formatter.Bold(string.IsNullOrWhiteSpace(message) ? "<previously set>" : message)}.", important: false);
                else
                    await this.InformAsync(ctx, $"Welcome messages are now disabled.", important: false);
            }

            [Command("welcome"), Priority(1)]
            public Task WelcomeAsync(CommandContext ctx,
                                    [Description("Channel.")] DiscordChannel channel,
                                    [RemainingText, Description("Welcome message.")] string message = null)
                => this.WelcomeAsync(ctx, true, channel, message);

            [Command("welcome"), Priority(0)]
            public async Task WelcomeAsync(CommandContext ctx,
                                          [RemainingText, Description("Welcome message.")] string message)
            {
                if (message.Length < 3 || message.Length > 120)
                    throw new CommandFailedException("Message cannot be shorter than 3 or longer than 120 characters!");

                await this.Database.SetWelcomeMessageAsync(ctx.Guild.Id, message);

                DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                if (logchn != null) {
                    var emb = new DiscordEmbedBuilder() {
                        Title = "Guild config changed",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Welcome message set to", message);
                    await logchn.SendMessageAsync(embed: emb.Build());
                }

                await this.InformAsync(ctx, $"Welcome message set to:\n{Formatter.Bold(message ?? "Default message")}.", important: false);
            }

            #endregion

            #region COMMAND_CONFIG_LEAVE
            [Command("leave"), Priority(3)]
            [Description("Allows user leaving message configuration.")]
            [Aliases("exit", "drop", "lvm", "lm", "l")]
            [UsageExamples("!guild cfg leave",
                           "!guild cfg leave on #general",
                           "!guild cfg leave Welcome, %user%!",
                           "!guild cfg leave off")]
            public async Task LeaveAsync(CommandContext ctx)
            {
                DiscordChannel lchn = await this.Database.GetLeaveChannelAsync(ctx.Guild);
                await this.InformAsync(ctx, $"Member leave messages for this guild are: {Formatter.Bold(lchn != null ? $"enabled @ {lchn.Mention}" : "disabled")}!");
            }

            [Command("leave"), Priority(2)]
            public async Task LeaveAsync(CommandContext ctx,
                                        [Description("Enable leave messages?")] bool enable,
                                        [Description("Channel.")] DiscordChannel lchn = null,
                                        [RemainingText, Description("Leave message.")] string message = null)
            {
                lchn = lchn ?? ctx.Channel;

                if (lchn.Type != ChannelType.Text)
                    throw new CommandFailedException("Leave channel must be a text channel.");

                if (!string.IsNullOrWhiteSpace(message)) {
                    if (message.Length < 3 || message.Length > 120)
                        throw new CommandFailedException("Message cannot be shorter than 3 or longer than 120 characters!");
                    await this.Database.SetLeaveMessageAsync(ctx.Guild.Id, message);
                }

                if (enable)
                    await this.Database.SetLeaveChannelAsync(ctx.Guild.Id, lchn.Id);
                else
                    await this.Database.RemoveLeaveChannelAsync(ctx.Guild.Id);

                DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                if (logchn != null) {
                    var emb = new DiscordEmbedBuilder() {
                        Title = "Guild config changed",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("User leave messages", enable ? $"on @ {lchn.Mention}" : "off", inline: true);
                    emb.AddField("Leave message", message ?? Formatter.Italic("default"));
                    await logchn.SendMessageAsync(embed: emb.Build());
                }

                if (enable)
                    await this.InformAsync(ctx, $"Welcome message channel set to {Formatter.Bold(lchn.Name)} with message: {Formatter.Bold(string.IsNullOrWhiteSpace(message) ? "<previously set>" : message)}.", important: false);
                else
                    await this.InformAsync(ctx, $"Welcome messages are now disabled.", important: false);
            }

            [Command("leave"), Priority(1)]
            public Task LeaveAsync(CommandContext ctx,
                                  [Description("Channel.")] DiscordChannel channel,
                                  [RemainingText, Description("Leave message.")] string message)
                => this.LeaveAsync(ctx, true, channel, message);

            [Command("leave"), Priority(0)]
            public async Task LeaveAsync(CommandContext ctx,
                                        [RemainingText, Description("Leave message.")] string message)
            {
                if (message.Length < 3 || message.Length > 120)
                    throw new CommandFailedException("Message cannot be shorter than 3 or longer than 120 characters!");

                await this.Database.SetLeaveMessageAsync(ctx.Guild.Id, message);

                DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                if (logchn != null) {
                    var emb = new DiscordEmbedBuilder() {
                        Title = "Guild config changed",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Leave message set to", message);
                    await logchn.SendMessageAsync(embed: emb.Build());
                }

                await this.InformAsync(ctx, $"Leave message set to:\n{Formatter.Bold(message ?? "Default message")}.", important: false);
            }
            #endregion

            #region COMMAND_CONFIG_MUTEROLE
            [Command("setmuterole")]
            [Description("Gets or sets mute role for this guild.")]
            [Aliases("muterole", "mr", "muterl", "mrl")]
            [UsageExamples("!guild cfg muterole",
                           "!guild cfg muterole MuteRoleName")]
            public async Task GetOrSetMuteRoleAsync(CommandContext ctx,
                                                   [Description("New mute role.")] DiscordRole muteRole = null)
            {
                if (muteRole != null) {
                    await this.Database.SetMuteRoleAsync(ctx.Guild.Id, muteRole.Id);
                } else {
                    muteRole = await this.Database.GetMuteRoleAsync(ctx.Guild);
                    await this.InformAsync(ctx, $"Mute role for this guild: {Formatter.Bold(muteRole.Name)}");
                }
            }
            #endregion


            #region HELPER_FUNCTIONS
            private async Task PrintGuildConfigAsync(DiscordGuild guild, DiscordChannel channel, bool changed = false)
            {
                var emb = new DiscordEmbedBuilder() {
                    Title = $"Guild configuration {(changed ? " changed" : "")}",
                    Description = guild.ToString(),
                    Color = this.ModuleColor
                };

                CachedGuildConfig gcfg = this.Shared.GetGuildConfig(guild.Id);

                emb.AddField("Prefix", this.Shared.GetGuildPrefix(guild.Id), inline: true);
                emb.AddField("Silent replies active", gcfg.ReactionResponse.ToString(), inline: true);
                emb.AddField("Command suggestions active", gcfg.SuggestionsEnabled.ToString(), inline: true);
                emb.AddField("Action logging enabled", gcfg.LoggingEnabled.ToString(), inline: true);

                DiscordChannel wchn = await this.Database.GetWelcomeChannelAsync(guild);
                emb.AddField("Welcome messages", wchn != null ? $"on @ {wchn.Mention}" : "off", inline: true);

                DiscordChannel lchn = await this.Database.GetLeaveChannelAsync(guild);
                emb.AddField("Leave messages", lchn != null ? $"on @ {lchn.Mention}" : "off", inline: true);

                if (gcfg.RatelimitEnabled)
                    emb.AddField("Ratelimit watch", $"Sensitivity: {gcfg.RatelimitSensitivity} msgs per 5s\nAction: {gcfg.RatelimitAction.ToTypeString()}", inline: true);
                else
                    emb.AddField("Ratelimit watch", "off", inline: true);

                AntifloodSettings antifloodSettings = await this.Database.GetAntifloodSettingsAsync(guild.Id);
                if (antifloodSettings.Enabled) 
                    emb.AddField("Antiflood watch", $"Sensitivity: {antifloodSettings.Sensitivity} users per {antifloodSettings.Cooldown}s\nAction: {antifloodSettings.Action.ToTypeString()}", inline: true);
                else 
                    emb.AddField("Antiflood watch", "off", inline: true);

                AntiInstantLeaveSettings antiILSettings = await this.Database.GetAntiInstantLeaveSettingsAsync(guild.Id);
                if (antiILSettings.Enabled)
                    emb.AddField("Instant leave watch", $"Sensitivity: {antiILSettings.Sensitivity}", inline: true);
                else 
                    emb.AddField("Instant leave watch", "off", inline: true);
                

                DiscordRole muteRole = await this.Database.GetMuteRoleAsync(guild);
                if (muteRole == null)
                    muteRole = guild.Roles.FirstOrDefault(r => r.Name.ToLowerInvariant() == "gf_mute");
                if (muteRole != null)
                    emb.AddField("Mute role", muteRole.Name, inline: true);

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

                await channel.SendMessageAsync(embed: emb.Build());
            }
            
            private async Task ApplySettingsAsync(ulong gid, CachedGuildConfig gcfg, DiscordRole muteRole, 
                                                  JoinLeaveSettings ninfo, AntifloodSettings antifloodSettings, 
                                                  AntiInstantLeaveSettings antiInstantLeaveSettings)
            {
                this.Shared.GuildConfigurations[gid] = gcfg;

                await this.Database.UpdateGuildSettingsAsync(gid, gcfg);
                await this.Database.SetMuteRoleAsync(gid, muteRole.Id);
                if (ninfo.WelcomeChannelId != 0)
                    await this.Database.SetWelcomeChannelAsync(gid, ninfo.WelcomeChannelId);
                else
                    await this.Database.RemoveWelcomeChannelAsync(gid);
                if (!string.IsNullOrWhiteSpace(ninfo.WelcomeMessage))
                    await this.Database.SetWelcomeMessageAsync(gid, ninfo.WelcomeMessage);
                if (ninfo.LeaveChannelId != 0)
                    await this.Database.SetLeaveChannelAsync(gid, ninfo.LeaveChannelId);
                else
                    await this.Database.RemoveLeaveChannelAsync(gid);
                if (!string.IsNullOrWhiteSpace(ninfo.LeaveMessage))
                    await this.Database.SetLeaveMessageAsync(gid, ninfo.LeaveMessage);
                await this.Database.SetAntifloodSettingsAsync(gid, antifloodSettings);
                await this.Database.SetAntifloodSettingsAsync(gid, antifloodSettings);
            }

            private async Task<DiscordChannel> ChooseSetupChannelAsync(CommandContext ctx)
            {
                DiscordChannel channel = ctx.Guild.Channels.FirstOrDefault(c => c.Name == "gf_setup" && c.Type == ChannelType.Text);

                if (channel == null) {
                    if (await ctx.WaitForBoolReplyAsync($"Before we start, if you want to move this to somewhere else, would you like me to create a temporary public blank channel for the setup? Please reply with yes if you wish for me to create the channel or with no if you want us to continue here. Alternatively, if you do not want that channel to be public, let this command to timeout and create the channel yourself with name {Formatter.Bold("gf_setup")} and whatever permissions you like (just let me access it) and re-run the wizard.", reply: false)) {
                        try {
                            channel = await ctx.Guild.CreateChannelAsync("gf_setup", ChannelType.Text, reason: "TheGodfather setup channel creation.");
                            await channel.AddOverwriteAsync(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id), allow: Permissions.AccessChannels | Permissions.SendMessages, deny: Permissions.None);
                            await channel.AddOverwriteAsync(ctx.Guild.EveryoneRole, allow: Permissions.None, deny: Permissions.AccessChannels | Permissions.SendMessages);
                            await channel.AddOverwriteAsync(ctx.Member, allow: Permissions.AccessChannels | Permissions.SendMessages, deny: Permissions.None);
                            await this.InformAsync(ctx, $"Alright, let's move the setup to {channel.Mention}");
                        } catch {
                            throw new CommandFailedException($"I have failed to create a setup channel. Could you kindly create the channel called {Formatter.Bold("gf_setup")} and then re-run the command or give me the permission to create channels? The wizard will now exit...");
                        }
                    } else {
                        channel = ctx.Channel;
                    }
                }

                return channel;
            }

            private async Task PreviewSettingsAsync(CachedGuildConfig gcfg, CommandContext ctx, DiscordChannel channel, 
                                                    DiscordRole muteRole, JoinLeaveSettings msgSettings,
                                                    AntifloodSettings antifloodSettings, AntiInstantLeaveSettings antiInstantLeaveSettings)
            {
                var sb = new StringBuilder("Selected settings:").AppendLine().AppendLine();
                sb.Append("Prefix: ").AppendLine(Formatter.Bold(gcfg.Prefix ?? this.Shared.BotConfiguration.DefaultPrefix));
                sb.Append("Currency: ").AppendLine(Formatter.Bold(gcfg.Currency ?? "default"));
                sb.Append("Command suggestions: ").AppendLine(Formatter.Bold((gcfg.SuggestionsEnabled ? "on" : "off")));
                sb.Append("Mute role: ").AppendLine(Formatter.Bold(muteRole.Name));

                sb.Append("Action logging: ");
                if (gcfg.LoggingEnabled)
                    sb.Append(Formatter.Bold("on")).Append(" in channel ").AppendLine(ctx.Guild.GetChannel(gcfg.LogChannelId).Mention);
                else
                    sb.AppendLine(Formatter.Bold("off"));

                sb.AppendLine().Append("Welcome messages: ");
                if (msgSettings.WelcomeChannelId != 0) {
                    sb.Append(Formatter.Bold("enabled")).Append(" in ").AppendLine(ctx.Guild.GetChannel(msgSettings.WelcomeChannelId).Mention);
                    sb.Append("Message: ").AppendLine(Formatter.BlockCode(msgSettings.WelcomeMessage ?? "default"));
                } else {
                    sb.AppendLine(Formatter.Bold("disabled"));
                }

                sb.AppendLine().Append("Leave messages: ");
                if (msgSettings.LeaveChannelId != 0) {
                    sb.Append(Formatter.Bold("enabled")).Append(" in ").AppendLine(ctx.Guild.GetChannel(msgSettings.LeaveChannelId).Mention);
                    sb.Append("Message: ").AppendLine(Formatter.BlockCode(msgSettings.LeaveMessage ?? "default"));
                } else {
                    sb.AppendLine(Formatter.Bold("disabled"));
                }

                sb.AppendLine().Append("Ratelimit watch: ");
                if (gcfg.RatelimitEnabled) {
                    sb.AppendLine(Formatter.Bold("enabled"));
                    sb.Append("- Sensitivity: ").Append(gcfg.RatelimitSensitivity).AppendLine(" msgs per 5s.");
                    sb.Append("- Action: ").AppendLine(gcfg.RatelimitAction.ToTypeString());
                } else {
                    sb.AppendLine(Formatter.Bold("disabled"));
                }

                sb.AppendLine().Append("Antiflood watch: ");
                if (antifloodSettings.Enabled) {
                    sb.AppendLine(Formatter.Bold("enabled"));
                    sb.Append("- Sensitivity: ").Append(antifloodSettings.Sensitivity).Append(" users per ").Append(antifloodSettings.Cooldown).AppendLine("s");
                    sb.Append("- Action: ").AppendLine(antifloodSettings.Action.ToTypeString());
                } else {
                    sb.AppendLine(Formatter.Bold("disabled"));
                }

                sb.AppendLine().Append("Instant leave watch: ");
                if (antiInstantLeaveSettings.Enabled) {
                    sb.AppendLine(Formatter.Bold("enabled"));
                    sb.Append("- Sensitivity: ").Append(antiInstantLeaveSettings.Sensitivity).AppendLine("s");
                } else {
                    sb.AppendLine(Formatter.Bold("disabled"));
                }

                sb.AppendLine().Append("Linkfilter ");
                if (gcfg.LinkfilterEnabled) {
                    sb.Append(Formatter.Bold("enabled")).AppendLine(" with module settings:");
                    sb.Append(" - Discord invites blocker: ").AppendLine(gcfg.BlockDiscordInvites ? "on" : "off");
                    sb.Append(" - DDoS/Booter websites blocker: ").AppendLine(gcfg.BlockBooterWebsites ? "on" : "off");
                    sb.Append(" - IP logging websites blocker: ").AppendLine(gcfg.BlockIpLoggingWebsites ? "on" : "off");
                    sb.Append(" - Disturbing websites blocker: ").AppendLine(gcfg.BlockDisturbingWebsites ? "on" : "off");
                    sb.Append(" - URL shorteners blocker: ").AppendLine(gcfg.BlockUrlShorteners ? "on" : "off");
                    sb.AppendLine();
                } else {
                    sb.AppendLine(Formatter.Bold("disabled"));
                }

                await channel.EmbedAsync(sb.ToString());
            }

            private async Task SetupVerboseRepliesAsync(CachedGuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
            {
                string query = "By default I am sending verbose messages whenever a command is executed, " +
                               "but I can also silently react. Should I continue with the verbose replies?";
                if (await channel.WaitForBoolResponseAsync(ctx, query, reply: false))
                    gcfg.ReactionResponse = false;
            }

            private async Task SetupPrefixAsync(CachedGuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
            {
                if (await channel.WaitForBoolResponseAsync(ctx, "Do you wish to change the prefix?", reply: false)) {
                    await channel.EmbedAsync("What will the new prefix be?");
                    MessageContext mctx = await channel.WaitForMessageAsync(ctx.User, m => true);
                    gcfg.Prefix = mctx?.Message.Content;
                }
            }

            private async Task SetupCommandSuggestionsAsync(CachedGuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
            {
                string query = "Do you wish to enable command suggestions for those nasty times when you " +
                               "just can't remember the command name?";
                gcfg.SuggestionsEnabled = await channel.WaitForBoolResponseAsync(ctx, query, reply: false);
            }

            private async Task SetupLoggingAsync(CachedGuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
            {
                string logQuery = "I can log the actions that happen in the guild (such as message deletion, " +
                                  "channel updates etc.), so you always know what is going on in the guild. " +
                                  "Do you wish to enable action logging?";
                string chnQuery = $"Alright, cool. In order for the action logs to work you will need to " +
                                  $"tell me where to send the log messages. Please reply with a channel " +
                                  $"mention, for example {Formatter.Bold("#logs")}";

                if (await channel.WaitForBoolResponseAsync(ctx, logQuery, reply: false)) {
                    await channel.EmbedAsync(chnQuery);
                    MessageContext mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                        m => m.ChannelId == channel.Id &&
                             m.Author.Id == ctx.User.Id &&
                             m.MentionedChannels.Count == 1
                    );
                    gcfg.LogChannelId = mctx.MentionedChannels.FirstOrDefault()?.Id ?? 0;
                }
            }

            private async Task<JoinLeaveSettings> SetupMemberUpdateMessagesAsync(CachedGuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
            {
                var ninfo = new JoinLeaveSettings();
                await GetChannelIdAndMessageAsync(ninfo, true);
                await GetChannelIdAndMessageAsync(ninfo, false);
                return ninfo;

                async Task GetChannelIdAndMessageAsync(JoinLeaveSettings info, bool welcome)
                {
                    InteractivityExtension interactivity = ctx.Client.GetInteractivity();
                    string query = $"I can also send a {(welcome ? "welcome" : "leave")} message when someone " +
                                   $"{(welcome ? "joins" : "leaves")} the guild. Do you wish to enable this feature?";

                    if (await channel.WaitForBoolResponseAsync(ctx, query, reply: false)) {
                        query = $"I will need a channel where to send the {(welcome ? "welcome" : "leave")}" +
                                " messages. Please reply with a channel mention, for example: " +
                                $"{Formatter.Bold(ctx.Guild.GetDefaultChannel()?.Mention ?? "#general")}";
                        await channel.EmbedAsync(query);
                        MessageContext mctx = await interactivity.WaitForMessageAsync(
                            m => m.ChannelId == channel.Id && 
                                 m.Author.Id == ctx.User.Id && 
                                 m.MentionedChannels.Count == 1
                        );

                        ulong cid = mctx?.MentionedChannels.FirstOrDefault()?.Id ?? 0;
                        if (info.WelcomeChannelId != 0 && ctx.Guild.GetChannel(info.WelcomeChannelId).Type != ChannelType.Text) {
                            await channel.InformFailureAsync("You need to provide a text channel!");
                            cid = 0;
                        }

                        string message = null;
                        query = $"You can also customize the {(welcome ? "welcome" : "leave")} message. " +
                                "Do you want to do that now?";
                        if (await channel.WaitForBoolResponseAsync(ctx, query, reply: false)) {
                            query = "Tell me what message you want me to send. Note that you can use the " +
                                    $"wildcard {Formatter.Bold("%user%")} and I will replace it with the " +
                                    "member mention.";
                            await channel.EmbedAsync(query);
                            mctx = await channel.WaitForMessageAsync(ctx.User, m => true);
                            info.WelcomeMessage = mctx?.Message?.Content;
                        }

                        if (welcome) {
                            info.WelcomeChannelId = cid;
                            info.WelcomeMessage = message;
                        } else {
                            info.LeaveChannelId = cid;
                            info.LeaveMessage = message;
                        }
                    }
                }
            }

            private async Task SetupLinkfilterAsync(CachedGuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
            {
                if (await channel.WaitForBoolResponseAsync(ctx, "Do you wish to enable link filtering?", reply: false)) {
                    gcfg.LinkfilterEnabled = true;
                    if (await channel.WaitForBoolResponseAsync(ctx, "Do you wish to enable Discord invite links filtering?", reply: false))
                        gcfg.BlockDiscordInvites = true;
                    else
                        gcfg.BlockDiscordInvites = false;
                    if (await channel.WaitForBoolResponseAsync(ctx, "Do you wish to enable DDoS/Booter websites filtering?", reply: false))
                        gcfg.BlockBooterWebsites = true;
                    else
                        gcfg.BlockBooterWebsites = false;
                    if (await channel.WaitForBoolResponseAsync(ctx, "Do you wish to enable IP logging websites filtering?", reply: false))
                        gcfg.BlockIpLoggingWebsites = true;
                    else
                        gcfg.BlockIpLoggingWebsites = false;
                    if (await channel.WaitForBoolResponseAsync(ctx, "Do you wish to enable disturbing/shock/gore websites filtering?", reply: false))
                        gcfg.BlockDisturbingWebsites = true;
                    else
                        gcfg.BlockDisturbingWebsites = false;
                    if (await channel.WaitForBoolResponseAsync(ctx, "Do you wish to enable URL shorteners filtering?", reply: false))
                        gcfg.BlockUrlShorteners = true;
                    else
                        gcfg.BlockUrlShorteners = false;
                }
            }

            private async Task<DiscordRole> SetupMuteRoleAsync(CachedGuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
            {
                DiscordRole muteRole = null;

                if (await channel.WaitForBoolResponseAsync(ctx, "Do you wish to manually set the mute role for this guild?", reply: false)) {
                    await channel.EmbedAsync("Which role will it be?");
                    MessageContext mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                        m => m.ChannelId == channel.Id && 
                             m.Author.Id == ctx.User.Id && 
                             m.MentionedRoles.Count == 1
                    );
                    if (mctx != null)
                        muteRole = mctx.MentionedRoles.First();
                }

                muteRole = muteRole ?? await ctx.Services.GetService<RatelimitService>().GetOrCreateMuteRoleAsync(ctx.Guild);
                return muteRole;
            }

            private async Task SetupRatelimitAsync(CachedGuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
            {
                string query = "Ratelimit watch is a feature that automatically punishes users that post " +
                               "more than specified amount of messages in a 5s timespan. Do you wish to " +
                               "enable ratelimit watch?";
                if (await channel.WaitForBoolResponseAsync(ctx, query, reply: false)) {
                    gcfg.RatelimitEnabled = true;

                    query = $"Do you wish to change the default ratelimit action ({gcfg.RatelimitAction.ToTypeString()})?";
                    if (await channel.WaitForBoolResponseAsync(ctx, query, reply: false)) {
                        await channel.EmbedAsync("Please specify the action. Possible values: Mute, TempMute, Kick, Ban, TempBan");
                        MessageContext mctx = await channel.WaitForMessageAsync(ctx.User,
                            m => CustomPunishmentActionTypeConverter.TryConvert(m).HasValue
                        );
                        if (mctx != null)
                            gcfg.RatelimitAction = CustomPunishmentActionTypeConverter.TryConvert(mctx.Message.Content).Value;
                    }

                    query = "Do you wish to change the default ratelimit sensitivity aka number of messages " +
                            $"in 5s window before the action is triggered ({gcfg.RatelimitSensitivity})?";
                    if (await channel.WaitForBoolResponseAsync(ctx, query, reply: false)) {
                        await channel.EmbedAsync("Please specify the sensitivity. Valid range: [4, 10]");
                        MessageContext mctx = await channel.WaitForMessageAsync(ctx.User,
                            m => short.TryParse(m, out short sens) && sens >= 4 && sens <= 10
                        );
                        if (mctx != null)
                            gcfg.RatelimitSensitivity = short.Parse(mctx.Message.Content);
                    }
                }
            }

            private async Task<AntifloodSettings> SetupAntifloodAsync(CachedGuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
            {
                var antifloodSettings = new AntifloodSettings();

                string query = "Antiflood watch is a feature that automatically punishes users that flood " +
                               "(raid) the guild. Do you wish to enable antiflood watch?";
                if (await channel.WaitForBoolResponseAsync(ctx, query, reply: false)) {
                    antifloodSettings.Enabled = true;

                    query = $"Do you wish to change the default antiflood action " +
                            $"({antifloodSettings.Action.ToTypeString()})?";
                    if (await channel.WaitForBoolResponseAsync(ctx, query, reply: false)) {
                        query = "Please specify the action. Possible values: Mute, TempMute, Kick, Ban, TempBan";
                        await channel.EmbedAsync(query);
                        MessageContext mctx = await channel.WaitForMessageAsync(ctx.User,
                            m => CustomPunishmentActionTypeConverter.TryConvert(m).HasValue
                        );
                        if (mctx != null)
                            antifloodSettings.Action = CustomPunishmentActionTypeConverter.TryConvert(mctx.Message.Content).Value;
                    }

                    query = $"Do you wish to change the default antiflood user quota after which the " +
                            $"action will be applied ({antifloodSettings.Sensitivity})?";
                    if (await channel.WaitForBoolResponseAsync(ctx, query, reply: false)) {
                        await channel.EmbedAsync("Please specify the sensitivity. Valid range: [2, 20]");
                        MessageContext mctx = await channel.WaitForMessageAsync(ctx.User,
                            m => short.TryParse(m, out short sens) && sens >= 2 && sens <= 20
                        );
                        if (mctx != null)
                            antifloodSettings.Sensitivity = short.Parse(mctx.Message.Content);
                    }

                    query = $"Do you wish to change the default cooldown time " +
                            $"({antifloodSettings.Cooldown})?";
                    if (await channel.WaitForBoolResponseAsync(ctx, query, reply: false)) {
                        await channel.EmbedAsync("Please specify the cooldown as a number of seconds. Valid range: [5, 60]");
                        MessageContext mctx = await channel.WaitForMessageAsync(ctx.User,
                            m => short.TryParse(m, out short cooldown) && cooldown >= 5 && cooldown <= 60
                        );
                        if (mctx != null)
                            antifloodSettings.Cooldown = short.Parse(mctx.Message.Content);
                    }
                }

                return antifloodSettings;
            }

            private async Task<AntiInstantLeaveSettings> SetupAntiInstantLeaveAsync(CachedGuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
            {
                var antiInstantLeaveSettings = new AntiInstantLeaveSettings();

                string query = "Instant leave watch is a feature that automatically punishes users that enter " +
                               "and instantly leave the guild (no idea why they do this, I assume ads). " +
                               "Do you wish to enable instant leave watch?";
                if (await channel.WaitForBoolResponseAsync(ctx, query, reply: false)) {
                    antiInstantLeaveSettings.Enabled = true;

                    query = $"Do you wish to change the default time window after which the new user will" +
                            $"not be punished ({antiInstantLeaveSettings.Sensitivity})?";
                    if (await channel.WaitForBoolResponseAsync(ctx, query, reply: false)) {
                        await channel.EmbedAsync("Please specify the time window in seconds. Valid range: [2, 20]");
                        MessageContext mctx = await channel.WaitForMessageAsync(ctx.User,
                            m => short.TryParse(m, out short sens) && sens >= 2 && sens <= 20
                        );
                        if (mctx != null)
                            antiInstantLeaveSettings.Sensitivity = short.Parse(mctx.Message.Content);
                    }
                }

                return antiInstantLeaveSettings;
            }

            private async Task SetupCurrencyAsync(CachedGuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
            {
                string query = "Do you wish to change the currency for this guild (default: credit)?";
                if (await channel.WaitForBoolResponseAsync(ctx, query, reply: false)) {
                    query = "Please specify the new currency (can also be an emoji). Note: Currency name " +
                            "cannot be longer than 30 characters.";
                    await channel.EmbedAsync(query);
                    MessageContext mctx = await channel.WaitForMessageAsync(ctx.User, m => m.Length < 30);
                    if (mctx != null)
                        gcfg.Currency = mctx.Message.Content;
                }
            }


            private class JoinLeaveSettings
            {
                public ulong WelcomeChannelId { get; set; }
                public ulong LeaveChannelId { get; set; }
                public string WelcomeMessage { get; set; }
                public string LeaveMessage { get; set; }
            }
            #endregion
        }
    }
}

