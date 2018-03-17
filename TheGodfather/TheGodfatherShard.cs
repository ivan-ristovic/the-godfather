#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Extensions.Converters;
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
    public sealed class TheGodfatherShard
    {
        #region STATIC_FIELDS
        public static bool Listening { get; set; } = true;
        #endregion

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

        public async Task DisconnectAndDispose()
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
                LogLevel = LogLevel.Info
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version <= new Version(6, 1, 7601, 65536))
                cfg.WebSocketClientFactory = WebSocket4NetClient.CreateNew;

            Client = new DiscordClient(cfg);

            Client.ClientErrored += Client_Error;
            Client.DebugLogger.LogMessageReceived += (s, e) => Logger.LogMessage(ShardId, e);
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
            await _db.AddGuildIfNotExistsAsync(e.Guild.Id)
                .ConfigureAwait(false);
        }

        private async Task Client_GuildMemberAdd(GuildMemberAddEventArgs e)
        {
            Log(LogLevel.Info,
                $"Member joined: {e.Member.ToString()}<br>" +
                $"{e.Guild.ToString()}"
            );

            ulong cid = 0;
            try {
                cid = await _db.GetGuildWelcomeChannelIdAsync(e.Guild.Id)
                    .ConfigureAwait(false);
            } catch (Exception exc) {
                Logger.LogException(LogLevel.Debug, exc);
            }

            if (cid == 0)
                return;

            try {
                var chn = e.Guild.GetChannel(cid);
                if (chn != null)
                    await chn.SendIconEmbedAsync($"Welcome to {Formatter.Bold(e.Guild.Name)}, {e.Member.Mention}!", DiscordEmoji.FromName(Client, ":wave:"))
                        .ConfigureAwait(false);
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
                    await _db.RemoveGuildWelcomeChannelAsync(e.Guild.Id)
                        .ConfigureAwait(false);
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
                cid = await _db.GetGuildLeaveChannelIdAsync(e.Guild.Id)
                    .ConfigureAwait(false);
            } catch (Exception exc) {
                Logger.LogException(LogLevel.Debug, exc);
            }

            if (cid == 0)
                return;

            try {
                var chn = e.Guild.GetChannel(cid);
                if (chn != null)
                    await chn.SendIconEmbedAsync($"{Formatter.Bold(e.Member?.Username ?? "<unknown>")} left the server. Cya never!", EmojiUtil.Wave)
                        .ConfigureAwait(false);
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
                    await _db.RemoveGuildLeaveChannelAsync(e.Guild.Id)
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
                    Log(LogLevel.Info,
                        $"Filter triggered:<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} ; {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    Log(LogLevel.Debug,
                        $"Filter triggered in message but missing permissions to delete!<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} ; {e.Channel.ToString()}"
                    );
                    if (e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                        await e.Channel.SendFailedEmbedAsync("The message contains the filtered word but I do not have permissions to delete it.")
                            .ConfigureAwait(false);
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
            if (_shared.GuildTextReactions.ContainsKey(e.Guild.Id)) {
                var tr = _shared.GuildTextReactions[e.Guild.Id].FirstOrDefault(r => r.TriggerRegex.IsMatch(e.Message.Content));
                if (tr != null) {
                    Log(LogLevel.Info,
                        $"Text reaction detected: {tr.TriggerRegex} : {tr.Response}<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} ; {e.Channel.ToString()}"
                    );
                    await e.Channel.SendMessageAsync(tr.Response.Replace("%user%", e.Author.Mention))
                        .ConfigureAwait(false);
                }
            }

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.AddReactions))
                return;

            // Check if message has an emoji reaction
            if (_shared.GuildEmojiReactions.ContainsKey(e.Guild.Id) && _shared.GuildEmojiReactions[e.Guild.Id] != null) {
                foreach (var reaction in _shared.GuildEmojiReactions[e.Guild.Id]) {
                    foreach (var trigger in reaction.Value) {
                        if (trigger.IsMatch(e.Message.Content)) {
                            Log(LogLevel.Info,
                                $"Emoji reaction detected: {reaction.Key}<br>" +
                                $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                                $"{e.Message.Author.ToString()}<br>" +
                                $"{e.Guild.ToString()} ; {e.Channel.ToString()}"
                            );
                            try {
                                var emoji = DiscordEmoji.FromName(Client, reaction.Key);
                                await e.Message.CreateReactionAsync(emoji)
                                    .ConfigureAwait(false);
                                break;
                            } catch (ArgumentException) {
                                await e.Channel.SendFailedEmbedAsync($"Emoji reaction set, but emoji: {reaction.Key} doesn't exist!")
                                    .ConfigureAwait(false);
                            } catch (UnauthorizedException) {
                                await e.Channel.SendFailedEmbedAsync("I have a reaction for that message set up but I do not have permissions to add reactions. Fix your shit pls.")
                                    .ConfigureAwait(false);
                            }
                        }
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
                        $"{e.Guild.ToString()} ; {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    Log(LogLevel.Debug,
                        $"Filter triggered in edited message but missing permissions to delete!<br>" +
                        $"Message: '{e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} ; {e.Channel.ToString()}"
                    );
                    await e.Channel.SendFailedEmbedAsync("The edited message contains the filtered word but I do not have permissions to delete it.")
                        .ConfigureAwait(false);
                }
                await e.Channel.SendFailedEmbedAsync($"Nice try, {e.Author.Mention}! But I see throught it!")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_EVENTS
        private async Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            await Task.Yield();

            Log(LogLevel.Info,
                $"Executed: {e.Command?.QualifiedName ?? "<unknown command>"}<br>" +
                $"{e.Context.User.ToString()}<br>" +
                $"{e.Context.Guild.ToString()}; {e.Context.Channel.ToString()}"
            );
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            if (!Listening || e.Exception == null)
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

            if (e.Exception is CommandNotFoundException)
                return;
            else if (e.Exception is InvalidCommandUsageException)
                emb.Description = $"{emoji} Invalid usage! {ex.Message}";
            else if (e.Exception is ArgumentException)
                emb.Description = $"{emoji} Argument specified is invalid (please see {Formatter.Bold("!help <command>")} and make sure the arguments are valid).";
            else if (e.Exception is CommandFailedException)
                emb.Description = $"{emoji} {ex.Message} {(ex.InnerException != null ? "Details: " + ex.InnerException.Message : "")}";
            else if (e.Exception is DatabaseServiceException)
                emb.Description = $"{emoji} {ex.Message} Details: {ex.InnerException?.Message ?? "<none>"}";
            else if (e.Exception is NotSupportedException)
                emb.Description = $"{emoji} Not supported. {ex.Message}";
            else if (e.Exception is InvalidOperationException)
                emb.Description = $"{emoji} Invalid operation. {ex.Message}";
            else if (e.Exception is NotFoundException)
                emb.Description = $"{emoji} 404: Not found.";
            else if (e.Exception is BadRequestException)
                emb.Description = $"{emoji} Bad request. Please check if the parameters are valid.";
            else if (e.Exception is Npgsql.NpgsqlException)
                emb.Description = $"{emoji} This is what happens when I use a Serbian database... Please {Formatter.InlineCode("!report")}.";
            else if (ex is ChecksFailedException exc) {
                var attr = exc.FailedChecks.First();
                if (attr is CooldownAttribute)
                    return;
                else if (attr is RequireUserPermissionsAttribute uperms)
                    emb.Description = $"{emoji} You do not have the required permissions ({uperms.Permissions.ToPermissionString()}) to run this command!";
                else if (attr is RequirePermissionsAttribute perms)
                    emb.Description = $"{emoji} Permissions to execute that command ({perms.Permissions.ToPermissionString()}) aren't met!";
                else if (attr is RequireBotPermissionsAttribute bperms)
                    emb.Description = $"{emoji} I do not have the required permissions ({bperms.Permissions.ToPermissionString()}) to run this command!";
                else if (attr is RequireOwnerAttribute)
                    emb.Description = $"{emoji} That command is reserved for the bot owner only!";
                else
                    emb.Description = $"{emoji} Command execution checks failed!";
            } else if (e.Exception is UnauthorizedException)
                emb.Description = $"{emoji} I am not authorized to do that.";
            else
                emb.Description = $"{emoji} Unknown error occured (probably because a Serbian made this bot). Please {Formatter.InlineCode("!report")}.";

            await e.Context.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion
    }
}