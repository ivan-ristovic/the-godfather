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
        public static List<(string, Command)> CommandNames { get; private set; }

        public bool IsListening => Shared.ListeningStatus;
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

            AsyncExecutionManager.RegisterEventListeners(Client, this);
        }

        public async Task DisconnectAndDisposeAsync()
        {
            await Client.DisconnectAsync();
            Client.Dispose();
        }

        public async Task StartAsync()
            => await Client.ConnectAsync().ConfigureAwait(false);

        public void Log(LogLevel level, string message)
            => Shared.LogProvider.LogMessage(level, message, ShardId, DateTime.Now);


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
                LogLevel = Shared.BotConfiguration.LogLevel
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version <= new Version(6, 1, 7601, 65536))
                cfg.WebSocketClientFactory = WebSocket4NetClient.CreateNew;

            Client = new DiscordClient(cfg);

            Client.DebugLogger.LogMessageReceived += (s, e) => Shared.LogProvider.LogMessage(ShardId, e);
            Client.Ready += e => { Shared.LogProvider.ElevatedLog(LogLevel.Info, "Ready!", ShardId); return Task.CompletedTask; };
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
    }
}