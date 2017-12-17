#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using TheGodfather.Exceptions;
using TheGodfather.Helpers;
using TheGodfather.Helpers.DataManagers;

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

using Npgsql;
using NpgsqlTypes;

namespace TheGodfather
{
    public class TheGodfather
    {
        #region PUBLIC_FIELDS
        public bool Listening { get; private set; }
        private BotConfig Config { get; set; }
        #endregion

        #region PRIVATE_FIELDS
        private DiscordClient _client { get; set; }
        private CommandsNextModule _commands { get; set; }
        private InteractivityModule _interactivity { get; set; }
        private VoiceNextClient _voice { get; set; }

        private BotDependencyList _dependecies { get; set; }

        internal Logger LogHandle { get; private set; }
        #endregion


        public TheGodfather()
        {
            Listening = true;
        }

        ~TheGodfather()
        {
            if (_client != null && LogHandle != null) {
                LogHandle.Log(LogLevel.Info, "Shutting down...");
                SaveData();
                _client.DisconnectAsync();
                _client.Dispose();
                if (Directory.Exists("Temp"))
                    Directory.Delete("Temp");
            }
        }


        public async Task MainAsync(string[] args)
        {
            Config = BotConfig.Load();
            if (Config == null) {
                Console.WriteLine("Config file corrupted or missing.");
                Console.ReadKey();
                return;
            };

            SetupClient();
            SetupCommands();
            SetupInteractivity();
            SetupVoice();
            LoadData();

            try {
                await _client.ConnectAsync()
                    .ConfigureAwait(false);
            } catch (Exception ex) {
                LogHandle.Log(LogLevel.Error, "Exception occured during connection: " + Environment.NewLine +
                    $" Exception: {ex.GetType()}" + Environment.NewLine +
                    (ex.InnerException != null ? $" Inner exception: {ex.InnerException.GetType()}" + Environment.NewLine : "") +
                    $" Message: {ex.Message ?? "<no message>"}"
                );
                Console.ReadKey();
                return;
            }

            await Task.Delay(-1);
        }


        #region BOT_SETUP_FUNCTIONS
        private void SetupClient()
        {
            _client = new DiscordClient(new DiscordConfiguration {
                LogLevel = LogLevel.Info,
                LargeThreshold = 250,
                AutoReconnect = true,
                Token = Config.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true
            });

            LogHandle = new Logger(_client.DebugLogger);

            _client.ClientErrored += Client_Error;
            _client.DebugLogger.LogMessageReceived += Client_LogMessage;
            _client.GuildAvailable += Client_GuildAvailable;
            _client.GuildMemberAdded += Client_GuildMemberAdd;
            _client.GuildMemberRemoved += Client_GuildMemberRemove;
            _client.Heartbeated += Client_Heartbeated;
            _client.MessageCreated += Client_MessageCreated;
            _client.MessageReactionAdded += Client_ReactToMessage;
            _client.MessageUpdated += Client_MessageUpdated;
            _client.Ready += Client_Ready;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version <= new Version(6, 1, 7601, 65536))
                _client.SetWebSocketClient<WebSocket4NetClient>();
        }

        private void SetupCommands()
        {
            _dependecies = new BotDependencyList(_client, Config);
            _commands = _client.UseCommandsNext(new CommandsNextConfiguration {
                EnableDms = false,
                CaseSensitive = false,
                EnableMentionPrefix = true,
                CustomPrefixPredicate = async m => await CheckMessageForPrefix(m),
                Dependencies = _dependecies.GetDependencyCollectionBuilder()
                                           .AddInstance(this)
                                           .AddInstance(Config)
                                           .Build(),
            });
            _commands.SetHelpFormatter<HelpFormatter>();
            _commands.RegisterCommands(Assembly.GetExecutingAssembly());
            _commands.CommandExecuted += Commands_CommandExecuted;
            _commands.CommandErrored += Commands_CommandErrored;
        }

        private void SetupInteractivity()
        {
            _interactivity = _client.UseInteractivity(new InteractivityConfiguration() {
                PaginationBehaviour = TimeoutBehaviour.Delete,
                PaginationTimeout = TimeSpan.FromSeconds(30),
                Timeout = TimeSpan.FromSeconds(30)
            });
        }

        private void SetupVoice()
        {
            _voice = _client.UseVoiceNext();
        }

        private void LoadData()
        {
            try {
                _dependecies.LoadData(_client.DebugLogger);
            } catch (Exception e) {
                LogHandle.Log(LogLevel.Error,
                    $"Errors occured during data load: " + Environment.NewLine +
                    $" Exception: {e.GetType()}" + Environment.NewLine +
                    (e.InnerException != null ? $" Inner exception: {e.InnerException.GetType()}" + Environment.NewLine : "") +
                    $" Message: {e.Message}"
                );
                return;
            }

            LogHandle.Log(LogLevel.Debug, "Data loaded.");
        }

        private void SaveData()
        {
            try {
                _dependecies.SaveData(_client.DebugLogger);
            } catch (Exception e) {
                LogHandle.Log(LogLevel.Error,
                    $"Errors occured during data save: " + Environment.NewLine +
                    $" Exception: {e.GetType()}" + Environment.NewLine +
                    (e.InnerException != null ? $" Inner exception: {e.InnerException.GetType()}" + Environment.NewLine : "") +
                    $" Message: {e.Message}"
                );
                return;
            }

            LogHandle.Log(LogLevel.Debug, "Data saved.");
        }

        private Task<int> CheckMessageForPrefix(DiscordMessage m)
        {
            string p = _dependecies.GuildConfigControl.GetGuildPrefix(m.Channel.Guild.Id);
            return Task.FromResult(m.GetStringPrefixLength(p));
        }
        #endregion

        #region CLIENT_EVENTS
        private async Task Client_Heartbeated(HeartbeatEventArgs e)
        {
            await _client.UpdateStatusAsync(new DiscordGame(_dependecies.StatusControl.GetRandomStatus()) {
                StreamType = GameStreamType.NoStream
            }).ConfigureAwait(false);
            SaveData();
        }

        private Task Client_Error(ClientErrorEventArgs e)
        {
            LogHandle.Log(LogLevel.Error, $"Client errored: {e.Exception.GetType()}: {e.Exception.Message}");
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            LogHandle.Log(LogLevel.Info, $"Guild available: {e.Guild.Name} ({e.Guild.Id})");
            return Task.CompletedTask;
        }

        private async Task Client_GuildMemberAdd(GuildMemberAddEventArgs e)
        {
            LogHandle.Log(LogLevel.Info,
                $"Member joined: {e.Member.Username} ({e.Member.Id})" + Environment.NewLine +
                $" Guild: {e.Guild.Name} ({e.Guild.Id})"
            );

            ulong cid = _dependecies.GuildConfigControl.GetGuildWelcomeChannelId(e.Guild.Id);
            if (cid == 0)
                return;

            try {
                await e.Guild.GetChannel(cid).SendMessageAsync($"Welcome to {Formatter.Bold(e.Guild.Name)}, {e.Member.Mention}!")
                    .ConfigureAwait(false);
            } catch (Exception exc) {
                while (exc is AggregateException)
                    exc = exc.InnerException;
                LogHandle.Log(LogLevel.Error,
                    $"Failed to send a welcome message!" + Environment.NewLine +
                    $" Channel ID: {cid}" + Environment.NewLine +
                    $" Exception: {exc.GetType()}" + Environment.NewLine +
                    $" Message: {exc.Message}"
                );

                if (exc is UnauthorizedException)
                    await e.Guild.Owner.SendMessageAsync("You have set a welcome message channel for me to post in, but I do not have permissions to do so. Please consider changing it. Guild: " + e.Guild.Name + " , Channel: " + e.Guild.GetChannel(cid).Name)
                        .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildMemberRemove(GuildMemberRemoveEventArgs e)
        {
            LogHandle.Log(LogLevel.Info,
                $"Member left: {e.Member.Username} ({e.Member.Id})" + Environment.NewLine +
                $" Guild: {e.Guild.Name} ({e.Guild.Id})"
            );

            ulong cid = _dependecies.GuildConfigControl.GetGuildLeaveChannelId(e.Guild.Id);
            if (cid == 0)
                return;

            try {
                await e.Guild.GetChannel(cid).SendMessageAsync($"{Formatter.Bold(e.Member?.Username ?? "<unknown>")} left the server. Bye!")
                    .ConfigureAwait(false);
            } catch (Exception exc) {
                while (exc is AggregateException)
                    exc = exc.InnerException;
                LogHandle.Log(LogLevel.Error,
                    $"Failed to send a leaving message!" + Environment.NewLine +
                    $" Channel ID: {cid}" + Environment.NewLine +
                    $" Exception: {exc.GetType()}" + Environment.NewLine +
                    $" Message: {exc.Message}"
                );
                if (exc is UnauthorizedException)
                    await e.Guild.Owner.SendMessageAsync("You have set a leave message channel for me to post in, but I do not have permissions to do so. Please consider changing it. Guild: " + e.Guild.Name + " , Channel: " + e.Guild.GetChannel(cid).Name)
                        .ConfigureAwait(false);
            }
        }

        private void Client_LogMessage(object sender, DebugLogMessageEventArgs e)
        {
            LogHandle.WriteToFile(e);
        }

        private async Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot)
                return;

            if (e.Channel.IsPrivate) {
                LogHandle.Log(LogLevel.Info, $"IGNORED DM FROM {e.Author.Username} ({e.Author.Id}): {e.Message}");
                return;
            }

            // Check if message contains filter
            if (e.Message.Content != null && _dependecies.GuildConfigControl.ContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Channel.DeleteMessageAsync(e.Message)
                        .ConfigureAwait(false);
                    LogHandle.Log(LogLevel.Info,
                        $"Filter triggered in message: '{e.Message.Content}'" + Environment.NewLine +
                        $" User: {e.Message.Author.ToString()}" + Environment.NewLine +
                        $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    LogHandle.Log(LogLevel.Warning,
                        $"Filter triggered in message but missing permissions to delete!" + Environment.NewLine +
                        $" Message: '{e.Message.Content}'" + Environment.NewLine +
                        $" User: {e.Message.Author.ToString()}" + Environment.NewLine +
                        $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
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
            int rank = _dependecies.RankControl.UpdateMessageCount(e.Author.Id);
            if (rank != -1) {
                var ranks = _dependecies.RankControl.Ranks;
                await e.Channel.SendMessageAsync($"GG {e.Author.Mention}! You have advanced to level {rank} ({(rank < ranks.Count ? ranks[rank] : "Low")})!")
                    .ConfigureAwait(false);
            }

            // Check if message has a text trigger
            var response = _dependecies.GuildConfigControl.GetResponseForTrigger(e.Guild.Id, e.Message.Content);
            if (response != null) {
                LogHandle.Log(LogLevel.Info,
                    $"Text trigger detected." + Environment.NewLine +
                    $" Message: {e.Message.Content}" + Environment.NewLine +
                    $" User: {e.Message.Author.ToString()}" + Environment.NewLine +
                    $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
                );
                await e.Channel.SendMessageAsync(response.Replace("%user%", e.Author.Mention))
                    .ConfigureAwait(false);
            }

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.AddReactions))
                return;

            // Check if message has a reaction trigger
            var emojilist = _dependecies.GuildConfigControl.GetReactionEmojis(_client, e.Guild.Id, e.Message.Content);
            if (emojilist.Count > 0) {
                LogHandle.Log(LogLevel.Info,
                    $"Reaction trigger detected." + Environment.NewLine +
                    $" Message: {e.Message.Content}" + Environment.NewLine +
                    $" User: {e.Message.Author.ToString()}" + Environment.NewLine +
                    $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
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
            if (!e.Author.IsBot && e.Message.Content != null && e.Message.Content.Split(' ').Any(s => _dependecies.GuildConfigControl.ContainsFilter(e.Guild.Id, s))) {
                try {
                    await e.Channel.DeleteMessageAsync(e.Message)
                        .ConfigureAwait(false);
                    LogHandle.Log(LogLevel.Info,
                        $"Filter triggered in edit of a message: '{e.Message.Content}'" + Environment.NewLine +
                        $" User: {e.Message.Author.ToString()}" + Environment.NewLine +
                        $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    LogHandle.Log(LogLevel.Warning,
                        $"Filter triggered in edited message but missing permissions to delete!" + Environment.NewLine +
                        $" Message: '{e.Message.Content}'" + Environment.NewLine +
                        $" User: {e.Message.Author.ToString()}" + Environment.NewLine +
                        $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
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

        private async Task Client_Ready(ReadyEventArgs e)
        {
            LogHandle.Log(LogLevel.Info, "Client ready.");
            await _client.UpdateStatusAsync(new DiscordGame(_dependecies.StatusControl.GetRandomStatus()) {
                StreamType = GameStreamType.NoStream
            }).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_EVENTS
        private async Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            await Task.Yield();

            LogHandle.Log(LogLevel.Info,
                $"Executed: {e.Command?.QualifiedName ?? "<unknown command>"}" + Environment.NewLine +
                $" User: {e.Context.User.ToString()}" + Environment.NewLine +
                $" Location: '{e.Context.Guild.Name}' ({e.Context.Guild.Id}) ; {e.Context.Channel.ToString()}"
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

            LogHandle.Log(LogLevel.Error,
                $"Tried executing: {e.Command?.QualifiedName ?? "<unknown command>"}" + Environment.NewLine +
                $" User: {e.Context.User.ToString()}" + Environment.NewLine +
                $" Location: '{e.Context.Guild.Name}' ({e.Context.Guild.Id}) ; {e.Context.Channel.ToString()}" + Environment.NewLine +
                $" Exception: {ex.GetType()}" + Environment.NewLine +
                (ex.InnerException != null ? $" Inner exception: {ex.InnerException.GetType()}" + Environment.NewLine : "") +
                $" Message: {ex.Message ?? "<no message>"}"
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
            else if (e.Exception is NotSupportedException)
                embed.Description = $"{emoji} Not supported. {e.Exception.Message}";
            else if (e.Exception is InvalidOperationException)
                embed.Description = $"{emoji} Invalid operation. {e.Exception.Message}";
            else if (e.Exception is NotFoundException)
                embed.Description = $"{emoji} 404: Not found.";
            else if (e.Exception is ArgumentException)
                embed.Description = $"{emoji} Argument specified is invalid (please use {Formatter.Bold("!help <command>")}).";
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

        #region GETTERS_AND_SETTERS
        internal void ToggleListening()
        {
            Listening = !Listening;
        }
        #endregion
    }
}