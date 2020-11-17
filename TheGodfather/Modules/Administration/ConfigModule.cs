using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Common.Converters;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration
{
    [Group("config")]
    [Aliases("configuration", "configure", "settings", "cfg")]
    [RequireGuild, RequireUserPermissions(Permissions.ManageGuild)]
    public partial class ConfigModule : TheGodfatherServiceModule<GuildConfigService>
    {
        public ConfigModule(GuildConfigService service)
            : base(service) { }


        #region config
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            await ctx.RespondAsync(embed: gcfg.ToDiscordEmbed(ctx.Guild, ctx.Services.GetRequiredService<LocalizationService>()));
        }
        #endregion

        #region config setup
        [Command("setup"), UsesInteractivity]
        [Aliases("wizard")]
        public async Task SetupAsync(CommandContext ctx,
                                    [Description("desc-setup-chn")] DiscordChannel? channel = null)
        {
            channel ??= ctx.Channel;

            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();

            var gcfg = new GuildConfig();
            await channel.LocalizedEmbedAsync(lcs, "str-setup");
            await Task.Delay(TimeSpan.FromSeconds(10));

            await SetupPrefixAsync(gcfg, ctx, channel);
            await SetupLoggingAsync(gcfg, ctx, channel);

            gcfg.ReactionResponse = await ctx.WaitForBoolReplyAsync("q-setup-verbose", channel, false);
            gcfg.SuggestionsEnabled = await ctx.WaitForBoolReplyAsync("q-setup-suggestions", channel, false);

            await SetupMemberUpdateMessagesAsync(gcfg, ctx, channel);
            await SetupMuteRoleAsync(gcfg, ctx, channel);
            await SetupLinkfilterAsync(gcfg, ctx, channel);
            await SetupAntispamAsync(gcfg, ctx, channel);
            await SetupRatelimitAsync(gcfg, ctx, channel);
            await SetupAntifloodAsync(gcfg, ctx, channel);
            await SetupAntiInstantLeaveAsync(gcfg, ctx, channel);
            await SetupCurrencyAsync(gcfg, ctx, channel);

            await channel.SendMessageAsync(embed: gcfg.ToDiscordEmbed(ctx.Guild, lcs));

            if (await ctx.WaitForBoolReplyAsync("q-setup-review", channel: channel)) {
                await this.ApplySettingsAsync(ctx, gcfg);
                await channel.EmbedAsync(lcs.GetString(ctx.Guild.Id, "str-done"), Emojis.CheckMarkSuccess);
            } else {
                await channel.InformFailureAsync(lcs.GetString(ctx.Guild.Id, "str-aborting"));
            }
        }
        #endregion

        #region COMMAND_CONFIG_VERBOSE
        [Command("verbose"), Priority(1)]
        [Description("Configuration of bot's responding options.")]
        [Aliases("fullresponse", "verbosereact", "verboseresponse", "v", "vr")]

        public async Task SilentResponseAsync(CommandContext ctx,
                                             [Description("Enable silent response?")] bool enable)
        {
            GuildConfig gcfg = await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.ReactionResponse = !enable;
            });

            DiscordChannel logchn = this.Service.GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
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
            CachedGuildConfig gcfg = this.Service.GetCachedConfig(ctx.Guild.Id);
            return this.InformAsync(ctx, $"Verbose responses for this guild are {Formatter.Bold(gcfg.ReactionResponse ? "disabled" : "enabled")}!");
        }
        #endregion

        #region COMMAND_CONFIG_SUGGESTIONS
        [Command("suggestions"), Priority(1)]
        [Description("Command suggestions configuration.")]
        [Aliases("suggestion", "cmdsug", "sugg", "sug", "cs", "s")]

        public async Task SuggestionsAsync(CommandContext ctx,
                                          [Description("Enable suggestions?")] bool enable)
        {
            GuildConfig gcfg = await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.SuggestionsEnabled = enable;
            });

            DiscordChannel logchn = this.Service.GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
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
        public async Task SuggestionsAsync(CommandContext ctx)
        {
            GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            await this.InformAsync(ctx, $"Command suggestions for this guild are {Formatter.Bold(gcfg.SuggestionsEnabled ? "enabled" : "disabled")}!");
        }
        #endregion

        #region COMMAND_CONFIG_WELCOME
        [Command("welcome"), Priority(3)]
        [Description("Allows user welcoming configuration.")]
        [Aliases("enter", "join", "wlc", "wm", "w")]

        public async Task WelcomeAsync(CommandContext ctx)
        {
            GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            DiscordChannel wchn = ctx.Guild.GetChannel(gcfg.WelcomeChannelId);
            await this.InformAsync(ctx, $"Member welcome messages for this guild are: {Formatter.Bold(wchn is null ? "disabled" : $"enabled @ {wchn.Mention}")}!");
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

            if (!string.IsNullOrWhiteSpace(message) && (message.Length < 3 || message.Length > 120))
                throw new CommandFailedException("Message cannot be shorter than 3 or longer than 120 characters!");

            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.WelcomeChannelIdDb = enable ? (long)wchn.Id : (long?)null;
                if (!string.IsNullOrWhiteSpace(message))
                    cfg.WelcomeMessage = message;
            });

            DiscordChannel logchn = this.Service.GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = "Guild config changed",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("User welcoming", enable ? $"on @ {wchn.Mention}" : "off", inline: true);
                emb.AddField("Welcome message", message ?? Formatter.Italic("not changed"));
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            if (enable)
                await this.InformAsync(ctx, $"Welcome message channel set to {wchn.Mention} with message: {Formatter.Bold(string.IsNullOrWhiteSpace(message) ? "<previously set>" : message)}.", important: false);
            else
                await this.InformAsync(ctx, $"Welcome messages are now disabled.", important: false);
        }

        [Command("welcome"), Priority(1)]
        public Task WelcomeAsync(CommandContext ctx,
                                [Description("Channel.")] DiscordChannel channel,
                                [RemainingText, Description("Welcome message.")] string message = null)
            => this.WelcomeAsync(ctx, true, channel, message);

        [Command("welcome"), Priority(0)]
        public Task WelcomeAsync(CommandContext ctx,
                                [RemainingText, Description("Welcome message.")] string message)
            => this.WelcomeAsync(ctx, true, ctx.Channel, message);

        #endregion

        #region COMMAND_CONFIG_LEAVE
        [Command("leave"), Priority(3)]
        [Description("Allows user leaving message configuration.")]
        [Aliases("exit", "drop", "lvm", "lm", "l")]

        public async Task LeaveAsync(CommandContext ctx)
        {
            GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            DiscordChannel lchn = ctx.Guild.GetChannel(gcfg.LeaveChannelId);
            await this.InformAsync(ctx, $"Member leave messages for this guild are: {Formatter.Bold(lchn is null ? "disabled" : $"enabled @ {lchn.Mention}")}!");
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

            if (!string.IsNullOrWhiteSpace(message) && (message.Length < 3 || message.Length > 120))
                throw new CommandFailedException("Message cannot be shorter than 3 or longer than 120 characters!");

            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.LeaveChannelIdDb = enable ? (long)lchn.Id : (long?)null;
                if (!string.IsNullOrWhiteSpace(message))
                    cfg.LeaveMessage = message;
            });

            DiscordChannel logchn = this.Service.GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = "Guild config changed",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("User leave messages", enable ? $"on @ {lchn.Mention}" : "off", inline: true);
                emb.AddField("Leave message", message ?? Formatter.Italic("not changed"));
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            if (enable)
                await this.InformAsync(ctx, $"Welcome message channel set to {lchn.Mention} with message: {Formatter.Bold(string.IsNullOrWhiteSpace(message) ? "<previously set>" : message)}.", important: false);
            else
                await this.InformAsync(ctx, $"Welcome messages are now disabled.", important: false);
        }

        [Command("leave"), Priority(1)]
        public Task LeaveAsync(CommandContext ctx,
                              [Description("Channel.")] DiscordChannel channel,
                              [RemainingText, Description("Leave message.")] string message)
            => this.LeaveAsync(ctx, true, channel, message);

        [Command("leave"), Priority(0)]
        public Task LeaveAsync(CommandContext ctx,
                              [RemainingText, Description("Leave message.")] string message)
            => this.LeaveAsync(ctx, true, ctx.Channel, message);
        #endregion

        #region COMMAND_CONFIG_MUTEROLE
        [Command("setmuterole")]
        [Description("Gets or sets mute role for this guild.")]
        [Aliases("muterole", "mr", "muterl", "mrl")]

        public async Task GetOrSetMuteRoleAsync(CommandContext ctx,
                                               [Description("New mute role.")] DiscordRole muteRole = null)
        {
            DiscordRole mr = null;
            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => {
                if (muteRole is null)
                    mr = ctx.Guild.GetRole(cfg.MuteRoleId);
                else
                    cfg.MuteRoleId = muteRole.Id;
            });

            if (!(mr is null))
                await this.InformAsync(ctx, $"Mute role for this guild: {Formatter.Bold(muteRole.Name)}");
        }
        #endregion

        // TODO cfg reset command


        #region Helpers
        private static async Task SetupPrefixAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-prefix", channel: channel, reply: false)) {
                await channel.LocalizedEmbedAsync(ctx.Services.GetRequiredService<LocalizationService>(), "q-setup-prefix-new");
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.Content.Length <= 8);
                gcfg.Prefix = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : mctx.Result.Content;
            }
        }

        private static async Task SetupLoggingAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-log", channel: channel, reply: false)) {
                await channel.LocalizedEmbedAsync(ctx.Services.GetRequiredService<LocalizationService>(), "q-setup-log-chn");
                DiscordChannel? logchn = await ctx.Client.GetInteractivity().WaitForChannelMentionAsync(channel, ctx.User);
                gcfg.LogChannelId = logchn?.Id ?? 0;
            }
        }

        private static async Task SetupMemberUpdateMessagesAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            await GetChannelIdAndMessageAsync(welcome: true);
            await GetChannelIdAndMessageAsync(welcome: false);


            async Task GetChannelIdAndMessageAsync(bool welcome)
            {
                LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
                InteractivityExtension interactivity = ctx.Client.GetInteractivity();

                if (await ctx.WaitForBoolReplyAsync(welcome ? "q-setup-memupd-wm" : "q-setup-memupd-lm", channel: channel, reply: false)) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-memupd-chn");

                    DiscordChannel? chn = await interactivity.WaitForChannelMentionAsync(channel, ctx.User);
                    if (chn is { } && chn.Type == ChannelType.Text) {
                        if (welcome)
                            gcfg.WelcomeChannelId = chn?.Id ?? 0;
                        else
                            gcfg.LeaveChannelId = chn?.Id ?? 0;
                    }

                    if (await ctx.WaitForBoolReplyAsync("q-setup-memupd-msg", channel: channel, reply: false)) {
                        await channel.LocalizedEmbedAsync(lcs, "q-setup-memupd-msg-new");
                        InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.Content.Length <= 128);
                        if (mctx.TimedOut) {
                            throw new CommandFailedException(ctx, "str-timeout");
                        } else {
                            if (welcome)
                                gcfg.WelcomeMessage = mctx.Result.Content;
                            else
                                gcfg.LeaveMessage = mctx.Result.Content;
                        }
                    }
                }
            }
        }

        private static async Task SetupMuteRoleAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            DiscordRole? muteRole = null;

            if (await ctx.WaitForBoolReplyAsync("q-setup-muterole", channel: channel, reply: false)) {
                await channel.LocalizedEmbedAsync(ctx.Services.GetRequiredService<LocalizationService>(), "q-setup-muterole-new");
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.MentionedRoles.Count == 1);
                muteRole = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : mctx.Result.MentionedRoles.FirstOrDefault();
            }

            try {
                muteRole ??= await ctx.Services.GetRequiredService<AntispamService>().GetOrCreateMuteRoleAsync(ctx.Guild);
            } catch (UnauthorizedException) {
                await channel.InformFailureAsync("cmd-err-role-403");
            }

            gcfg.MuteRoleId = muteRole?.Id ?? 0;
        }

        private static async Task SetupLinkfilterAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-lf", channel: channel, reply: false)) {
                gcfg.LinkfilterSettings.Enabled = true;
                gcfg.LinkfilterSettings.BlockDiscordInvites = await ctx.WaitForBoolReplyAsync("q-setup-lf-invites", channel: channel, reply: false);
                gcfg.LinkfilterSettings.BlockBooterWebsites = await ctx.WaitForBoolReplyAsync("q-setup-lf-ddos", channel: channel, reply: false);
                gcfg.LinkfilterSettings.BlockIpLoggingWebsites = await ctx.WaitForBoolReplyAsync("q-setup-lf-ip", channel: channel, reply: false);
                gcfg.LinkfilterSettings.BlockDisturbingWebsites = await ctx.WaitForBoolReplyAsync("q-setup-lf-gore", channel: channel, reply: false);
                gcfg.LinkfilterSettings.BlockUrlShorteners = await ctx.WaitForBoolReplyAsync("q-setup-lf-urlshort", channel: channel, reply: false);
            }
        }

        private static async Task SetupRatelimitAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            if (await ctx.WaitForBoolReplyAsync("q-setup-rl", channel: channel, reply: false)) {
                gcfg.RatelimitEnabled = true;

                if (await ctx.WaitForBoolReplyAsync("q-setup-rl-action", channel: channel, reply: false, args: gcfg.RatelimitAction.ToTypeString())) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-rl-action-new", args: Enum.GetNames<PunishmentAction>().Separate(", "));
                    PunishmentAction? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                    if (action is { })
                        gcfg.RatelimitAction = action.Value;
                }

                if (await ctx.WaitForBoolReplyAsync("q-setup-rl-sens", channel: channel, reply: false, args: gcfg.RatelimitSensitivity)) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-rl-sens-new");
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= 4 && sens <= 10
                    );
                    gcfg.RatelimitSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }
            }
        }

        private static async Task SetupAntispamAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            if (await ctx.WaitForBoolReplyAsync("q-setup-as", channel: channel, reply: false)) {
                gcfg.AntispamEnabled = true;

                if (await ctx.WaitForBoolReplyAsync("q-setup-as-action", channel: channel, reply: false, args: gcfg.AntispamAction.ToTypeString())) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-as-action-new", args: Enum.GetNames<PunishmentAction>().Separate(", "));
                    PunishmentAction? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                    if (action is { })
                        gcfg.AntispamAction = action.Value;
                }

                if (await ctx.WaitForBoolReplyAsync("q-setup-as-sens", channel: channel, reply: false, args: gcfg.AntispamSensitivity)) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-as-sens-new");
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= 3 && sens <= 10
                    );
                    gcfg.AntispamSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }
            }
        }

        private static async Task SetupAntifloodAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            if (await ctx.WaitForBoolReplyAsync("q-setup-af", channel: channel, reply: false)) {
                gcfg.AntifloodEnabled = true;

                if (await ctx.WaitForBoolReplyAsync("q-setup-af-action", channel: channel, reply: false, args: gcfg.AntifloodAction.ToTypeString())) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-af-action-new", args: Enum.GetNames<PunishmentAction>().Separate(", "));
                    PunishmentAction? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                    if (action is { })
                        gcfg.AntifloodAction = action.Value;
                }

                if (await ctx.WaitForBoolReplyAsync("q-setup-af-sens", channel: channel, reply: false, args: gcfg.AntifloodSensitivity)) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-af-sens-new");
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= 2 && sens <= 20
                    );
                    gcfg.AntifloodSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }

                if (await ctx.WaitForBoolReplyAsync("q-setup-af-cd", channel: channel, reply: false, args: gcfg.AntifloodCooldown)) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-af-cd-new");
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= 5 && sens <= 60
                    );
                    gcfg.AntifloodCooldown = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }
            }
        }

        private static async Task SetupAntiInstantLeaveAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            if (await ctx.WaitForBoolReplyAsync("q-setup-il", channel: channel, reply: false)) {
                gcfg.AntiInstantLeaveEnabled = true;

                if (await ctx.WaitForBoolReplyAsync("q-setup-il-cd", channel: channel, reply: false, args: gcfg.AntifloodCooldown)) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-il-cd-new");
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= 2 && sens <= 20
                    );
                    gcfg.AntiInstantLeaveCooldown = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }
            }
        }

        private static async Task SetupCurrencyAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-currency", channel: channel, reply: false)) {
                await channel.LocalizedEmbedAsync(ctx.Services.GetRequiredService<LocalizationService>(), "q-setup-currency-new");
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.Content.Length <= 30);
                gcfg.Currency = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : mctx.Result.Content;
            }
        }

        private async Task ApplySettingsAsync(CommandContext ctx, GuildConfig gcfg)
        {
            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.AntifloodSettings = gcfg.AntifloodSettings;
                cfg.AntiInstantLeaveSettings = gcfg.AntiInstantLeaveSettings;
                cfg.CachedConfig = gcfg.CachedConfig;
                cfg.LeaveChannelId = gcfg.LeaveChannelId;
                cfg.LeaveMessage = gcfg.LeaveMessage;
                cfg.MuteRoleId = gcfg.MuteRoleId;
                cfg.WelcomeChannelId = gcfg.WelcomeChannelId;
                cfg.WelcomeMessage = gcfg.WelcomeMessage;
            });

            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            LoggingService ls = ctx.Services.GetRequiredService<LoggingService>();
            await ls.LogAsync(ctx.Guild, gcfg.ToDiscordEmbed(ctx.Guild, lcs, update: true));
        }
        #endregion
    }
}

