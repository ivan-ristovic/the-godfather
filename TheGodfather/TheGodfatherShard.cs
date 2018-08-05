#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Lavalink;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Converters;
using TheGodfather.EventListeners;
using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather
{
    internal sealed class TheGodfatherShard
    {
        public static IReadOnlyList<(string, Command)> Commands;

        public int Id { get; }
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension CNext { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public VoiceNextExtension Voice { get; private set; }
        public SharedData SharedData { get; private set; }
        public DBService DatabaseService { get; private set; }

        public bool IsListening => this.SharedData.ListeningStatus;


        public TheGodfatherShard(int sid, DBService db, SharedData shared)
        {
            this.Id = sid;
            this.DatabaseService = db;
            this.SharedData = shared;
        }


        public async Task StartAsync()
            => await this.Client.ConnectAsync().ConfigureAwait(false);

        public async Task DisposeAsync()
        {
            await this.Client.DisconnectAsync();
            this.Client.Dispose();
        }

        public void Log(LogLevel level, string message)
            => this.SharedData.LogProvider.LogMessage(level, message, this.Id, DateTime.Now);


        #region SETUP_FUNCTIONS
        public void Initialize()
        {
            SetupClient();
            SetupCommands();
            SetupInteractivity();
            SetupVoice();

            AsyncExecutionManager.RegisterEventListeners(this.Client, this);
        }

        private void SetupClient()
        {
            var cfg = new DiscordConfiguration {
                Token = this.SharedData.BotConfiguration.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LargeThreshold = 250,
                ShardCount = this.SharedData.BotConfiguration.ShardCount,
                ShardId = this.Id,
                UseInternalLogHandler = false,
                LogLevel = this.SharedData.BotConfiguration.LogLevel
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version <= new Version(6, 1, 7601, 65536))
                cfg.WebSocketClientFactory = WebSocket4NetClient.CreateNew;

            this.Client = new DiscordClient(cfg);

            this.Client.DebugLogger.LogMessageReceived += (s, e) => {
                this.SharedData.LogProvider.LogMessage(this.Id, e);
            };
            this.Client.Ready += e => {
                this.SharedData.LogProvider.ElevatedLog(LogLevel.Info, "| Ready!", this.Id);
                return Task.CompletedTask;
            };
        }

        private void SetupCommands()
        {
            this.CNext = this.Client.UseCommandsNext(new CommandsNextConfiguration {
                EnableDms = false,
                CaseSensitive = false,
                EnableMentionPrefix = true,
                PrefixResolver = this.PrefixResolverAsync,
                Services = new ServiceCollection()
                    .AddSingleton(this)
                    .AddSingleton(this.SharedData)
                    .AddSingleton(this.DatabaseService)
                    .AddSingleton(new GiphyService(this.SharedData.BotConfiguration.GiphyKey))
                    .AddSingleton(new GoodreadsService(this.SharedData.BotConfiguration.GoodreadsKey))
                    .AddSingleton(new ImgurService(this.SharedData.BotConfiguration.ImgurKey))
                    .AddSingleton(new OMDbService(this.SharedData.BotConfiguration.OMDbKey))
                    .AddSingleton(new SteamService(this.SharedData.BotConfiguration.SteamKey))
                    .AddSingleton(new WeatherService(this.SharedData.BotConfiguration.WeatherKey))
                    .AddSingleton(new YtService(this.SharedData.BotConfiguration.YouTubeKey))
                    .BuildServiceProvider()
            });
            this.CNext.SetHelpFormatter<CustomHelpFormatter>();
            this.CNext.RegisterCommands(Assembly.GetExecutingAssembly());
            this.CNext.RegisterConverter(new CustomActivityTypeConverter());
            this.CNext.RegisterConverter(new CustomBoolConverter());
            this.CNext.RegisterConverter(new CustomTimeWindowConverter());
            this.CNext.RegisterConverter(new CustomIPAddressConverter());
            this.CNext.RegisterConverter(new CustomIPFormatConverter());
            
            Commands = this.CNext.GetAllRegisteredCommands()
                .Where(cmd => cmd.Parent == null)
                .SelectMany(cmd => cmd.Aliases.Select(alias => (alias, cmd)).Concat(new[] { (cmd.Name, cmd) }))
                .ToList()
                .AsReadOnly();
        }

        private void SetupInteractivity()
        {
            this.Interactivity = this.Client.UseInteractivity(new InteractivityConfiguration() {
                PaginationBehavior = TimeoutBehaviour.DeleteReactions,
                PaginationTimeout = TimeSpan.FromMinutes(1),
                Timeout = TimeSpan.FromMinutes(1)
            });
        }

        private void SetupVoice()
        {
            this.Voice = this.Client.UseVoiceNext();
        }

        private Task<int> PrefixResolverAsync(DiscordMessage m)
        {
            string p = this.SharedData.GetGuildPrefix(m.Channel.Guild.Id) ?? this.SharedData.BotConfiguration.DefaultPrefix;
            return Task.FromResult(m.GetStringPrefixLength(p));
        }
        #endregion
    }
}