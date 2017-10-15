#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

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

namespace TheGodfather
{
    public class TheGodfather
    {
        #region PRIVATE_FIELDS
        private DiscordClient _client { get; set; }
        private CommandsNextModule _commands { get; set; }
        private InteractivityModule _interactivity { get; set; }
        private VoiceNextClient _voice { get; set; }

        private BotConfigManager _config { get; set; }
        private BotDependencyList _dependecies { get; set; }

        internal Logger LogHandle { get; private set; }
        #endregion


        public TheGodfather()
        {
            _config = new BotConfigManager();
            _dependecies = new BotDependencyList();
        }

        ~TheGodfather()
        {
            LogHandle.Log(LogLevel.Info, "Shutting down by demand...");

            SaveData();
            LogHandle.ClearLogFile();
            _client.DisconnectAsync();
            _client.Dispose();
        }


        public async Task MainAsync(string[] args)
        {
            _config.Load();

            SetupClient();
            SetupCommands();
            SetupInteractivity();
            SetupVoice();
            LoadData();

            await _client.ConnectAsync();

            await Task.Delay(-1);
        }
        
        
        #region BOT_SETUP_FUNCTIONS
        private void SetupClient()
        {
            _client = new DiscordClient(new DiscordConfiguration {
                LogLevel = LogLevel.Debug,
                LargeThreshold = 250,
                AutoReconnect = true,
                Token = _config.CurrentConfig.Token,
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
                Dependencies = _dependecies.GetDependencyCollectionBuilder()
                                           .AddInstance(this)
                                           .AddInstance(_config)
                                           .Build()
            });

            _commands.SetHelpFormatter<HelpFormatter>();

            _commands.RegisterCommands<Commands.Administration.CommandsOwner>();
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
            _commands.RegisterCommands<Commands.Search.CommandsJokes>();
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
            try {
                _dependecies.LoadData(_client.DebugLogger);
            } catch (Exception e) {
                LogHandle.Log(LogLevel.Error,
                    $"Errors occured during data load: " + Environment.NewLine +
                    $" Exception: {e.GetType()}" + Environment.NewLine +
                    (e.InnerException != null ? $" Inner exception: {e.GetType()}" + Environment.NewLine : "") +
                    $" Message: {e.Message}" 
                );
                return;
            }

            LogHandle.Log(LogLevel.Info, "Data loaded.");
        }

        private void SaveData()
        {
            try {
                _dependecies.SaveData(_client.DebugLogger);
            } catch (Exception e) {
                LogHandle.Log(LogLevel.Error,
                    $"Errors occured during data save: " + Environment.NewLine +
                    $" Exception: {e.GetType()}" + Environment.NewLine +
                    (e.InnerException != null ? $" Inner exception: {e.GetType()}" + Environment.NewLine : "") +
                    $" Message: {e.Message}"
                );
                return;
            }

            LogHandle.Log(LogLevel.Info, "Data saved.");
        }

        private Task<int> CheckMessageForPrefix(DiscordMessage m)
        {
            string prefix;
            if (_dependecies.PrefixControl.Prefixes.ContainsKey(m.ChannelId))
                prefix = _dependecies.PrefixControl.Prefixes[m.ChannelId];
            else
                prefix = _config.CurrentConfig.DefaultPrefix;

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
            await _client.UpdateStatusAsync(new DiscordGame(_dependecies.StatusControl.GetRandomStatus()) { StreamType = GameStreamType.NoStream });
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

            ulong cid = _dependecies.ChannelControl.GetWelcomeChannelId(e.Guild.Id);
            if (cid == 0)
                return;

            try {
                await e.Guild.GetChannel(cid).SendMessageAsync($"Welcome to {Formatter.Bold(e.Guild.Name)}, {e.Member.Mention}!");
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
                    await e.Guild.Owner.SendMessageAsync("You have set a welcome message channel for me to post in, but I do not have permissions to do so. Please consider changing it. Guild: " + e.Guild.Name + " , Channel: " + e.Guild.GetChannel(cid).Name);
            }
        }

        private async Task Client_GuildMemberRemove(GuildMemberRemoveEventArgs e)
        {
            LogHandle.Log(LogLevel.Info,
                $"Member left: {e.Member.Username} ({e.Member.Id})" + Environment.NewLine +
                $" Guild: {e.Guild.Name} ({e.Guild.Id})"
            );

            ulong cid = _dependecies.ChannelControl.GetLeaveChannelId(e.Guild.Id);
            if (cid == 0)
                return;

            try {
                await e.Guild.GetChannel(cid).SendMessageAsync($"{Formatter.Bold(e.Member?.Username ?? "<unknown>")} left the server. Bye!");
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
                    await e.Guild.Owner.SendMessageAsync("You have set a leave message channel for me to post in, but I do not have permissions to do so. Please consider changing it. Guild: " + e.Guild.Name + " , Channel: " + e.Guild.GetChannel(cid).Name);
            }
        }

        private void Client_LogMessage(object sender, DebugLogMessageEventArgs e)
        {
            LogHandle.WriteToFile(e);
        }

        private async Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Message.Author.IsBot)
                return;

            if (e.Channel.IsPrivate) {
                LogHandle.Log(LogLevel.Info, $"IGNORED DM FROM {e.Author.Username} ({e.Author.Id}): {e.Message}");
                return;
            }

            // Check if message contains filter
            if (!e.Author.IsBot && e.Message.Content != null && e.Message.Content.Split(' ').Any(s => _dependecies.FilterControl.Contains(e.Guild.Id, s))) {
                try {
                    await e.Channel.DeleteMessageAsync(e.Message);
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
                    await e.Channel.SendMessageAsync("The message contains the filtered word but I do not have permissions to delete it.");
                }
                return;
            }

            // Update message count for the user that sent the message
            int rank = _dependecies.RankControl.UpdateMessageCount(e.Author.Id);
            if (rank != -1) {
                var ranks = _dependecies.RankControl.Ranks;
                await e.Channel.SendMessageAsync($"GG {e.Author.Mention}! You have advanced to level {rank} ({(rank < ranks.Count ? ranks[rank] : "Low")})!");
            }

            // Check if message has an alias
            var response = _dependecies.AliasControl.GetResponse(e.Guild.Id, e.Message.Content);
            if (response != null) {
                LogHandle.Log(LogLevel.Info,
                    $"Alias triggered: {e.Message.Content}" + Environment.NewLine +
                    $" User: {e.Message.Author.ToString()}" + Environment.NewLine +
                    $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
                );
                await e.Channel.SendMessageAsync(response.Replace("%user%", e.Author.Mention));
            }

            // Check if message has react trigger
            var emojilist = _dependecies.ReactionControl.GetReactionEmojis(_client, e.Guild.Id, e.Message.Content);
            if (emojilist.Count > 0) {
                LogHandle.Log(LogLevel.Info,
                    $"Reactions triggered in message: {e.Message.Content}" + Environment.NewLine +
                    $" User: {e.Message.Author.ToString()}" + Environment.NewLine +
                    $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
                );
                foreach (var emoji in emojilist) {
                    try {
                        await e.Message.CreateReactionAsync(emoji);
                    } catch (ArgumentException) {
                        await e.Channel.SendMessageAsync($"I have a reaction for that message set up ({emoji}) but that emoji doesn't exits. Fix your shit pls.");
                    }
                    await Task.Delay(500);
                }
            }
        }

        private async Task Client_MessageUpdated(MessageUpdateEventArgs e)
        {
            // Check if message contains filter
            if (!e.Author.IsBot && e.Message.Content != null && e.Message.Content.Split(' ').Any(s => _dependecies.FilterControl.Contains(e.Guild.Id, s))) {
                try {
                    await e.Channel.DeleteMessageAsync(e.Message);
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
            LogHandle.Log(LogLevel.Info, "Client ready.");
            await _client.UpdateStatusAsync(new DiscordGame(_dependecies.StatusControl.GetRandomStatus()) { StreamType = GameStreamType.NoStream });
        }
        #endregion

        #region COMMAND_EVENTS
        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            LogHandle.Log(LogLevel.Info,
                $" Executed: {e.Command?.QualifiedName ?? "<unknown command>"}" + Environment.NewLine +
                $" User: {e.Context.User.ToString()}" + Environment.NewLine +
                $" Location: '{e.Context.Guild.Name}' ({e.Context.Guild.Id}) ; {e.Context.Channel.ToString()}"
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

            LogHandle.Log(LogLevel.Error,
                $" Tried executing: {e.Command?.QualifiedName ?? "<unknown command>"}" + Environment.NewLine +
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
            else if (e.Exception is NotSupportedException)
                embed.Description = $"{emoji} Not supported. {e.Exception.Message}";
            else if (e.Exception is InvalidOperationException)
                embed.Description = $"{emoji} Invalid operation. {e.Exception.Message}";
            else if (e.Exception is InvalidCommandUsageException)
                embed.Description = $"{emoji} Invalid usage! {ex.Message}";
            else if (e.Exception is ArgumentException)
                embed.Description = $"{emoji} Wrong argument format (please use {Formatter.Bold("!help <command>")}.";
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
                else
                    embed.Description = $"{emoji} That command is reserved for the bot owner only!";
            } else if (e.Exception is UnauthorizedException)
                embed.Description = $"{emoji} I am not authorized to do that.";
            else
                embed.Description = $"{emoji} Unknown error occured (probably because a Serbian made this bot). Please **!report**.";

            await e.Context.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
        }
        #endregion
    }
}