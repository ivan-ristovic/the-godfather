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
using TheGodfather.EventListeners;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
        internal SharedData Shared { get; }
        internal DBService Database { get; }
        #endregion


        public TheGodfatherShard(int sid, DBService db, SharedData shared)
        {
            ShardId = sid;
            Database = db;
            Shared = shared;
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
                Token = Shared.BotConfiguration.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LargeThreshold = 250,
                ShardCount = Shared.BotConfiguration.ShardCount,
                ShardId = ShardId,
                UseInternalLogHandler = false,
                LogLevel = TheGodfather.LogHandle.LogLevel
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version <= new Version(6, 1, 7601, 65536))
                cfg.WebSocketClientFactory = WebSocket4NetClient.CreateNew;

            Client = new DiscordClient(cfg);

            Client.DebugLogger.LogMessageReceived += (s, e) => TheGodfather.LogHandle.LogMessage(ShardId, e);

            AsyncExecutionManager.InstallListeners(Client, this);
        }

        private void SetupCommands()
        {
            Commands = Client.UseCommandsNext(new CommandsNextConfiguration {
                EnableDms = false,
                CaseSensitive = false,
                EnableMentionPrefix = true,
                PrefixResolver = PrefixResolverAsync,
                Services = new ServiceCollection()
                    .AddSingleton(new YoutubeService(Shared.BotConfiguration.YouTubeKey))
                    .AddSingleton(new GiphyService(Shared.BotConfiguration.GiphyKey))
                    .AddSingleton(new ImgurService(Shared.BotConfiguration.ImgurKey))
                    .AddSingleton(new OMDbService(Shared.BotConfiguration.OMDbKey))
                    .AddSingleton(new SteamService(Shared.BotConfiguration.SteamKey))
                    .AddSingleton(new WeatherService(Shared.BotConfiguration.WeatherKey))
                    .AddSingleton(this)
                    .AddSingleton(Database)
                    .AddSingleton(Shared)
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
            string p = Shared.GetGuildPrefix(m.Channel.Guild.Id) ?? Shared.BotConfiguration.DefaultPrefix;
            return Task.FromResult(m.GetStringPrefixLength(p));
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

            if (Shared.GuildConfigurations[e.Context.Guild.Id].SuggestionsEnabled && ex is CommandNotFoundException cne) {
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
    }
}