#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TheGodfatherBot.Exceptions;
using TheGodfatherBot.Helpers;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.VoiceNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfatherBot
{
    public class TheGodfather
    {
        #region STATIC_FIELDS
        static DiscordClient _client { get; set; }
        static CommandsNextModule _commands { get; set; }
        static InteractivityModule _interactivity { get; set; }
        static VoiceNextClient _voice { get; set; }
        public static List<string> _statuses = new List<string> { "!help" , "worldmafia.net", "worldmafia.net/discord" };
        #endregion

        #region PRIVATE_FIELDS
        private static StreamWriter _logstream = null;
        private static EventWaitHandle _logwritelock = null;
        #endregion
        

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
                _logwritelock = new EventWaitHandle(true, EventResetMode.AutoReset, "SHARED_BY_ALL_PROCESSES");
            } catch (Exception e) {
                Console.WriteLine("Cannot open log file. Details: " + e.Message);
                return;
            }

            try {
                _logwritelock.WaitOne();
                _logstream.WriteLine();
                _logstream.WriteLine($"*** NEW INSTANCE STARTED AT {DateTime.Now.ToLongDateString()} : {DateTime.Now.ToLongTimeString()} ***");
                _logstream.WriteLine();
                _logstream.Flush();
                _logwritelock.Set();
            } catch (Exception e) {
                _client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "Cannot write to log file. Details: " + e.Message, DateTime.Now);
            }
        }

        public static void CloseLogFile()
        {
            if (_logwritelock != null && _logstream != null) {
                _logstream.Close();
                _logwritelock.Dispose();
                _logstream = null;
                _logwritelock = null;
            }
        }

        private void SetupClient()
        {
            _client = new DiscordClient(new DiscordConfiguration {
                LargeThreshold = 250,
                AutoReconnect = true,
                Token = TokenReader.GetToken("Resources/token.txt"),
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
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
                StringPrefix = "!",
                EnableDms = false,
                CaseSensitive = false,
                EnableMentionPrefix = true
            });

            _commands.SetHelpFormatter<HelpFormatter>();

            _commands.RegisterCommands<Modules.Admin.CommandsAdmin>();
            _commands.RegisterCommands<Modules.Admin.CommandsChannels>();
            _commands.RegisterCommands<Modules.Admin.CommandsGuild>();
            _commands.RegisterCommands<Modules.Admin.CommandsMessages>();
            _commands.RegisterCommands<Modules.Admin.CommandsRoles>();
            _commands.RegisterCommands<Modules.Admin.CommandsUsers>();
            _commands.RegisterCommands<Modules.Games.CommandsBank>();
            _commands.RegisterCommands<Modules.Games.CommandsCards>();
            _commands.RegisterCommands<Modules.Games.CommandsGamble>();
            _commands.RegisterCommands<Modules.Games.CommandsGames>();
            _commands.RegisterCommands<Modules.Games.CommandsNunchi>();
            _commands.RegisterCommands<Modules.Games.CommandsRace>();
            _commands.RegisterCommands<Modules.Games.CommandsQuiz>();
            _commands.RegisterCommands<Modules.Main.CommandsMain>();
            _commands.RegisterCommands<Modules.Main.CommandsRandom>();
            _commands.RegisterCommands<Modules.Messages.CommandsAlias>();
            _commands.RegisterCommands<Modules.Messages.CommandsFilter>();
            _commands.RegisterCommands<Modules.Messages.CommandsInsult>();
            _commands.RegisterCommands<Modules.Messages.CommandsMemes>();
            _commands.RegisterCommands<Modules.Messages.CommandsPoll>();
            _commands.RegisterCommands<Modules.Messages.CommandsRanking>();
            _commands.RegisterCommands<Modules.Search.CommandsGiphy>();
            _commands.RegisterCommands<Modules.Search.CommandsImgur>();
            //_commands.RegisterCommands<Modules.Search.CommandsReddit>();
            _commands.RegisterCommands<Modules.Search.CommandsRSS>();
            _commands.RegisterCommands<Modules.Search.CommandsSteam>();
            _commands.RegisterCommands<Modules.Search.CommandsUrbanDict>();
            _commands.RegisterCommands<Modules.Search.CommandsYoutube>();
            _commands.RegisterCommands<Modules.SWAT.CommandsSwat>();
            //_commands.RegisterCommands<Modules.Voice.CommandsVoice>();
            _commands.CommandExecuted += Commands_CommandExecuted;
            _commands.CommandErrored += Commands_CommandErrored;
        }

        private void SetupInteractivity()
        {
            _interactivity = _client.UseInteractivity();
        }

        private void SetupVoice()
        {
            _voice = _client.UseVoiceNext();
        }

        private void LoadData()
        {
            _client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", "Loading data...", DateTime.Now);

            Exception exc = null;
            try {
                Modules.Messages.CommandsAlias.LoadAliases(_client.DebugLogger);
                Modules.Messages.CommandsFilter.LoadFilters(_client.DebugLogger);
                Modules.Messages.CommandsMemes.LoadMemes(_client.DebugLogger);
                Modules.Messages.CommandsRanking.LoadRanks(_client.DebugLogger);
                Modules.SWAT.CommandsSwat.LoadServers(_client.DebugLogger);
                Modules.Messages.CommandsInsult.LoadInsults(_client.DebugLogger);
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
            _client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", "Saving data...", DateTime.Now);

            Exception exc = null;
            try {
                Modules.Messages.CommandsAlias.SaveAliases(_client.DebugLogger);
                Modules.Messages.CommandsFilter.SaveFilters(_client.DebugLogger);
                Modules.Messages.CommandsMemes.SaveMemes(_client.DebugLogger);
                Modules.Messages.CommandsRanking.SaveRanks(_client.DebugLogger);
                Modules.SWAT.CommandsSwat.SaveServers(_client.DebugLogger);
                Modules.Messages.CommandsInsult.SaveInsults(_client.DebugLogger);
            } catch (Exception e) {
                exc = e;
            }

            if (exc == null)
                _client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", "Data saved.", DateTime.Now);
            else
                _client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "Errors occured during data save.", DateTime.Now);

        }
        #endregion


        #region CLIENT_EVENTS
        private async Task Client_Heartbeated(HeartbeatEventArgs e)
        {
            await _client.UpdateStatusAsync(new Game(_statuses[new Random().Next(_statuses.Count)]) { StreamType = GameStreamType.NoStream });
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
                $"Guild available: '{e.Guild.Name}' ({e.Guild.Id})",
                DateTime.Now);
            return Task.CompletedTask;
        }

        private async Task Client_GuildMemberAdd(GuildMemberAddEventArgs e)
        {
            _client.DebugLogger.LogMessage(
                   LogLevel.Info,
                   "TheGodfather",
                   $"Member join: {e.Member.Username} ({e.Member.Id})\n" +
                   $" Guild: '{e.Guild.Name}' ({e.Guild.Id})",
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
                   $"Member leave: {e.Member.Username} ({e.Member.Id})\n" +
                   $" Guild: '{e.Guild.Name}' ({e.Guild.Id})",
                   DateTime.Now
                );
            }
        }

        private async Task Client_GuildMemberRemove(GuildMemberRemoveEventArgs e)
        {
            _client.DebugLogger.LogMessage(
                LogLevel.Info, 
                "TheGodfather", 
                $"Member leave: {e.Member.Username} ({e.Member.Id})\n" +
                $" Guild: '{e.Guild.Name}' ({e.Guild.Id})", 
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
                   $"Member leave: {e.Member.Username} ({e.Member.Id})\n" +
                   $" Guild: '{e.Guild.Name}' ({e.Guild.Id})",
                   DateTime.Now
                );
            }
        }

        private void Client_LogMessage(object sender, DebugLogMessageEventArgs e)
        {
            if (_logstream == null)
                return;

            try {
                _logwritelock.WaitOne();
                _logstream.WriteLine($"*[{e.Timestamp}] [{e.Level}]\t{e.Message}");
                _logstream.Flush();
                _logwritelock.Set();
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
            if (e.Message.Content.Split(' ').Any(s => Modules.Messages.CommandsFilter.ContainsFilter(e.Guild.Id, s))) {
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

            Modules.Messages.CommandsRanking.UpdateMessageCount(e.Channel, e.Author);

            // React to random message
            var r = new Random();
            if (e.Guild.Emojis.Count > 0 && r.Next(10) == 0)
                await e.Message.CreateReactionAsync(e.Guild.Emojis.ElementAt(r.Next(e.Guild.Emojis.Count)));

            // Check if message has an alias
            var response = Modules.Messages.CommandsAlias.FindAlias(e.Guild.Id, e.Message.Content);
            if (response != null) {
                _client.DebugLogger.LogMessage(
                    LogLevel.Info,
                    "TheGodfather",
                    $"Alias triggered: {e.Message.Content}\n" +
                    $" User: {e.Message.Author.ToString()}\n" +
                    $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
                    , DateTime.Now
                );
                await e.Channel.SendMessageAsync(response);
            }
        }

        private async Task Client_ReactToMessage(MessageReactionAddEventArgs e)
        {
            if (new Random().Next(10) == 0)
                await e.Message.CreateReactionAsync(e.Emoji);
        }
        
        private async Task Client_MessageUpdated(MessageUpdateEventArgs e)
        {
            // Check if message contains filter
            if (Modules.Messages.CommandsFilter.ContainsFilter(e.Guild.Id, e.Message.Content)) {
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

        private async Task Client_Ready(ReadyEventArgs e)
        {
            _client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", "Ready.", DateTime.Now);
            await _client.UpdateStatusAsync(new Game(_statuses[0]) { StreamType = GameStreamType.NoStream });
        }
        #endregion


        #region COMMAND_EVENTS
        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(
                LogLevel.Info, 
                "TheGodfather",
                $"Executed: {e.Command?.QualifiedName ?? "<unknown command>"}\n" +
                $" User: {e.Context.User.ToString()}\n" +
                $" Location: '{e.Context.Guild.Name}' ({e.Context.Guild.Id}) ; {e.Context.Channel.ToString()}"
                , DateTime.Now
            );
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            var ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException;

            e.Context.Client.DebugLogger.LogMessage(
                LogLevel.Error, 
                "TheGodfather",
                $"Tried executing: {e.Command?.QualifiedName ?? "<unknown command>"}\n" +
                $" User: {e.Context.User.ToString()}\n" +
                $" Location: '{e.Context.Guild.Name}' ({e.Context.Guild.Id}) ; {e.Context.Channel.ToString()}\n" +
                $" Exception: {ex.GetType()}\n" + 
                (ex.InnerException != null ? $" Inner exception: {ex.InnerException.GetType()}\n" : "") +
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
            else if (ex is ChecksFailedException)
                embed.Description = $"{emoji} Either you or I don't have the permissions required to execute this command.";
            else if (e.Exception is UnauthorizedException)
                embed.Description = $"{emoji} I am not authorized to do that.";
            else
                embed.Description = $"{emoji} Unknown error occured (probably because a Serbian made this bot). Please **!report**.";

            await e.Context.RespondAsync("", embed: embed);
        }
        #endregion
    }
}