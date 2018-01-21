#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Exceptions;
using TheGodfather.Helpers;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather
{
    public sealed class TheGodfather
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
        private BotConfig _cfg { get; set; }
        private SharedData _shared { get; }
        private DatabaseService _db { get; }
        #endregion


        public TheGodfather(BotConfig cfg, int sid, DatabaseService db, SharedData sd)
        {
            _cfg = cfg;
            ShardId = sid;
            _db = db;
            _shared = sd;
        }

        ~TheGodfather()
        {
            Client.DisconnectAsync().GetAwaiter().GetResult();
        }


        public void Initialize()
        {
            SetupClient();
            SetupCommands();
            SetupInteractivity();
            SetupVoice();
        }

        public async Task StartAsync()
        {
            await Client.ConnectAsync().ConfigureAwait(false);
        }


        #region BOT_SETUP_FUNCTIONS
        private void SetupClient()
        {
            var cfg = new DiscordConfiguration {
                Token = _cfg.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LargeThreshold = 250,
                ShardCount = _cfg.ShardCount,
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
            Client.MessageReactionAdded += Client_ReactToMessage;
            Client.MessageUpdated += Client_MessageUpdated;
        }

        private void SetupCommands()
        {
            Commands = Client.UseCommandsNext(new CommandsNextConfiguration {
                EnableDms = false,
                CaseSensitive = false,
                EnableMentionPrefix = true,
                PrefixResolver = async m => await CheckMessageForPrefix(m),
                Services = new ServiceCollection()
                    .AddSingleton(new YoutubeService(_cfg.YoutubeKey))
                    .AddSingleton(new GiphyService(_cfg.GiphyKey))
                    .AddSingleton(new ImgurService(_cfg.ImgurKey))
                    .AddSingleton(new SteamService(_cfg.SteamKey))
                    .AddSingleton(this)
                    .AddSingleton(_db)
                    .AddSingleton(_shared)
                    .BuildServiceProvider(),
            });
            Commands.SetHelpFormatter<HelpFormatter>();
            Commands.RegisterCommands(Assembly.GetExecutingAssembly());

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

        private Task<int> CheckMessageForPrefix(DiscordMessage m)
        {
            string p = _shared.GetGuildPrefix(m.Channel.Guild.Id) ?? _cfg.DefaultPrefix;
            return Task.FromResult(m.GetStringPrefixLength(p));
        }
        #endregion

        #region CLIENT_EVENTS
        private Task Client_Error(ClientErrorEventArgs e)
        {
            Log(LogLevel.Error, $"Client errored: {e.Exception.GetType()}: {e.Exception.Message}");
            return Task.CompletedTask;
        }

        private async Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            Log(LogLevel.Info, $"Guild available: {e.Guild.ToString()}");
            if (await _db.AddGuildIfNotExistsAsync(e.Guild.Id).ConfigureAwait(false))
                await e.Guild.GetDefaultChannel().SendMessageAsync($"Thank you for adding me! Type {Formatter.InlineCode("!help / !help <command>")} to view my command list or get help for a specific command.").ConfigureAwait(false);
        }

        private async Task Client_GuildMemberAdd(GuildMemberAddEventArgs e)
        {
            Log(LogLevel.Info,
                $"Member joined: {e.Member.ToString()}<br>" +
                $"{e.Guild.ToString()}"
            );

            ulong cid = await _db.GetGuildWelcomeChannelIdAsync(e.Guild.Id)
                .ConfigureAwait(false);
            if (cid == 0)
                return;

            try {
                var chn = e.Guild.GetChannel(cid);
                if (chn != null)
                    await chn.SendMessageAsync($"Welcome to {Formatter.Bold(e.Guild.Name)}, {e.Member.Mention}!").ConfigureAwait(false);
            } catch (Exception exc) {
                while (exc is AggregateException)
                    exc = exc.InnerException;
                Log(LogLevel.Error,
                    $"Failed to send a welcome message!<br>" + 
                    $"Channel ID: {cid}<br>" +
                    $"{e.Guild.ToString()}<br>" +
                    $"Exception: {exc.GetType()}<br>" +
                    $"Message: {exc.Message}"
                );
            }
        }

        private async Task Client_GuildMemberRemove(GuildMemberRemoveEventArgs e)
        {
            Log(LogLevel.Info,
                $"Member left: {e.Member.ToString()}<br>" +
                e.Guild.ToString()
            );

            ulong cid = await _db.GetGuildLeaveChannelIdAsync(e.Guild.Id)
                .ConfigureAwait(false);
            if (cid == 0)
                return;

            try {
                var chn = e.Guild.GetChannel(cid);
                if (chn != null)
                    await chn.SendMessageAsync($"{Formatter.Bold(e.Member?.Username ?? "<unknown>")} left the server. Bye!").ConfigureAwait(false);
            } catch (Exception exc) {
                while (exc is AggregateException)
                    exc = exc.InnerException;
                Log(LogLevel.Error,
                    $"Failed to send a leaving message!<br>" +
                    $"Channel ID: {cid}<br>" +
                    $"{e.Guild.ToString()}<br>" +
                    $"Exception: {exc.GetType()}<br>" + 
                    $"Message: {exc.Message}"
                );
            }
        }

        private async Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot)
                return;

            if (e.Channel.IsPrivate) {
                Log(LogLevel.Info, $"IGNORED DM FROM {e.Author.ToString()}:<br>{e.Message}");
                return;
            }

            // Check if message contains filter
            if (e.Message.Content != null && _shared.ContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Channel.DeleteMessageAsync(e.Message)
                        .ConfigureAwait(false);
                    Log(LogLevel.Info,
                        $"Filter triggered:<br>" + 
                        $"Message: {e.Message.Content}<br>" +
                        $"{e.Message.Author.ToString()}<br>" + 
                        $"{e.Guild.ToString()} ; {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    Log(LogLevel.Warning,
                        $"Filter triggered in message but missing permissions to delete!<br>" +
                        $"Message: {e.Message.Content}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} ; {e.Channel.ToString()}"
                    );
                    if (e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                        await e.Channel.SendMessageAsync("The message contains the filtered word but I do not have permissions to delete it.")
                            .ConfigureAwait(false);
                }
                return;
            }

            // Since below actions require SendMessages permission, checking it now
            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                return;

            // Update message count for the user that sent the message
            int rank = _shared.UpdateMessageCount(e.Author.Id);
            if (rank != -1) {
                var ranks = _shared.Ranks;
                await e.Channel.SendMessageAsync($"GG {e.Author.Mention}! You have advanced to level {rank} ({(rank < ranks.Count ? ranks[rank] : "Low")})!")
                    .ConfigureAwait(false);
            }

            // Check if message has a text reaction
            var response = _shared.GetResponseForTextReaction(e.Guild.Id, e.Message.Content);
            if (response != null) {
                Log(LogLevel.Info,
                    $"Text reaction detected:<br>" + 
                    $"Message: {e.Message.Content}<br>" +
                    $"{e.Message.Author.ToString()}<br>" +
                    $"{e.Guild.ToString()} ; {e.Channel.ToString()}"
                );
                await e.Channel.SendMessageAsync(response.Replace("%user%", e.Author.Mention))
                    .ConfigureAwait(false);
            }

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.AddReactions))
                return;

            // Check if message has an emoji reaction
            var emojilist = _shared.GetEmojisForEmojiReaction(Client, e.Guild.Id, e.Message.Content);
            if (emojilist.Count > 0) {
                Log(LogLevel.Info,
                    $"Emoji reaction detected:<br>" + 
                    $"Message: {e.Message.Content}<br>" +
                    $"{e.Message.Author.ToString()}<br>" + 
                    $"{e.Guild.ToString()} ; {e.Channel.ToString()}"
                );
                foreach (var emoji in emojilist) {
                    try {
                        await e.Message.CreateReactionAsync(emoji)
                            .ConfigureAwait(false);
                    } catch (ArgumentException) {
                        await e.Channel.SendMessageAsync($"I have a reaction for that message set up ({emoji}) but that emoji doesn't exits. Fix your shit pls.")
                            .ConfigureAwait(false);
                    }
                    await Task.Delay(500)
                        .ConfigureAwait(false);
                }
            }
        }

        private async Task Client_MessageUpdated(MessageUpdateEventArgs e)
        {
            if (e.Author == null || e.Message == null)
                return;

            // Check if message contains filter
            if (!e.Author.IsBot && e.Message.Content != null && _shared.ContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Channel.DeleteMessageAsync(e.Message)
                        .ConfigureAwait(false);
                    Log(LogLevel.Info,
                        $"Filter triggered in edit of a message:<br>" +
                        $"Message: {e.Message.Content}<br>" + 
                        $"{e.Message.Author.ToString()}<br>" + 
                        $"{e.Guild.ToString()} ; {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    Log(LogLevel.Warning,
                        $"Filter triggered in edited message but missing permissions to delete!<br>" +
                        $"Message: '{e.Message.Content}<br>" + 
                        $"{e.Message.Author.ToString()}<br>" + 
                        $"{e.Guild.ToString()} ; {e.Channel.ToString()}"
                    );
                    await e.Channel.SendMessageAsync("The edited message contains the filtered word but I do not have permissions to delete it.")
                        .ConfigureAwait(false);
                }
                await e.Channel.SendMessageAsync($"Nice try, {e.Author.Mention}! But I see throught it!")
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_ReactToMessage(MessageReactionAddEventArgs e)
        {
            if (new Random().Next(10) == 0)
                await e.Message.CreateReactionAsync(e.Emoji).ConfigureAwait(false);
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

            if (ex is ChecksFailedException chke && chke.FailedChecks.Any(c => c is PreExecutionCheck))
                return;

            Log(LogLevel.Error,
                $"Tried executing: {e.Command?.QualifiedName ?? "<unknown command>"}<br>" +
                $"{e.Context.User.ToString()}<br>" +
                $"{e.Context.Guild.ToString()}; {e.Context.Channel.ToString()}<br>" +
                $"Exception: {ex.GetType()}<br>" +
                (ex.InnerException != null ? $"Inner exception: {ex.InnerException.GetType()}<br>" : "") +
                $"Message: {ex.Message.Replace("\n", "<br>") ?? "<no message>"}<br>"
            );

            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
            var embed = new DiscordEmbedBuilder {
                Title = "Error",
                Color = DiscordColor.Red
            };

            if (e.Exception is CommandNotFoundException)
                embed.Description = $"{emoji} The specified command does not exist.";
            else if (e.Exception is InvalidCommandUsageException)
                embed.Description = $"{emoji} Invalid usage! {ex.Message}";
            else if (e.Exception is CommandFailedException)
                embed.Description = $"{emoji} {ex.Message}";
            else if (e.Exception is DatabaseServiceException)
                embed.Description = $"{emoji} {ex.Message} Details: {ex.InnerException?.Message ?? "<none>"}";
            else if (e.Exception is NotSupportedException)
                embed.Description = $"{emoji} Not supported. {ex.Message}";
            else if (e.Exception is InvalidOperationException)
                embed.Description = $"{emoji} Invalid operation. {ex.Message}";
            else if (e.Exception is NotFoundException)
                embed.Description = $"{emoji} 404: Not found.";
            else if (e.Exception is ArgumentException)
                embed.Description = $"{emoji} Argument specified is invalid (please use {Formatter.Bold("!help <command>")}).";
            else if (e.Exception is Npgsql.NpgsqlException)
                embed.Description = $"{emoji} This is what happens when I use a Serbian database... Please {Formatter.InlineCode("!report")}.";
            else if (ex is ChecksFailedException exc) {
                var attr = exc.FailedChecks.First();
                if (attr is CooldownAttribute)
                    return;
                else if (attr is RequireUserPermissionsAttribute)
                    embed.Description = $"{emoji} You do not have the required permissions to run this command!";
                else if (attr is RequirePermissionsAttribute)
                    embed.Description = $"{emoji} Permissions to execute that command aren't met!";
                else if (attr is RequireOwnerAttribute)
                    embed.Description = $"{emoji} That command is reserved for the bot owner only!";
                else
                    embed.Description = $"{emoji} Command execution checks failed!";
            } else if (e.Exception is UnauthorizedException)
                embed.Description = $"{emoji} I am not authorized to do that.";
            else
                embed.Description = $"{emoji} Unknown error occured (probably because a Serbian made this bot). Please {Formatter.InlineCode("!report")}.";

            await e.Context.RespondAsync(embed: embed.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region HELPER_FUNCTIONS
        public void Log(LogLevel level, string message)
        {
            Client.DebugLogger.LogMessage(level, "TheGodfather", message, DateTime.Now);
        }

        public async Task<DiscordDmChannel> CreateDmChannelAsync(ulong uid)
        {
            var firstResult = Client.Guilds.Values.SelectMany(e => e.Members).FirstOrDefault(e => e.Id == uid);
            if (firstResult != null)
                return await firstResult.CreateDmChannelAsync().ConfigureAwait(false);
            else
                return null;
        }
        #endregion
    }
}