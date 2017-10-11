#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Exceptions;
using TheGodfather.Helpers;

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
    public class TheGodfather
    {
        #region STATIC_FIELDS
        private static DiscordClient _client { get; set; }
        private static CommandsNextModule _commands { get; set; }
        private static InteractivityModule _interactivity { get; set; }
        private static VoiceNextClient _voice { get; set; }

        private static StreamWriter _logstream { get; set; }
        private static object _lock { get; set; }

        private static ConcurrentDictionary<ulong, string> _prefixes { get; set; }

        public IReadOnlyList<string> Statuses => _statuses;
        private List<string> _statuses;
        public static BotConfig Config { get; internal set; }

        #endregion


        public TheGodfather()
        {
            _lock = new object();
            _logstream = null;
            _prefixes = new ConcurrentDictionary<ulong, string>();
            _statuses = new List<string> { "!help", "worldmafia.net", "worldmafia.net/discord" };
        }


        ~TheGodfather()
        {
            _client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", "Shutting down by demand...", DateTime.Now);

            SaveData();

            if (_logstream != null)
                _logstream.Close();
            _client.DisconnectAsync();
            _client.Dispose();
        }


        public async Task MainAsync(string[] args)
        {
            try {
                string data = "";
                using (var fs = File.OpenRead("Resources/config.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    data = await sr.ReadToEndAsync();
                Config = JsonConvert.DeserializeObject<BotConfig>(data);
            } catch (Exception e) {
                _client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", $"Settings loading error: {e.GetType()}: {e.Message}", DateTime.Now);
                Environment.Exit(1);
            }

            SetupClient();
            OpenLogFile();
            SetupCommands();
            SetupInteractivity();
            SetupVoice();
            LoadData();

            await _client.ConnectAsync();

            await Task.Delay(-1);
        }


        #region HELPER_FUNCTIONS
        public static void OpenLogFile()
        {
            try {
                _logstream = new StreamWriter("log.txt", append: true);
            } catch (Exception e) {
                Console.WriteLine("Cannot open log file. Details: " + e.Message);
                return;
            }

            try {
                lock (_lock) {
                    _logstream.WriteLine($"\n*** NEW INSTANCE STARTED AT {DateTime.Now.ToLongDateString()} : {DateTime.Now.ToLongTimeString()} ***\n");
                    _logstream.Flush();
                }
            } catch (Exception e) {
                _client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "Cannot write to log file. Details: " + e.Message, DateTime.Now);
            }
        }

        public static void CloseLogFile()
        {
            if (_logstream != null) {
                _logstream.Close();
                _logstream = null;
            }
        }

        private void SetupClient()
        {
            _client = new DiscordClient(new DiscordConfiguration {
                LogLevel = LogLevel.Debug,
                LargeThreshold = 250,
                AutoReconnect = true,
                Token = Config.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
            });
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

            // Windows 7 specific
            _client.SetWebSocketClient<WebSocket4NetClient>();
        }

        private void SetupCommands()
        {
            _commands = _client.UseCommandsNext(new CommandsNextConfiguration {
                EnableDms = false,
                CaseSensitive = false,
                EnableMentionPrefix = true,
                CustomPrefixPredicate = async m => await CheckMessageForPrefix(m),
                Dependencies = new DependencyCollectionBuilder().AddInstance(this).Build()
            });

            _commands.SetHelpFormatter<HelpFormatter>();

            _commands.RegisterCommands<Commands.Administration.CommandsAdmin>();
            _commands.RegisterCommands<Commands.Administration.CommandsChannels>();
            _commands.RegisterCommands<Commands.Administration.CommandsGuild>();
            _commands.RegisterCommands<Commands.Administration.CommandsMessages>();
            _commands.RegisterCommands<Commands.Administration.CommandsRoles>();
            _commands.RegisterCommands<Commands.Administration.CommandsUsers>();
            _commands.RegisterCommands<Commands.Games.CommandsBank>();
            _commands.RegisterCommands<Commands.Games.CommandsGamble>();
            _commands.RegisterCommands<Commands.Games.CommandsGames>();
            _commands.RegisterCommands<Commands.Main.CommandsMain>();
            _commands.RegisterCommands<Commands.Main.CommandsRandom>();
            _commands.RegisterCommands<Commands.Messages.CommandsAlias>();
            _commands.RegisterCommands<Commands.Messages.CommandsFilter>();
            _commands.RegisterCommands<Commands.Messages.CommandsInsult>();
            _commands.RegisterCommands<Commands.Messages.CommandsMemes>();
            _commands.RegisterCommands<Commands.Messages.CommandsPoll>();
            _commands.RegisterCommands<Commands.Messages.CommandsRanking>();
            _commands.RegisterCommands<Commands.Messages.CommandsReaction>();
            _commands.RegisterCommands<Commands.Search.CommandsGiphy>();
            _commands.RegisterCommands<Commands.Search.CommandsImgur>();
            //_commands.RegisterCommands<Modules.Search.CommandsReddit>();
            _commands.RegisterCommands<Commands.Search.CommandsRSS>();
            _commands.RegisterCommands<Commands.Search.CommandsSteam>();
            _commands.RegisterCommands<Commands.Search.CommandsUrbanDict>();
            _commands.RegisterCommands<Commands.Search.CommandsYoutube>();
            _commands.RegisterCommands<Commands.SWAT.CommandsSwat>();
            //_commands.RegisterCommands<Modules.Voice.CommandsVoice>();

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
            Exception exc = null;
            try {
                Commands.Messages.CommandsAlias.LoadAliases(_client.DebugLogger);
                Commands.Messages.CommandsFilter.LoadFilters(_client.DebugLogger);
                Commands.Messages.CommandsMemes.LoadMemes(_client.DebugLogger);
                Commands.Messages.CommandsRanking.LoadRanks(_client.DebugLogger);
                Commands.Messages.CommandsReaction.LoadReactions(_client.DebugLogger);
                Commands.SWAT.CommandsSwat.LoadServers(_client.DebugLogger);
                Commands.Messages.CommandsInsult.LoadInsults(_client.DebugLogger);
            } catch (Exception e) {
                exc = e;
            }

            if (exc == null)
                _client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", "Data loaded.", DateTime.Now);
            else
                _client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "Errors occured during data load.", DateTime.Now);
        }

        private void SaveData()
        {
            Exception exc = null;
            try {
                Commands.Messages.CommandsAlias.SaveAliases(_client.DebugLogger);
                Commands.Messages.CommandsFilter.SaveFilters(_client.DebugLogger);
                Commands.Messages.CommandsMemes.SaveMemes(_client.DebugLogger);
                Commands.Messages.CommandsRanking.SaveRanks(_client.DebugLogger);
                Commands.Messages.CommandsReaction.SaveReactions(_client.DebugLogger);
                Commands.SWAT.CommandsSwat.SaveServers(_client.DebugLogger);
                Commands.Messages.CommandsInsult.SaveInsults(_client.DebugLogger);
            } catch (Exception e) {
                exc = e;
            }

            if (exc == null)
                _client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", "Data saved.", DateTime.Now);
            else
                _client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "Errors occured during data save.", DateTime.Now);

        }

        private Task<int> CheckMessageForPrefix(DiscordMessage m)
        {
            string prefix = _prefixes.ContainsKey(m.ChannelId) ? _prefixes[m.ChannelId] : Config.DefaultPrefix;
            int pos = m.Content.IndexOf(prefix);

            if (pos != 0)
                return Task.FromResult(-1);
            else
                return Task.FromResult(prefix.Length);
        }
        #endregion


        #region CLIENT_EVENTS
        private async Task Client_Heartbeated(HeartbeatEventArgs e)
        {
            await _client.UpdateStatusAsync(new DiscordGame(Statuses[new Random().Next(Statuses.Count)]) { StreamType = GameStreamType.NoStream });
            SaveData();
        }

        private Task Client_Error(ClientErrorEventArgs e)
        {
            _client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            _client.DebugLogger.LogMessage(
                LogLevel.Info,
                "TheGodfather",
                $"Guild available: {e.Guild.Name} ({e.Guild.Id})",
                DateTime.Now);
            return Task.CompletedTask;
        }

        private async Task Client_GuildMemberAdd(GuildMemberAddEventArgs e)
        {
            _client.DebugLogger.LogMessage(
                   LogLevel.Info,
                   "TheGodfather",
                   $"Member joined: {e.Member.Username} ({e.Member.Id})\n" +
                   $" Guild: {e.Guild.Name} ({e.Guild.Id})",
                   DateTime.Now
            );

            try {
                await e.Guild.GetDefaultChannel().SendMessageAsync($"Welcome to {Formatter.Bold(e.Guild.Name)}, {e.Member.Mention}!");
            } catch (Exception exc) {
                while (exc is AggregateException)
                    exc = exc.InnerException;
                _client.DebugLogger.LogMessage(
                   LogLevel.Error,
                   "TheGodfather",
                   $"Failed to send a welcome message!\n" +
                   $" Member joined: {e.Member.Username} ({e.Member.Id})\n" +
                   $" Guild: {e.Guild.Name} ({e.Guild.Id})" +
                   $" Exception: {exc.GetType()}" +
                   $" Message: {exc.Message}",
                   DateTime.Now
                );
            }
        }

        private async Task Client_GuildMemberRemove(GuildMemberRemoveEventArgs e)
        {
            _client.DebugLogger.LogMessage(
                LogLevel.Info,
                "TheGodfather",
                $"Member left: {e.Member.Username} ({e.Member.Id})\n" +
                $" Guild: {e.Guild.Name} ({e.Guild.Id})",
                DateTime.Now
            );
            try {
                await e.Guild.GetDefaultChannel().SendMessageAsync($"{Formatter.Bold(e.Member?.Username ?? "<unknown>")} left the server. Bye!");
            } catch (Exception exc) {
                while (exc is AggregateException)
                    exc = exc.InnerException;
                _client.DebugLogger.LogMessage(
                   LogLevel.Error,
                   "TheGodfather",
                   $"Failed to send a leaving message!\n" +
                   $" Member left: {e.Member.Username} ({e.Member.Id})\n" +
                   $" Guild: {e.Guild.Name} ({e.Guild.Id})" +
                   $" Exception: {exc.GetType()}" +
                   $" Message: {exc.Message}",
                   DateTime.Now
                );
            }
        }

        private void Client_LogMessage(object sender, DebugLogMessageEventArgs e)
        {
            if (_logstream == null)
                return;

            try {
                lock (_lock) {
                    _logstream.WriteLine($"[{e.Timestamp}] [{e.Level}]{Environment.NewLine}{e.Message}");
                    _logstream.Flush();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Message.Author.IsBot)
                return;

            if (e.Channel.IsPrivate) {
                _client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", $"IGNORED DM: {e.Author.Username} : {e.Message}", DateTime.Now);
                return;
            }

            // Check if message contains filter
            if (!e.Author.IsBot && e.Message.Content != null && e.Message.Content.Split(' ').Any(s => Commands.Messages.CommandsFilter.ContainsFilter(e.Guild.Id, s))) {
                try {
                    await e.Channel.DeleteMessageAsync(e.Message);
                    _client.DebugLogger.LogMessage(
                        LogLevel.Info,
                        "TheGodfather",
                        $"Filter triggered in message: '{e.Message.Content}'\n" +
                        $" User: {e.Message.Author.ToString()}\n" +
                        $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
                        , DateTime.Now
                    );
                } catch (UnauthorizedException) {
                    _client.DebugLogger.LogMessage(
                        LogLevel.Warning,
                        "TheGodfather",
                        $"Filter triggered in message but missing permissions to delete!\n" +
                        $" Message: '{e.Message.Content}'\n" +
                        $" User: {e.Message.Author.ToString()}\n" +
                        $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
                        , DateTime.Now
                    );
                    await e.Channel.SendMessageAsync("The message contains the filtered word but I do not have permissions to delete it.");
                }
                return;
            }

            // Update message count for the user that sent the message
            Commands.Messages.CommandsRanking.UpdateMessageCount(e.Channel, e.Author);

            // Check if message has an alias
            var response = Commands.Messages.CommandsAlias.FindAlias(e.Guild.Id, e.Message.Content);
            if (response != null) {
                _client.DebugLogger.LogMessage(
                    LogLevel.Info,
                    "TheGodfather",
                    $"Alias triggered: {e.Message.Content}\n" +
                    $" User: {e.Message.Author.ToString()}\n" +
                    $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
                    , DateTime.Now
                );
                var split = response.Split(new string[] { "%user%" }, StringSplitOptions.None);
                await e.Channel.SendMessageAsync(string.Join(e.Author.Mention, split));
            }

            // Check if message has react trigger
            var emojilist = Commands.Messages.CommandsReaction.GetReactionEmojis(_client, e.Guild.Id, e.Message.Content);
            if (emojilist.Count > 0) {
                _client.DebugLogger.LogMessage(
                    LogLevel.Info,
                    "TheGodfather",
                    $"Reactions triggered in message: {e.Message.Content}\n" +
                    $" User: {e.Message.Author.ToString()}\n" +
                    $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
                    , DateTime.Now
                );
                foreach (var emoji in emojilist) {
                    try {
                        await e.Message.CreateReactionAsync(emoji);
                    } catch (ArgumentException) {
                        await e.Channel.SendMessageAsync($"I have a reaction for that message set up ({emoji}) but that emoji doesn't exits. Fix your shit pls.");
                    }
                    await Task.Delay(1000);
                }
            }
        }

        private async Task Client_MessageUpdated(MessageUpdateEventArgs e)
        {
            // Check if message contains filter
            if (!e.Author.IsBot && e.Message.Content != null && e.Message.Content.Split(' ').Any(s => Commands.Messages.CommandsFilter.ContainsFilter(e.Guild.Id, s))) {
                try {
                    await e.Channel.DeleteMessageAsync(e.Message);
                    _client.DebugLogger.LogMessage(
                        LogLevel.Info,
                        "TheGodfather",
                        $"Filter triggered in edit of a message: '{e.Message.Content}'\n" +
                        $" User: {e.Message.Author.ToString()}\n" +
                        $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
                        , DateTime.Now
                    );
                } catch (UnauthorizedException) {
                    _client.DebugLogger.LogMessage(
                        LogLevel.Warning,
                        "TheGodfather",
                        $"Filter triggered in edited message but missing permissions to delete!\n" +
                        $" Message: '{e.Message.Content}'\n" +
                        $" User: {e.Message.Author.ToString()}\n" +
                        $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
                        , DateTime.Now
                    );
                    await e.Channel.SendMessageAsync("The edited message contains the filtered word but I do not have permissions to delete it.");
                }
                await e.Channel.SendMessageAsync($"Nice try, {e.Author.Mention}! But I see throught it!");
            }
        }

        private async Task Client_ReactToMessage(MessageReactionAddEventArgs e)
        {
            if (new Random().Next(10) == 0)
                await e.Message.CreateReactionAsync(e.Emoji);
        }

        private async Task Client_Ready(ReadyEventArgs e)
        {
            _client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", "Ready.", DateTime.Now);
            await _client.UpdateStatusAsync(new DiscordGame(Statuses[0]) { StreamType = GameStreamType.NoStream });
        }
        #endregion


        #region COMMAND_EVENTS
        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(
                LogLevel.Info,
                "TheGodfather",
                $" Executed: {e.Command?.QualifiedName ?? "<unknown command>"}" + Environment.NewLine +
                $" User: {e.Context.User.ToString()}" + Environment.NewLine +
                $" Location: '{e.Context.Guild.Name}' ({e.Context.Guild.Id}) ; {e.Context.Channel.ToString()}"
                , DateTime.Now
            );
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            if (e.Exception == null)
                return;

            var ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException;

            e.Context.Client.DebugLogger.LogMessage(
                LogLevel.Error,
                "TheGodfather",
                $" Tried executing: {e.Command?.QualifiedName ?? "<unknown command>"}" + Environment.NewLine +
                $" User: {e.Context.User.ToString()}" + Environment.NewLine +
                $" Location: '{e.Context.Guild.Name}' ({e.Context.Guild.Id}) ; {e.Context.Channel.ToString()}" + Environment.NewLine +
                $" Exception: {ex.GetType()}" + Environment.NewLine +
                (ex.InnerException != null ? $" Inner exception: {ex.InnerException.GetType()}" + Environment.NewLine : "") +
                $" Message: {ex.Message ?? "<no message>"}"
                , DateTime.Now
            );

            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
            var embed = new DiscordEmbedBuilder {
                Title = "Error",
                Color = DiscordColor.Red
            };

            if (e.Exception is CommandNotFoundException)
                embed.Description = $"{emoji} The specified command does not exist.";
            else if (e.Exception is NotSupportedException)
                embed.Description = $"{emoji} That command group is not executable without subcommands.";
            else if (e.Exception is InvalidCommandUsageException)
                embed.Description = $"{emoji} Invalid usage! {ex.Message}";
            else if (e.Exception is ArgumentException)
                embed.Description = $"{emoji} Wrong argument format (please use **!help <command>**).";
            else if (e.Exception is CommandFailedException)
                embed.Description = $"{emoji} {ex.Message}";
            else if (ex is ChecksFailedException exc) {
                var attr = exc.FailedChecks.First();
                if (attr is CooldownAttribute)
                    embed.Description = $"{emoji} CHILL!";
                else if (attr is RequireUserPermissionsAttribute)
                    embed.Description = $"{emoji} You do not have the required permissions to run this command!";
                else if (attr is RequirePermissionsAttribute)
                    embed.Description = $"{emoji} Permissions to execute that command aren't met!";
            } else if (e.Exception is UnauthorizedException)
                embed.Description = $"{emoji} I am not authorized to do that.";
            else
                embed.Description = $"{emoji} Unknown error occured (probably because a Serbian made this bot). Please **!report**.";

            await e.Context.RespondAsync("", embed: embed);
        }
        #endregion


        #region GETTERS_AND_SETTERS
        public static string PrefixFor(ulong cid)
        {
            if (_prefixes.ContainsKey(cid))
                return _prefixes[cid];
            else
                return Config.DefaultPrefix;
        }

        public static void SetPrefix(ulong cid, string prefix)
        {
            if (_prefixes.ContainsKey(cid))
                _prefixes[cid] = prefix;
            _prefixes.TryAdd(cid, prefix);
        }

        public void AddStatus(string status)
        {
            if (_statuses.Contains(status))
                return;
            _statuses.Add(status);
        }

        public void DeleteStatus(string status)
        {
            _statuses.RemoveAll(s => s.ToLower() == status.ToLower());
        }
        #endregion
    }
}