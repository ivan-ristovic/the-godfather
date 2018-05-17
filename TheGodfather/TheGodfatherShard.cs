#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Common.Converters;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.VoiceNext;
#endregion

namespace TheGodfather
{
    internal sealed class TheGodfatherShard
    {
        #region PUBLIC_FIELDS
        public int ShardId { get; }
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public VoiceNextExtension Voice { get; private set; }
        public List<(string, Command)> CommandNames { get; private set; }
        #endregion

        #region PRIVATE_FIELDS
        private SharedData _shared { get; }
        private DBService _db { get; }
        #endregion


        public TheGodfatherShard(int sid, DBService db, SharedData shared)
        {
            ShardId = sid;
            _db = db;
            _shared = shared;
        }


        public void Initialize()
        {
            SetupClient();
            SetupCommands();
            SetupInteractivity();
            SetupVoice();
        }

        public async Task DisconnectAndDisposeAsync()
        {
            await Client.DisconnectAsync();
            Client.Dispose();
        }

        public async Task StartAsync()
            => await Client.ConnectAsync().ConfigureAwait(false);

        public void Log(LogLevel level, string message)
            => Client.DebugLogger.LogMessage(level, "TheGodfather", message, DateTime.Now);


        #region BOT_SETUP_FUNCTIONS
        private void SetupClient()
        {
            var cfg = new DiscordConfiguration {
                Token = _shared.BotConfiguration.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LargeThreshold = 250,
                ShardCount = _shared.BotConfiguration.ShardCount,
                ShardId = ShardId,
                UseInternalLogHandler = false,
                LogLevel = TheGodfather.LogHandle.LogLevel
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version <= new Version(6, 1, 7601, 65536))
                cfg.WebSocketClientFactory = WebSocket4NetClient.CreateNew;

            Client = new DiscordClient(cfg);

            Client.DebugLogger.LogMessageReceived += (s, e) => TheGodfather.LogHandle.LogMessage(ShardId, e);

            Client.ChannelCreated += Client_ChannelCreated;
            Client.ChannelDeleted += Client_ChannelDeleted;
            Client.ChannelPinsUpdated += Client_ChannelPinsUpdated;
            Client.ChannelUpdated += Client_ChannelUpdated;
            Client.ClientErrored += Client_Errored;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.GuildBanAdded += Client_GuildBanAdded;
            Client.GuildBanRemoved += Client_GuildBanRemoved;
            Client.GuildCreated += Client_GuildCreated;
            Client.GuildDeleted += Client_GuildDeleted;
            Client.GuildEmojisUpdated += Client_GuildEmojisUpdated;
            Client.GuildIntegrationsUpdated += Client_GuildIntegrationsUpdated;
            Client.GuildMemberAdded += Client_GuildMemberAdded;
            Client.GuildMemberRemoved += Client_GuildMemberRemoved;
            Client.GuildMemberUpdated += Client_GuildMemberUpdated;
            Client.GuildRoleCreated += Client_GuildRoleCreated;
            Client.GuildRoleDeleted += Client_GuildRoleDeleted;
            Client.GuildRoleUpdated += Client_GuildRoleUpdated;
            Client.GuildUnavailable += Client_GuildUnavailable;
            Client.GuildUpdated += Client_GuildUpdated;
            Client.GuildUnavailable += Client_GuildUnavailable;
            Client.MessagesBulkDeleted += Client_MessagesBulkDeleted;
            Client.MessageCreated += Client_MessageCreated;
            Client.MessageDeleted += Client_MessageDeleted;
            Client.MessageUpdated += Client_MessageUpdated;
            Client.VoiceServerUpdated += Client_VoiceServerUpdated;
            Client.WebhooksUpdated += Client_WebhooksUpdated;
        }

        private void SetupCommands()
        {
            Commands = Client.UseCommandsNext(new CommandsNextConfiguration {
                EnableDms = false,
                CaseSensitive = false,
                EnableMentionPrefix = true,
                PrefixResolver = PrefixResolverAsync,
                Services = new ServiceCollection()
                    .AddSingleton(new YoutubeService(_shared.BotConfiguration.YouTubeKey))
                    .AddSingleton(new GiphyService(_shared.BotConfiguration.GiphyKey))
                    .AddSingleton(new ImgurService(_shared.BotConfiguration.ImgurKey))
                    .AddSingleton(new OMDbService(_shared.BotConfiguration.OMDbKey))
                    .AddSingleton(new SteamService(_shared.BotConfiguration.SteamKey))
                    .AddSingleton(new WeatherService(_shared.BotConfiguration.WeatherKey))
                    .AddSingleton(this)
                    .AddSingleton(_db)
                    .AddSingleton(_shared)
                    .BuildServiceProvider(),
            });
            Commands.SetHelpFormatter<CustomHelpFormatter>();
            Commands.RegisterCommands(Assembly.GetExecutingAssembly());
            Commands.RegisterConverter(new CustomActivityTypeConverter());
            Commands.RegisterConverter(new CustomBoolConverter());
            Commands.RegisterConverter(new CustomTimeWindowConverter());
            Commands.RegisterConverter(new CustomUriConverter());

            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            CommandNames = Commands.RegisteredCommands
                .SelectMany(TheGodfatherBaseModule.CommandSelector)
                .Distinct()
                .Where(cmd => cmd.Parent == null)
                .SelectMany(cmd => cmd.Aliases.Select(alias => (alias, cmd)).Concat(new[] { (cmd.Name, cmd) }))
                .ToList();
        }

        private void SetupInteractivity()
        {
            Interactivity = Client.UseInteractivity(new InteractivityConfiguration() {
                PaginationBehavior = TimeoutBehaviour.DeleteReactions,
                PaginationTimeout = TimeSpan.FromSeconds(30),
                Timeout = TimeSpan.FromMinutes(1)
            });
        }

        private void SetupVoice()
        {
            Voice = Client.UseVoiceNext();
        }

        private Task<int> PrefixResolverAsync(DiscordMessage m)
        {
            string p = _shared.GetGuildPrefix(m.Channel.Guild.Id) ?? _shared.BotConfiguration.DefaultPrefix;
            return Task.FromResult(m.GetStringPrefixLength(p));
        }
        #endregion

        #region CLIENT_EVENTS
        private async Task Client_ChannelCreated(ChannelCreateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Channel created: {e.Channel.ToString()}")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_ChannelDeleted(ChannelDeleteEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Channel deleted: {e.Channel.ToString()}")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_ChannelPinsUpdated(ChannelPinsUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Channel.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Pins updated in channel: {e.Channel.ToString()} ({Formatter.InlineCode(e.LastPinTimestamp.ToUniversalTime().ToString())})")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_ChannelUpdated(ChannelUpdateEventArgs e)
        {
            if (e.ChannelBefore.Position != e.ChannelAfter.Position)
                return;

            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Channel updated: {e.ChannelAfter.ToString()}")
                    .ConfigureAwait(false);
            }
        }

        private Task Client_Errored(ClientErrorEventArgs e)
        {
            Log(LogLevel.Critical, $"Client errored: {e.Exception.GetType()}: {e.Exception.Message}");
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            Log(LogLevel.Info, $"Guild available: {e.Guild.ToString()}");
            return Task.CompletedTask;
        }

        private async Task Client_GuildBanAdded(GuildBanAddEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Ban added: {e.Member.ToString()}")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildBanRemoved(GuildBanRemoveEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Ban removed: {e.Member.ToString()}")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildCreated(GuildCreateEventArgs e)
        {
            Log(LogLevel.Info, $"Joined guild: {e.Guild.ToString()}");

            await _db.RegisterGuildAsync(e.Guild.Id)
                .ConfigureAwait(false);
            _shared.GuildConfigurations.TryAdd(e.Guild.Id, PartialGuildConfig.Default);

            var emoji = DiscordEmoji.FromName(e.Client, ":small_blue_diamond:");
            await e.Guild.GetDefaultChannel().SendIconEmbedAsync(
                $"{Formatter.Bold("Thank you for adding me!")}\n\n" +
                $"{emoji} The default prefix for commands is {Formatter.Bold(_shared.BotConfiguration.DefaultPrefix)}, but it can be changed using {Formatter.Bold("prefix")} command.\n" +
                $"{emoji} I advise you to run the configuration wizard for this guild in order to quickly configure functions like logging, notifications etc. The wizard can be invoked using {Formatter.Bold("guild config setup")} command.\n" +
                $"{emoji} You can use the {Formatter.Bold("help")} command as a guide, though it is recommended to read the documentation @ https://github.com/ivan-ristovic/the-godfather\n" +
                $"{emoji} If you have any questions or problems, feel free to use the {Formatter.Bold("report")} command in order send a message to the bot owner ({e.Client.CurrentApplication.Owner.Username}#{e.Client.CurrentApplication.Owner.Discriminator}). Alternatively, you can create an issue on GitHub or join WorldMafia discord server for quick support (https://discord.me/worldmafia).\n"
                , StaticDiscordEmoji.Wave
            ).ConfigureAwait(false);
        }

        private async Task Client_GuildDeleted(GuildDeleteEventArgs e)
        {
            Log(LogLevel.Info, $"Left guild: {e.Guild.ToString()}");

            await _db.UnregisterGuildAsync(e.Guild.Id)
                .ConfigureAwait(false);
            _shared.GuildConfigurations.TryRemove(e.Guild.Id, out _);
        }

        private async Task Client_GuildEmojisUpdated(GuildEmojisUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Guild emojis updated.\n\nemojis before: {e.EmojisBefore?.Count}, emojis after: {e.EmojisAfter?.Count}")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildIntegrationsUpdated(GuildIntegrationsUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Guild integrations updated.")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildMemberAdded(GuildMemberAddEventArgs e)
        {
            if (!TheGodfather.Listening)
                return;

            Log(LogLevel.Info,
                $"Member joined: {e.Member.ToString()}<br>" +
                $"{e.Guild.ToString()}"
            );

            try {
                var cid = await _db.GetWelcomeChannelIdAsync(e.Guild.Id)
                    .ConfigureAwait(false);
                if (cid != 0) {
                    try {
                        var chn = e.Guild.GetChannel(cid);
                        if (chn != null) {
                            var msg = await _db.GetWelcomeMessageAsync(e.Guild.Id)
                                .ConfigureAwait(false);
                            if (string.IsNullOrWhiteSpace(msg))
                                await chn.SendIconEmbedAsync($"Welcome to {Formatter.Bold(e.Guild.Name)}, {e.Member.Mention}!", DiscordEmoji.FromName(Client, ":wave:")).ConfigureAwait(false);
                            else
                                await chn.SendIconEmbedAsync(msg.Replace("%user%", e.Member.Mention), DiscordEmoji.FromName(Client, ":wave:")).ConfigureAwait(false);
                        }
                    } catch (Exception exc) {
                        while (exc is AggregateException)
                            exc = exc.InnerException;
                        Log(LogLevel.Debug,
                            $"Failed to send a welcome message!<br>" +
                            $"Channel ID: {cid}<br>" +
                            $"{e.Guild.ToString()}<br>" +
                            $"Exception: {exc.GetType()}<br>" +
                            $"Message: {exc.Message}"
                        );
                        if (exc is NotFoundException)
                            await _db.RemoveWelcomeChannelAsync(e.Guild.Id)
                                .ConfigureAwait(false);
                    }
                }
            } catch (Exception exc) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, exc);
            }

            try {
                var rids = await _db.GetAutomaticRolesForGuildAsync(e.Guild.Id)
                    .ConfigureAwait(false);
                foreach (var rid in rids) {
                    try {
                        var role = e.Guild.GetRole(rid);
                        if (role == null) {
                            await _db.RemoveAutomaticRoleAsync(e.Guild.Id, rid)
                                .ConfigureAwait(false);
                        } else {
                            await e.Member.GrantRoleAsync(role)
                                .ConfigureAwait(false);
                        }
                    } catch (Exception exc) {
                        Log(LogLevel.Debug,
                            $"Failed to assign an automatic role to a new member!<br>" +
                            $"{e.Guild.ToString()}<br>" +
                            $"Exception: {exc.GetType()}<br>" +
                            $"Message: {exc.Message}"
                        );
                    }
                }
            } catch (Exception exc) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, exc);
            }

            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Member joined: {e.Member.ToString()}\n\nRegistered at: {e.Member.CreationTimestamp}, email: {e.Member.Email}")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildMemberRemoved(GuildMemberRemoveEventArgs e)
        {
            if (!TheGodfather.Listening || e.Member.Id == e.Client.CurrentUser.Id)
                return;

            Log(LogLevel.Info,
                $"Member left: {e.Member.ToString()}<br>" +
                e.Guild.ToString()
            );

            ulong cid = 0;
            try {
                cid = await _db.GetLeaveChannelIdAsync(e.Guild.Id)
                    .ConfigureAwait(false);
            } catch (Exception exc) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, exc);
            }

            if (cid == 0)
                return;

            try {
                var chn = e.Guild.GetChannel(cid);
                if (chn != null) {
                    var msg = await _db.GetLeaveMessageAsync(e.Guild.Id)
                        .ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(msg))
                        await chn.SendIconEmbedAsync($"{Formatter.Bold(e.Member?.Username ?? "<unknown>")} left the server! Bye!", StaticDiscordEmoji.Wave).ConfigureAwait(false);
                    else
                        await chn.SendIconEmbedAsync(msg.Replace("%user%", e.Member.Mention), DiscordEmoji.FromName(Client, ":wave:")).ConfigureAwait(false);
                }
            } catch (Exception exc) {
                while (exc is AggregateException)
                    exc = exc.InnerException;
                Log(LogLevel.Debug,
                    $"Failed to send a leaving message!<br>" +
                    $"Channel ID: {cid}<br>" +
                    $"{e.Guild.ToString()}<br>" +
                    $"Exception: {exc.GetType()}<br>" +
                    $"Message: {exc.Message}"
                );
                if (exc is NotFoundException)
                    await _db.RemoveLeaveChannelAsync(e.Guild.Id)
                        .ConfigureAwait(false);
            }

            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Member left: {e.Member.ToString()}")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildMemberUpdated(GuildMemberUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Member updated: {e.Member?.ToString()}\n\n{e.NicknameBefore ?? "<unknown name>"} -> {e.NicknameAfter ?? "<unknown name>"}\nRoles before: {string.Join(", ", e.RolesBefore)}\nRoles after: {string.Join(", ", e.RolesAfter)}")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildRoleCreated(GuildRoleCreateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Role created: {e.Role?.ToString()}")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildRoleDeleted(GuildRoleDeleteEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Role deleted: {e.Role?.ToString()}")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildRoleUpdated(GuildRoleUpdateEventArgs e)
        {
            if (e.RoleBefore.Position != e.RoleAfter.Position)
                return;

            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Role updated: {e.RoleBefore?.ToString()}")
                    .ConfigureAwait(false);
            }
        }

        private Task Client_GuildUnavailable(GuildDeleteEventArgs e)
        {
            Log(LogLevel.Info, $"Guild unavailable: {e.Guild.ToString()}");
            return Task.CompletedTask;
        }

        private async Task Client_GuildUpdated(GuildUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Guild settings updated.")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_MessagesBulkDeleted(MessageBulkDeleteEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Channel.Guild.Id)
                   .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Bulk message deletion occured in channel {e.Channel.Mention} (a total of {e.Messages.Count} messages were deleted).")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || !TheGodfather.Listening)
                return;

            if (e.Channel.IsPrivate) {
                Log(LogLevel.Info, $"Ignored DM from {e.Author.ToString()}:<br>{e.Message}");
                return;
            }

            if (_shared.BlockedChannels.Contains(e.Channel.Id))
                return;

            // Check if message contains filter
            if (e.Message.Content != null && _shared.MessageContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Channel.DeleteMessageAsync(e.Message)
                        .ConfigureAwait(false);
                    Log(LogLevel.Info,
                        $"Filter triggered in message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    Log(LogLevel.Debug,
                        $"Filter triggered in message but missing permissions to delete!<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                }
                return;
            }

            // If the user is blocked, ignore
            if (_shared.BlockedUsers.Contains(e.Author.Id))
                return;

            // Since below actions require SendMessages permission, checking it now
            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                return;

            // Update message count for the user that sent the message
            int rank = _shared.UpdateMessageCount(e.Author.Id);
            if (rank != -1) {
                var ranks = _shared.Ranks;
                await e.Channel.SendIconEmbedAsync($"GG {e.Author.Mention}! You have advanced to level {rank} ({(rank < ranks.Count ? ranks[rank] : "Low")})!", DiscordEmoji.FromName(Client, ":military_medal:"))
                    .ConfigureAwait(false);
            }

            // Check if message has a text reaction
            if (_shared.TextReactions.ContainsKey(e.Guild.Id)) {
                var tr = _shared.TextReactions[e.Guild.Id]?.FirstOrDefault(r => r.Matches(e.Message.Content));
                if (tr != null && tr.CanSend()) {
                    Log(LogLevel.Debug,
                        $"Text reaction detected: {tr.Response}<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                    await e.Channel.SendMessageAsync(tr.Response.Replace("%user%", e.Author.Mention))
                        .ConfigureAwait(false);
                }
            }

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.AddReactions))
                return;

            // Check if message has an emoji reaction
            if (_shared.EmojiReactions.ContainsKey(e.Guild.Id)) {
                var ereactions = _shared.EmojiReactions[e.Guild.Id].Where(er => er.Matches(e.Message.Content));
                foreach (var er in ereactions) {
                    Log(LogLevel.Debug,
                        $"Emoji reaction detected: {er.Response}<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                    try {
                        var emoji = DiscordEmoji.FromName(Client, er.Response);
                        await e.Message.CreateReactionAsync(emoji)
                            .ConfigureAwait(false);
                    } catch (ArgumentException) {
                        await _db.RemoveAllEmojiReactionTriggersForReactionAsync(e.Guild.Id, er.Response)
                            .ConfigureAwait(false);
                    } catch (UnauthorizedException) {
                        Log(LogLevel.Debug,
                            $"Emoji reaction trigger found but missing permissions to add reactions!<br>" +
                            $"Message: '{e.Message.Content.Replace('\n', ' ')}<br>" +
                            $"{e.Message.Author.ToString()}<br>" +
                            $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                        );
                        break;
                    }
                }
            }
        }

        private async Task Client_MessageDeleted(MessageDeleteEventArgs e)
        {
            if (e.Channel.IsPrivate)
                return;

            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null && e.Message != null) {
                var timestamp = e.Message.CreationTimestamp != null ? e.Message.CreationTimestamp.ToUniversalTime().ToString() : "<unknown timestamp>";
                var details = $"in channel {e.Channel.Mention}: {Formatter.BlockCode(string.IsNullOrWhiteSpace(e.Message?.Content) ? "<empty content>" : e.Message.Content)}\nCreated at: {timestamp}, embeds: {e.Message.Embeds.Count}, reactions: {e.Message.Reactions.Count}, attachments: {e.Message.Attachments.Count}";
                if (e.Message.Content != null && _shared.MessageContainsFilter(e.Guild.Id, e.Message.Content))
                    await logchn.SendIconEmbedAsync($"{e.Message.Author?.ToString() ?? "<unknown author>"} triggered a filter {details}").ConfigureAwait(false);
                else
                    await logchn.SendIconEmbedAsync($"Message by {e.Message.Author?.ToString() ?? "<unknown author>"} got deleted {details}").ConfigureAwait(false);
            }
        }

        private async Task Client_MessageUpdated(MessageUpdateEventArgs e)
        {
            if (e.Author == null || e.Message == null || !TheGodfather.Listening || e.Channel.IsPrivate)
                return;

            if (_shared.BlockedChannels.Contains(e.Channel.Id))
                return;

            // Check if message contains filter
            if (!e.Author.IsBot && e.Message.Content != null && _shared.MessageContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Channel.DeleteMessageAsync(e.Message)
                        .ConfigureAwait(false);

                    Log(LogLevel.Info,
                        $"Filter triggered after message edit:<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    Log(LogLevel.Debug,
                        $"Filter triggered in edited message but missing permissions to delete!<br>" +
                        $"Message: '{e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                }
            }

            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            try {
                if (logchn != null && !e.Author.IsBot && e.Message.EditedTimestamp != null) {
                    var detailspre = $"{Formatter.BlockCode(string.IsNullOrWhiteSpace(e.MessageBefore?.Content) ? "<empty content>" : e.MessageBefore.Content)}\nCreated at: {(e.Message.CreationTimestamp != null ? e.Message.CreationTimestamp.ToUniversalTime().ToString() : "<unknown>")}, embeds: {e.MessageBefore.Embeds.Count}, reactions: {e.MessageBefore.Reactions.Count}, attachments: {e.MessageBefore.Attachments.Count}";
                    var detailsafter = $"{Formatter.BlockCode(string.IsNullOrWhiteSpace(e.Message?.Content) ? "<empty content>" : e.Message.Content)}\nEdited at: {(e.Message.EditedTimestamp != null ? e.Message.EditedTimestamp.ToUniversalTime().ToString() : "<unknown>")}, embeds: {e.Message.Embeds.Count}, reactions: {e.Message.Reactions.Count}, attachments: {e.Message.Attachments.Count}";
                    await logchn.SendIconEmbedAsync($"Message by {e.Author.ToString()} in channel {e.Channel.Mention} was updated:\n\nBefore update: {detailspre}\n\nAfter update: {detailsafter}")
                        .ConfigureAwait(false);
                }
            } catch {

            }
        }

        private async Task Client_VoiceServerUpdated(VoiceServerUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Voice server updated to endpoint: {Formatter.Bold(e.Endpoint)}!")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_WebhooksUpdated(WebhooksUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendIconEmbedAsync($"Webhooks updated for {e.Channel.ToString()}!")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_EVENTS
        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            Log(LogLevel.Info,
                $"Executed: {e.Command?.QualifiedName ?? "<unknown command>"}<br>" +
                $"{e.Context.User.ToString()}<br>" +
                $"{e.Context.Guild.ToString()}; {e.Context.Channel.ToString()}"
            );

            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            if (!TheGodfather.Listening || e.Exception == null)
                return;

            var ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException;

            if (ex is ChecksFailedException chke && chke.FailedChecks.Any(c => c is ListeningCheckAttribute))
                return;

            Log(LogLevel.Info,
                $"Tried executing: {e.Command?.QualifiedName ?? "<unknown command>"}<br>" +
                $"{e.Context.User.ToString()}<br>" +
                $"{e.Context.Guild.ToString()}; {e.Context.Channel.ToString()}<br>" +
                $"Exception: {ex.GetType()}<br>" +
                (ex.InnerException != null ? $"Inner exception: {ex.InnerException.GetType()}<br>" : "") +
                $"Message: {ex.Message.Replace("\n", "<br>") ?? "<no message>"}<br>"
            );

            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
            var emb = new DiscordEmbedBuilder {
                Color = DiscordColor.Red
            };

            if (_shared.GuildConfigurations[e.Context.Guild.Id].SuggestionsEnabled && ex is CommandNotFoundException cne) {
                emb.WithTitle($"Command {cne.CommandName} not found. Did you mean...");
                var ordered = CommandNames
                    .OrderBy(tup => cne.CommandName.LevenshteinDistance(tup.Item1))
                    .Take(3);
                foreach (var (alias, cmd) in ordered)
                    emb.AddField($"{alias} ({cmd.QualifiedName})", cmd.Description);
            } else if (ex is InvalidCommandUsageException)
                emb.Description = $"{emoji} Invalid usage! {ex.Message}";
            else if (ex is ArgumentException)
                emb.Description = $"{emoji} Invalid command call (please see {Formatter.Bold("!help <command>")} and make sure the argument types are correct).";
            else if (ex is CommandFailedException)
                emb.Description = $"{emoji} {ex.Message} {(ex.InnerException != null ? "Details: " + ex.InnerException.Message : "")}";
            else if (ex is DatabaseServiceException)
                emb.Description = $"{emoji} {ex.Message} Details: {ex.InnerException?.Message ?? "<none>"}";
            else if (ex is NotSupportedException)
                emb.Description = $"{emoji} Not supported. {ex.Message}";
            else if (ex is InvalidOperationException)
                emb.Description = $"{emoji} Invalid operation. {ex.Message}";
            else if (ex is NotFoundException)
                emb.Description = $"{emoji} 404: Not found.";
            else if (ex is BadRequestException)
                emb.Description = $"{emoji} Bad request. Please check if the parameters are valid.";
            else if (ex is Npgsql.NpgsqlException)
                emb.Description = $"{emoji} Serbian database failed to respond... Please {Formatter.InlineCode("report")} this.";
            else if (ex is ChecksFailedException exc) {
                var attr = exc.FailedChecks.First();
                if (attr is CooldownAttribute)
                    return;
                else if (attr is RequirePermissionsAttribute perms)
                    emb.Description = $"{emoji} Permissions to execute that command ({perms.Permissions.ToPermissionString()}) aren't met!";
                else if (attr is RequireUserPermissionsAttribute uperms)
                    emb.Description = $"{emoji} You do not have the required permissions ({uperms.Permissions.ToPermissionString()}) to run this command!";
                else if (attr is RequireBotPermissionsAttribute bperms)
                    emb.Description = $"{emoji} I do not have the required permissions ({bperms.Permissions.ToPermissionString()}) to run this command!";
                else if (attr is RequirePriviledgedUserAttribute)
                    emb.Description = $"{emoji} That command is reserved for the bot owner and priviledged users!";
                else if (attr is RequireOwnerAttribute)
                    emb.Description = $"{emoji} That command is reserved for the bot owner only!";
                else if (attr is RequireNsfwAttribute)
                    emb.Description = $"{emoji} That command is allowed in NSFW channels only!";
                else if (attr is RequirePrefixesAttribute pattr)
                    emb.Description = $"{emoji} That command is allowedto be invoked with the following prefixes: {string.Join(", ", pattr.Prefixes)}!";
            } else if (ex is UnauthorizedException)
                emb.Description = $"{emoji} I am not authorized to do that.";
            else
                return;

            await e.Context.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region HELPER_FUNCTIONS
        private async Task<DiscordChannel> GetLogChannelForGuild(ulong gid)
        {
            var gcfg = _shared.GetGuildConfig(gid);
            if (gcfg.LoggingEnabled) {
                try {
                    var channel = await Client.GetChannelAsync(gcfg.LogChannelId)
                        .ConfigureAwait(false);
                    return channel;
                } catch (Exception e) {
                    TheGodfather.LogHandle.LogException(LogLevel.Warning, e);
                    return null;
                }
            } else {
                return null;
            }
        }
        #endregion
    }
}