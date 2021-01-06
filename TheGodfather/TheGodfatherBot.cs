using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using TheGodfather.Database;
using TheGodfather.EventListeners;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Services;

namespace TheGodfather
{
    public sealed class TheGodfatherBot
    {
        public ServiceProvider Services => this.services ?? throw new BotUninitializedException();
        public BotConfigService Config => this.config ?? throw new BotUninitializedException();
        public DbContextBuilder Database => this.database ?? throw new BotUninitializedException();
        public DiscordShardedClient Client => this.client ?? throw new BotUninitializedException();
        public IReadOnlyDictionary<int, InteractivityExtension> Interactivity => this.interactivity ?? throw new BotUninitializedException();
        public IReadOnlyDictionary<int, CommandsNextExtension> CNext => this.cnext ?? throw new BotUninitializedException();
        public IReadOnlyDictionary<string, Command> Commands => this.commands ?? throw new BotUninitializedException();

        private readonly BotConfigService? config;
        private readonly DbContextBuilder? database;
        private DiscordShardedClient? client;
        private ServiceProvider? services;
        private IReadOnlyDictionary<int, InteractivityExtension>? interactivity;
        private IReadOnlyDictionary<int, CommandsNextExtension>? cnext;
        private IReadOnlyDictionary<string, Command>? commands;


        public TheGodfatherBot(BotConfigService cfg, DbContextBuilder dbb)
        {
            this.config = cfg;
            this.database = dbb;
        }

        public async Task DisposeAsync()
        {
            if (this.Client is { })
                await this.Client.StopAsync();
            await this.Services.DisposeAsync();
        }


        public int GetId(ulong? gid)
            => gid is null ? 0 : this.Client.GetShard(gid.Value).ShardId;

        public async Task StartAsync()
        {
            Log.Information("Initializing the bot...");
            
            this.client = this.SetupClient();

            this.services = this.SetupServices();
            this.cnext = await this.SetupCommandsAsync();
            this.UpdateCommandList();

            this.interactivity = await this.SetupInteractivityAsync();

            Listeners.FindAndRegister(this);

            Log.Information("Starting {ShardCount} shard(s)", this.Config.CurrentConfiguration.ShardCount);
            await this.Client.StartAsync();
        }

        public void UpdateCommandList()
        {
            this.commands = this.CNext.First().Value.GetRegisteredCommands()
                .Where(cmd => cmd.Parent is null)
                .SelectMany(cmd => cmd.Aliases.Select(alias => (alias, cmd)).Concat(new[] { (cmd.Name, cmd) }))
                .ToDictionary(tup => tup.Item1, tup => tup.cmd);
        }


        private DiscordShardedClient SetupClient()
        {
            var cfg = new DiscordConfiguration {
                Token = this.Config.CurrentConfiguration.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LargeThreshold = 250,
                ShardCount = this.Config.CurrentConfiguration.ShardCount,
                LoggerFactory = new SerilogLoggerFactory(dispose: true),
                Intents = DiscordIntents.All
                       & ~DiscordIntents.GuildMessageTyping
                       & ~DiscordIntents.DirectMessageTyping
            };

            var client = new DiscordShardedClient(cfg);
            client.Ready += (s, e) => {
                LogExt.Information(s.ShardId, "Client ready!");
                return Task.CompletedTask;
            };

            return client;
        }

        private ServiceProvider SetupServices()
        {
            Log.Information("Initializing services");
            return new ServiceCollection()
                .AddSingleton(this.Config)
                .AddSingleton(this.Database)
                .AddSingleton(this.Client)
                .AddSharedServices()
                .BuildServiceProvider()
                .Initialize()
                ;
        }

        private async Task<IReadOnlyDictionary<int, CommandsNextExtension>> SetupCommandsAsync()
        {
            var cfg = new CommandsNextConfiguration {
                CaseSensitive = false,
                EnableDefaultHelp = false,
                EnableDms = true,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = false,
                PrefixResolver = m => {
                    string p = m.Channel.Guild is null
                        ? this.Config.CurrentConfiguration.Prefix
                        : this.Services.GetRequiredService<GuildConfigService>().GetGuildPrefix(m.Channel.Guild.Id) ?? this.Config.CurrentConfiguration.Prefix;
                    return Task.FromResult(m.GetStringPrefixLength(p));
                },
                Services = this.Services
            };
            IReadOnlyDictionary<int, CommandsNextExtension> cnext = await this.Client.UseCommandsNextAsync(cfg);

            Log.Debug("Registering commands...");
            var assembly = Assembly.GetExecutingAssembly();
            foreach ((int shardId, CommandsNextExtension cne) in cnext) {
                cne.RegisterCommands(assembly);
                cne.RegisterConverters(assembly);
                cne.SetHelpFormatter<LocalizedHelpFormatter>();
            }

            Log.Debug("Checking command translations...");
            LocalizationService lcs = this.Services.GetRequiredService<LocalizationService>();
            foreach (Command cmd in cnext.Values.First().GetRegisteredCommands()) {
                try {
                    _ = lcs.GetCommandDescription(0, cmd.QualifiedName);
                    IEnumerable<CommandArgument> args = cmd.Overloads.SelectMany(o => o.Arguments).Distinct();
                    foreach (CommandArgument arg in args)
                        _ = lcs.GetString(null, arg.Description);
                } catch (LocalizationException e) {
                    Log.Warning(e, "Translation not found");
                }
            }

            return cnext;
        }

        private Task<IReadOnlyDictionary<int, InteractivityExtension>> SetupInteractivityAsync()
        {
            return this.Client.UseInteractivityAsync(new InteractivityConfiguration {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteEmojis,
                PaginationEmojis = new PaginationEmojis(),
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromMinutes(1)
            });
        }
    }
}