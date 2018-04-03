#region USING_DIRECTIVES
using System;
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
        #endregion

        #region PRIVATE_FIELDS
        private SharedData _shared { get; }
        private DBService _db { get; }
        #endregion


        public TheGodfatherShard(int sid, DBService db, SharedData sd)
        {
            ShardId = sid;
            _db = db;
            _shared = sd;
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

            Client.ClientErrored += Client_Error;
            Client.DebugLogger.LogMessageReceived += (s, e) => TheGodfather.LogHandle.LogMessage(ShardId, e);
            Client.GuildAvailable += Client_GuildAvailable;
            Client.GuildMemberAdded += Client_GuildMemberAdd;
            Client.GuildMemberRemoved += Client_GuildMemberRemove;
            Client.MessageCreated += Client_MessageCreated;
            Client.MessageUpdated += Client_MessageUpdated;
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

            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;
        }

        private void SetupInteractivity()
        {
            Interactivity = Client.UseInteractivity(new InteractivityConfiguration() {
                PaginationBehavior = TimeoutBehaviour.DeleteReactions,
                PaginationTimeout = TimeSpan.FromSeconds(30),
                Timeout = TimeSpan.FromSeconds(30)
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
        private Task Client_Error(ClientErrorEventArgs e)
        {
            Log(LogLevel.Critical, $"Client errored: {e.Exception.GetType()}: {e.Exception.Message}");
            return Task.CompletedTask;
        }

        private async Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            Log(LogLevel.Info, $"Guild available: {e.Guild.ToString()}");
            await _db.RegisterGuildAsync(e.Guild.Id)
                .ConfigureAwait(false);
        }

        private async Task Client_GuildMemberAdd(GuildMemberAddEventArgs e)
        {
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
        }

        private async Task Client_GuildMemberRemove(GuildMemberRemoveEventArgs e)
        {
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
        }

        private async Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot)
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
                    Log(LogLevel.Debug,
                        $"Filter triggered:<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    Log(LogLevel.Info,
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

        private async Task Client_MessageUpdated(MessageUpdateEventArgs e)
        {
            if (e.Author == null || e.Message == null)
                return;

            if (_shared.BlockedChannels.Contains(e.Channel.Id))
                return;

            // Check if message contains filter
            if (!e.Author.IsBot && e.Message.Content != null && _shared.MessageContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Channel.DeleteMessageAsync(e.Message)
                        .ConfigureAwait(false);
                    Log(LogLevel.Info,
                        $"Filter triggered in edit of a message:<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    Log(LogLevel.Info,
                        $"Filter triggered in edited message but missing permissions to delete!<br>" +
                        $"Message: '{e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                }
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

            if (ex is CommandNotFoundException)
                return;
            else if (ex is InvalidCommandUsageException)
                emb.Description = $"{emoji} Invalid usage! {ex.Message}";
            else if (ex is ArgumentException)
                emb.Description = $"{emoji} Argument specified is invalid (please see {Formatter.Bold("!help <command>")} and make sure the arguments are valid).";
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
                else if (attr is RequireOwnerAttribute)
                    emb.Description = $"{emoji} That command is reserved for the bot owner only!";
                else
                    emb.Description = $"{emoji} Command execution checks failed!";
            } else if (ex is UnauthorizedException)
                emb.Description = $"{emoji} I am not authorized to do that.";
            else
                return;

            await e.Context.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion
    }
}