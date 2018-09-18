#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
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
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Guild configuration",
                    Description = ctx.Guild.ToString(),
                    Color = this.ModuleColor
                };

                CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);

                emb.AddField("Prefix", this.Shared.GetGuildPrefix(ctx.Guild.Id), inline: true);
                emb.AddField("Silent replies active", gcfg.ReactionResponse.ToString(), inline: true);
                emb.AddField("Command suggestions active", gcfg.SuggestionsEnabled.ToString(), inline: true);
                emb.AddField("Action logging enabled", gcfg.LoggingEnabled.ToString(), inline: true);

                DiscordChannel wchn = await this.Database.GetWelcomeChannelAsync(ctx.Guild);
                emb.AddField("Welcome messages", wchn != null ? $"on @ {wchn.Mention}" : "off", inline: true);

                DiscordChannel lchn = await this.Database.GetLeaveChannelAsync(ctx.Guild);
                emb.AddField("Leave messages", lchn != null ? $"on @ {lchn.Mention}" : "off", inline: true);

                if (gcfg.RatelimitEnabled)
                    emb.AddField("Ratelimit watch", $"Sensitivity: {gcfg.RatelimitSensitivity} msgs per 5s\nAction: {gcfg.RatelimitAction.ToTypeString()}", inline: true);
                else
                    emb.AddField("Ratelimit watch", "off", inline: true);

                if (gcfg.AntifloodEnabled)
                    emb.AddField("Antiflood watch", $"Sensitivity: {gcfg.AntifloodSensitivity} users per {gcfg.AntifloodCooldown}s\nAction: {gcfg.AntifloodAction.ToTypeString()}", inline: true);
                else
                    emb.AddField("Antiflood watch", "off", inline: true);

                DiscordRole muteRole = await this.Database.GetMuteRoleAsync(ctx.Guild);
                if (muteRole == null)
                    muteRole = ctx.Guild.Roles.FirstOrDefault(r => r.Name.ToLowerInvariant() == "gf_mute");
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

                await ctx.RespondAsync(embed: emb.Build());
            }


            #region COMMAND_CONFIG_WIZARD
            [Command("setup"), UsesInteractivity]
            [Description("Starts an interactive wizard for configuring the guild settings.")]
            [Aliases("wizard")]
            [UsageExamples("!guild cfg setup")]
            public async Task SetupAsync(CommandContext ctx)
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
                            await this.InformFailureAsync(ctx, $"I have failed to create a setup channel. Could you kindly create the channel called {Formatter.Bold("gf_setup")} and then re-run the command or give me the permission to create channels? The wizard will now exit...");
                            return;
                        }
                    } else {
                        channel = ctx.Channel;
                    }
                }

                var gcfg = CachedGuildConfig.Default;
                await channel.EmbedAsync("Welcome to the guild configuration wizard!\n\nI will guide you through the configuration. You can always re-run this setup or manually change the settings so do not worry if you don't do everything like you wanted.\n\nThat being said, let's start the fun! Note that the changes will apply after the wizard finishes.");
                await Task.Delay(TimeSpan.FromSeconds(10));

                if (await channel.WaitForBoolResponseAsync(ctx, "By default I am sending verbose messages whenever a command is executed. However, I can also silently react. Should I continue with the verbose replies?", reply: false))
                    gcfg.ReactionResponse = false;

                InteractivityExtension interactivity = ctx.Client.GetInteractivity();

                if (await channel.WaitForBoolResponseAsync(ctx, "Do you wish to change the prefix for the bot?", reply: false)) {
                    await channel.EmbedAsync("What will the new prefix be?");
                    MessageContext mctx = await channel.WaitForMessageAsync(ctx.User, m => true);
                    gcfg.Prefix = mctx?.Message.Content;
                }

                gcfg.SuggestionsEnabled = await channel.WaitForBoolResponseAsync(ctx, "Do you wish to enable command suggestions for those nasty times when you just can't remember the command name?", reply: false);

                if (await channel.WaitForBoolResponseAsync(ctx, "I can log the actions that happen in the guild (such as message deletion, channel updates etc.), so you always know what is going on in the guild. Do you wish to enable the action log?", reply: false)) {
                    await channel.EmbedAsync($"Alright, cool. In order for the action logs to work you will need to tell me where to send the log messages. Please reply with a channel mention, for example {Formatter.Bold("#logs")}");
                    MessageContext mctx = await interactivity.WaitForMessageAsync(
                        m => m.ChannelId == channel.Id && m.Author.Id == ctx.User.Id && m.MentionedChannels.Count == 1
                    );
                    gcfg.LogChannelId = mctx.MentionedChannels.FirstOrDefault()?.Id ?? 0;
                }

                ulong wcid = 0;
                string wmessage = null;
                if (await channel.WaitForBoolResponseAsync(ctx, "I can also send a welcome message when someone joins the guild. Do you wish to enable this feature?", reply: false)) {
                    await channel.EmbedAsync($"I will need a channel where to send the welcome messages. Please reply with a channel mention, for example {Formatter.Bold(ctx.Guild.GetDefaultChannel()?.Mention ?? "#general")}");
                    MessageContext mctx = await interactivity.WaitForMessageAsync(
                        m => m.ChannelId == channel.Id && m.Author.Id == ctx.User.Id && m.MentionedChannels.Count == 1
                    );

                    wcid = mctx?.MentionedChannels.FirstOrDefault()?.Id ?? 0;
                    if (wcid != 0 && ctx.Guild.GetChannel(wcid).Type != ChannelType.Text) {
                        await channel.InformFailureAsync("You need to provide a text channel!");
                        wcid = 0;
                    }

                    if (await channel.WaitForBoolResponseAsync(ctx, "You can also customize the welcome message. Do you want to do that now?", reply: false)) {
                        await channel.EmbedAsync($"Tell me what message you want me to send when someone joins the guild. Note that you can use the wildcard {Formatter.Bold("%user%")} and I will replace it with the mention for the member who joined.");
                        mctx = await channel.WaitForMessageAsync(ctx.User, m => true);
                        wmessage = mctx?.Message?.Content;
                    }
                }

                ulong lcid = 0;
                string lmessage = null;
                if (await channel.WaitForBoolResponseAsync(ctx, "The same applies for member leave messages. Do you wish to enable this feature?", reply: false)) {
                    await channel.EmbedAsync($"I will need a channel where to send the leave messages. Please reply with a channel mention, for example {Formatter.Bold(ctx.Guild.GetDefaultChannel()?.Mention ?? "#general")}");
                    MessageContext mctx = await interactivity.WaitForMessageAsync(
                        m => m.ChannelId == channel.Id && m.Author.Id == ctx.User.Id && m.MentionedChannels.Count == 1
                    );

                    lcid = mctx?.MentionedChannels.FirstOrDefault()?.Id ?? 0;
                    if (lcid != 0 && ctx.Guild.GetChannel(lcid).Type != ChannelType.Text) {
                        await channel.InformFailureAsync("You need to provide a text channel!");
                        lcid = 0;
                    }

                    if (await channel.WaitForBoolResponseAsync(ctx, "You can also customize the leave message. Do you want to do that now?", reply: false)) {
                        await channel.EmbedAsync($"Tell me what message you want me to send when someone leaves the guild. Note that you can use the wildcard {Formatter.Bold("%user%")} and I will replace it with the mention for the member who left.");
                        mctx = await channel.WaitForMessageAsync(ctx.User, m => true);
                        lmessage = mctx?.Message?.Content;
                    }
                }

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

                DiscordRole muteRole = null;
                if (await channel.WaitForBoolResponseAsync(ctx, "Do you wish to manually set the mute role for this guild?", reply: false)) {
                    await channel.EmbedAsync("Which role will it be?");
                    MessageContext mctx = await interactivity.WaitForMessageAsync(
                        m => m.ChannelId == channel.Id && m.Author.Id == ctx.User.Id && m.MentionedChannels.Count == 1
                    );
                    if (mctx != null)
                        muteRole = mctx.MentionedRoles.First();
                }

                muteRole = muteRole ?? await ctx.Services.GetService<RatelimitService>().GetOrCreateMuteRoleAsync(ctx.Guild);

                if (await channel.WaitForBoolResponseAsync(ctx, "Ratelimit watch is a feature that automatically punishes users that post more than specified amount of messages in a 5s timespan. Do you wish to enable ratelimit watch?", reply: false)) {
                    gcfg.RatelimitEnabled = true;

                    if (await channel.WaitForBoolResponseAsync(ctx, $"Do you wish to change the default ratelimit action ({gcfg.RatelimitAction.ToTypeString()})?", reply: false)) {
                        await channel.EmbedAsync("Please specify the action. Possible values: Mute, TempMute, Kick, Ban, TempBan");
                        MessageContext mctx = await channel.WaitForMessageAsync(ctx.User,
                            m => CustomPunishmentActionTypeConverter.TryConvert(m).HasValue
                        );
                        if (mctx != null)
                            gcfg.RatelimitAction = CustomPunishmentActionTypeConverter.TryConvert(mctx.Message.Content).Value;
                    }

                    if (await channel.WaitForBoolResponseAsync(ctx, $"Do you wish to change the default ratelimit sensitivity aka number of messages in 5s window before the action is triggered ({gcfg.RatelimitSensitivity})?", reply: false)) {
                        await channel.EmbedAsync("Please specify the sensitivity. Valid range: [4, 10]");
                        MessageContext mctx = await channel.WaitForMessageAsync(ctx.User,
                            m => short.TryParse(m, out short sens) && sens >= 4 && sens <= 10
                        );
                        if (mctx != null)
                            gcfg.RatelimitSensitivity = short.Parse(mctx.Message.Content);
                    }
                }

                if (await channel.WaitForBoolResponseAsync(ctx, "Antiflood watch is a feature that automatically punishes users that flood the guild. Do you wish to enable antiflood watch?", reply: false)) {
                    gcfg.AntifloodEnabled = true;

                    if (await channel.WaitForBoolResponseAsync(ctx, $"Do you wish to change the default antiflood action ({gcfg.RatelimitAction.ToTypeString()})?", reply: false)) {
                        await channel.EmbedAsync("Please specify the action. Possible values: Mute, TempMute, Kick, Ban, TempBan");
                        MessageContext mctx = await channel.WaitForMessageAsync(ctx.User,
                            m => CustomPunishmentActionTypeConverter.TryConvert(m).HasValue
                        );
                        if (mctx != null)
                            gcfg.AntifloodAction = CustomPunishmentActionTypeConverter.TryConvert(mctx.Message.Content).Value;
                    }

                    if (await channel.WaitForBoolResponseAsync(ctx, $"Do you wish to change the default antiflood user quota after which the action will be applied ({gcfg.AntifloodSensitivity})?", reply: false)) {
                        await channel.EmbedAsync("Please specify the sensitivity. Valid range: [2, 20]");
                        MessageContext mctx = await channel.WaitForMessageAsync(ctx.User,
                            m => short.TryParse(m, out short sens) && sens >= 2 && sens <= 20
                        );
                        if (mctx != null)
                            gcfg.AntifloodSensitivity = short.Parse(mctx.Message.Content);
                    }

                    if (await channel.WaitForBoolResponseAsync(ctx, $"Do you wish to change the default cooldown time ({gcfg.AntifloodSensitivity})?", reply: false)) {
                        await channel.EmbedAsync("Please specify the cooldown as a number of seconds. Valid range: [5, 60]");
                        MessageContext mctx = await channel.WaitForMessageAsync(ctx.User,
                            m => short.TryParse(m, out short cooldown) && cooldown >= 5 && cooldown <= 60
                        );
                        if (mctx != null)
                            gcfg.AntifloodCooldown = short.Parse(mctx.Message.Content);
                    }
                }

                if (await channel.WaitForBoolResponseAsync(ctx, "Do you wish to change the currency for this guild (default: credit)?", reply: false)) {
                    await channel.EmbedAsync("Please specify the new currency (can also be an emoji). Note: Currency name cannot be longer than 30 characters.");
                    MessageContext mctx = await channel.WaitForMessageAsync(ctx.User, m => m.Length < 30);
                    if (mctx != null)
                        gcfg.Currency = mctx.Message.Content;
                }

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
                if (wcid != 0) {
                    sb.Append(Formatter.Bold("enabled")).Append(" in ").AppendLine(ctx.Guild.GetChannel(wcid).Mention);
                    sb.Append("Message: ").AppendLine(Formatter.BlockCode(wmessage ?? "default"));
                } else {
                    sb.AppendLine(Formatter.Bold("disabled"));
                }

                sb.AppendLine().Append("Leave messages: ");
                if (lcid != 0) {
                    sb.Append(Formatter.Bold("enabled")).Append(" in ").AppendLine(ctx.Guild.GetChannel(lcid).Mention);
                    sb.Append("Message: ").AppendLine(Formatter.BlockCode(lmessage ?? "default"));
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
                if (gcfg.AntifloodEnabled) {
                    sb.AppendLine(Formatter.Bold("enabled"));
                    sb.Append("- Sensitivity: ").Append(gcfg.AntifloodSensitivity).Append(" users per ").Append(gcfg.AntifloodCooldown).AppendLine("s");
                    sb.Append("- Action: ").AppendLine(gcfg.RatelimitAction.ToTypeString());
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

                if (await channel.WaitForBoolResponseAsync(ctx, "We are almost done! Please review the settings above and say whether you want me to apply them.")) {
                    this.Shared.GuildConfigurations[ctx.Guild.Id] = gcfg;

                    await this.Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg);
                    await this.Database.SetMuteRoleAsync(ctx.Guild.Id, muteRole.Id);
                    if (wcid != 0)
                        await this.Database.SetWelcomeChannelAsync(ctx.Guild.Id, wcid);
                    else
                        await this.Database.RemoveWelcomeChannelAsync(ctx.Guild.Id);
                    if (!string.IsNullOrWhiteSpace(wmessage))
                        await this.Database.SetWelcomeMessageAsync(ctx.Guild.Id, wmessage);
                    if (lcid != 0)
                        await this.Database.SetLeaveChannelAsync(ctx.Guild.Id, lcid);
                    else
                        await this.Database.RemoveLeaveChannelAsync(ctx.Guild.Id);
                    if (!string.IsNullOrWhiteSpace(lmessage))
                        await this.Database.SetLeaveMessageAsync(ctx.Guild.Id, lmessage);

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Prefix", this.Shared.GetGuildPrefix(ctx.Guild.Id), inline: true);
                        emb.AddField("Command suggestions", gcfg.SuggestionsEnabled ? "on" : "off", inline: true);
                        emb.AddField("Action logging", gcfg.LoggingEnabled ? "on" : "off", inline: true);
                        emb.AddField("Welcome messages", wcid != 0 ? "on" : "off", inline: true);
                        emb.AddField("Leave messages", lcid != 0 ? "on" : "off", inline: true);
                        emb.AddField("Linkfilter", gcfg.LinkfilterEnabled ? "on" : "off", inline: true);
                        emb.AddField("Linkfilter - Block invites", gcfg.BlockDiscordInvites ? "on" : "off", inline: true);
                        emb.AddField("Linkfilter - Block booter websites", gcfg.BlockBooterWebsites ? "on" : "off", inline: true);
                        emb.AddField("Linkfilter - Block disturbing websites", gcfg.BlockDisturbingWebsites ? "on" : "off", inline: true);
                        emb.AddField("Linkfilter - Block IP loggers", gcfg.BlockIpLoggingWebsites ? "on" : "off", inline: true);
                        emb.AddField("Linkfilter - Block URL shorteners", gcfg.BlockUrlShorteners ? "on" : "off", inline: true);
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

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
        }
    }
}

