using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Common.Converters;
using TheGodfather.Database;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Reactions.Services;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;

namespace TheGodfather
{
    public sealed class TheGodfatherShard
    {
        #region Static 
        public static IReadOnlyDictionary<string, Command> Commands => _commands;
        private static ConcurrentDictionary<string, Command> _commands;

        public static void UpdateCommandList(CommandsNextExtension cnext)
        {
            _commands = new ConcurrentDictionary<string, Command>(
                cnext.GetAllRegisteredCommands()
                .Where(cmd => cmd.Parent is null)
                .SelectMany(cmd => cmd.Aliases.Select(alias => (Name: alias, Command: cmd)).Concat(new[] { (cmd.Name, Command: cmd) }))
                .ToDictionary(tup => tup.Name, tup => tup.Command)
            );
        }
        #endregion

        #region Public Properties
        public int Id { get; }
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension CNext { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public VoiceNextExtension Voice { get; private set; }
        public SharedData SharedData { get; private set; }
        public DatabaseContextBuilder Database { get; private set; }
        public ServiceProvider Services { get; }

        public bool IsListening => this.SharedData.IsBotListening;
        #endregion


        public TheGodfatherShard(int sid, DatabaseContextBuilder dbb, SharedData shared)
        {
            this.Id = sid;
            this.Database = dbb;
            this.SharedData = shared;
            this.Services = new ServiceCollection()
                .AddSingleton(this)
                .AddSingleton(this.SharedData)
                .AddSingleton(this.Database)
                .AddSingleton(new AntifloodService(this))
                .AddSingleton(new AntiInstantLeaveService(this))
                .AddSingleton(new AntispamService(this))
                .AddSingleton(new ChannelEventService())
                .AddSingleton(new FilteringService(this.Database, this.SharedData.LogProvider))
                .AddSingleton(new GiphyService(this.SharedData.BotConfiguration.GiphyKey))
                .AddSingleton(new GoodreadsService(this.SharedData.BotConfiguration.GoodreadsKey))
                .AddSingleton(new ImgurService(this.SharedData.BotConfiguration.ImgurKey))
                .AddSingleton(new LinkfilterService(this))
                .AddSingleton(new OMDbService(this.SharedData.BotConfiguration.OMDbKey))
                .AddSingleton(new RatelimitService(this))
                .AddSingleton(new ReactionsService(this.Database, this.SharedData.LogProvider))
                .AddSingleton(new SteamService(this.SharedData.BotConfiguration.SteamKey))
                .AddSingleton(new UserRanksService())
                .AddSingleton(new WeatherService(this.SharedData.BotConfiguration.WeatherKey))
                .AddSingleton(new YtService(this.SharedData.BotConfiguration.YouTubeKey))
                .BuildServiceProvider();
        }

        public async Task DisposeAsync()
        {
            await this.Client.DisconnectAsync();
            this.Client.Dispose();
        }


        #region Setup
        public void Initialize(AsyncEventHandler<GuildDownloadCompletedEventArgs> onGuildDownloadCompleted)
        {
            this.SetupClient(onGuildDownloadCompleted);
            this.SetupCommands();
            this.SetupInteractivity();
            this.SetupVoice();

            AsyncExecutionManager.RegisterEventListeners(this.Client, this);
        }

        private void SetupClient(AsyncEventHandler<GuildDownloadCompletedEventArgs> onGuildDownloadCompleted)
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
                cfg.WebSocketClientFactory = WebSocket4NetCoreClient.CreateNew;

            this.Client = new DiscordClient(cfg);

            this.Client.DebugLogger.LogMessageReceived += (s, e) => {
                this.SharedData.LogProvider.Log(this.Id, e);
            };
            this.Client.Ready += e => {
                this.SharedData.LogProvider.ElevatedLog(LogLevel.Info, "Ready!", this.Id);
                return Task.CompletedTask;
            };
            this.Client.GuildDownloadCompleted += onGuildDownloadCompleted;
        }

        private void SetupCommands()
        {
            this.CNext = this.Client.UseCommandsNext(new CommandsNextConfiguration {
                EnableDms = false,
                CaseSensitive = false,
                EnableMentionPrefix = true,
                PrefixResolver = this.PrefixResolverAsync,
                Services = this.Services
            });

            this.CNext.SetHelpFormatter<CustomHelpFormatter>();

            this.CNext.RegisterCommands(Assembly.GetExecutingAssembly());

            this.CNext.RegisterConverter(new CustomActivityTypeConverter());
            this.CNext.RegisterConverter(new CustomBoolConverter());
            this.CNext.RegisterConverter(new CustomTimeWindowConverter());
            this.CNext.RegisterConverter(new CustomIPAddressConverter());
            this.CNext.RegisterConverter(new CustomIPFormatConverter());
            this.CNext.RegisterConverter(new CustomPunishmentActionTypeConverter());

            UpdateCommandList(this.CNext);
        }

        private void SetupInteractivity()
        {
            this.Interactivity = this.Client.UseInteractivity(new InteractivityConfiguration {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteEmojis,
                PaginationEmojis = new PaginationEmojis(),
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromMinutes(1)
            });
        }

        private void SetupVoice()
        {
            this.Voice = this.Client.UseVoiceNext();
        }

        public async Task StartAsync()
            => await this.Client.ConnectAsync();

        #endregion

        #region Misc
        public void Log(LogLevel level, string message)
            => this.SharedData.LogProvider.Log(level, message, this.Id, DateTime.Now);

        public void LogMany(LogLevel level, params string[] messages)
            => this.SharedData.LogProvider.LogMany(level, this.Id, DateTime.Now, this.SharedData.LogProvider.LogToFile, messages);

        private Task<int> PrefixResolverAsync(DiscordMessage m)
        {
            string p = this.SharedData.GetGuildPrefix(m.Channel.Guild.Id) ?? this.SharedData.BotConfiguration.DefaultPrefix;
            return Task.FromResult(m.GetStringPrefixLength(p));
        }
        #endregion
    }
}