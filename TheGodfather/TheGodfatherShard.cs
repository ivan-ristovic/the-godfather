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
using Serilog;
using TheGodfather.Common;
using TheGodfather.Common.Converters;
using TheGodfather.Database;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;

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
        public ServiceProvider Services { get; set; }
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension CNext { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public VoiceNextExtension Voice { get; private set; }
        public DatabaseContextBuilder Database { get; private set; }
        public BotConfig Config { get; private set; }
        #endregion


        public TheGodfatherShard(BotConfig cfg, int shardId, DatabaseContextBuilder databaseBuilder)
        {
            this.Id = shardId;
            this.Database = databaseBuilder;
            this.Config = cfg;
        }

        public async Task DisposeAsync()
        {
            await this.Client.DisconnectAsync();
            this.Client.Dispose();
        }


        public async Task StartAsync()
            => await this.Client.ConnectAsync();

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
                Token = this.Config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LargeThreshold = 250,
                ShardCount = this.Config.ShardCount,
                ShardId = this.Id,
                UseInternalLogHandler = false,
                LogLevel = LogLevel.Debug
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version <= new Version(6, 1, 7601, 65536))
                cfg.WebSocketClientFactory = WebSocket4NetCoreClient.CreateNew;

            this.Client = new DiscordClient(cfg);

            this.Client.DebugLogger.LogMessageReceived += LogExt.Event;
            this.Client.Ready += e => {
                Log.Information("Client ready!");
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
                PrefixResolver = m => {
                    string p = this.Services.GetService<GuildConfigService>().GetGuildPrefix(m.Channel.Guild.Id) ?? this.Config.Prefix;
                    return Task.FromResult(m.GetStringPrefixLength(p));
                },
                Services = this.Services
            });

            this.CNext.SetHelpFormatter<CustomHelpFormatter>();

            this.CNext.RegisterCommands(Assembly.GetExecutingAssembly());

            // TODO load these like event listeners
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
    }
}