using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Common.Converters;
using TheGodfather.Database;
using TheGodfather.EventListeners;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather
{
    public sealed class TheGodfatherShard
    {
        public static IReadOnlyDictionary<string, Command> Commands => _commands;

        private static ConcurrentDictionary<string, Command> _commands = new ConcurrentDictionary<string, Command>();

        public static void UpdateCommandList(CommandsNextExtension cnext)
        {
            _commands = new ConcurrentDictionary<string, Command>(
                cnext.GetAllRegisteredCommands()
                    .Where(cmd => cmd.Parent is null)
                    .SelectMany(cmd => cmd.Aliases.Select(alias => (Name: alias, Command: cmd)).Concat(new[] { (cmd.Name, Command: cmd) }))
                    .ToDictionary(tup => tup.Name, tup => tup.Command)
            );
        }


        public int Id { get; }
        public ServiceProvider Services { get; private set; }
        public BotConfig Config { get; private set; }
        public DatabaseContextBuilder Database { get; private set; }
        public DiscordClient? Client { get; private set; }
        public CommandsNextExtension? CNext { get; private set; }
        public InteractivityExtension? Interactivity { get; private set; }
        public VoiceNextExtension? Voice { get; private set; }


        public TheGodfatherShard(int shardId, IServiceCollection services)
        {
            this.Id = shardId;
            this.Services = BotServiceCollectionProvider.AddShardSpecificServices(services, this).BuildServiceProvider();
            this.Database = this.Services.GetService<DatabaseContextBuilder>();
            this.Config = this.Services.GetService<BotConfigService>().CurrentConfiguration;
        }

        public async Task DisposeAsync()
        {
            if (this.Client is { }) {
                await this.Client.DisconnectAsync();
                this.Client.Dispose();
            }
        }


        public async Task StartAsync()
        {
            if (this.Client is null)
                throw new InvalidOperationException("Shard needs to be initialized before it could be started.");
            await this.Client.ConnectAsync();
        }

        public void Initialize(AsyncEventHandler<GuildDownloadCompletedEventArgs> onGuildDownloadCompleted)
        {
            this.SetupClient(onGuildDownloadCompleted);
            this.SetupCommands();
            this.SetupInteractivity();
            this.SetupVoice();

            Listeners.FindAndRegister(this.Client!, this);
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

            this.CNext.RegisterConverter(new ActivityTypeConverter());
            this.CNext.RegisterConverter(new BoolConverter());
            this.CNext.RegisterConverter(new ImgurTimeWindowConverter());
            this.CNext.RegisterConverter(new IPAddressConverter());
            this.CNext.RegisterConverter(new IPAddressRangeConverter());
            this.CNext.RegisterConverter(new PunishmentActionConverter());

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