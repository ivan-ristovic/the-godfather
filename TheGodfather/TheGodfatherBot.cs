using System.Reflection;
using System.Threading;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Lavalink4NET.InactivityTracking;
using Lavalink4NET.InactivityTracking.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Extensions.Logging;
using TheGodfather.EventListeners;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Services.Common;
using TheGodfather.Services.Extensions;

namespace TheGodfather;

public sealed class TheGodfatherBot
{
    public BotConfigService Config { get; }
    public DbContextBuilder Database { get; }
    public ServiceProvider Services => this.services ?? throw new BotUninitializedException();
    public DiscordShardedClient Client => this.client ?? throw new BotUninitializedException();
    public IReadOnlyDictionary<int, InteractivityExtension> Interactivity => this.interactivity ?? throw new BotUninitializedException();
    public IReadOnlyDictionary<int, CommandsNextExtension> CNext => this.cnext ?? throw new BotUninitializedException();
    public IReadOnlyDictionary<int, VoiceNextExtension> VNext => this.vnext ?? throw new BotUninitializedException();
    public IReadOnlyDictionary<string, Command> Commands => this.commands ?? throw new BotUninitializedException();

    private DiscordShardedClient? client;
    private ServiceProvider? services;
    private IReadOnlyDictionary<int, InteractivityExtension>? interactivity;
    private IReadOnlyDictionary<int, CommandsNextExtension>? cnext;
    private IReadOnlyDictionary<int, VoiceNextExtension>? vnext;
    private IReadOnlyDictionary<string, Command>? commands;


    public TheGodfatherBot(BotConfigService cfg, DbContextBuilder dbb)
    {
        this.Config = cfg;
        this.Database = dbb;
    }

    public async Task DisposeAsync()
    {
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
        await this.SetupLavalinkAsync();
        this.vnext = await this.client.UseVoiceNextAsync(new VoiceNextConfiguration());

        Listeners.FindAndRegister(this);

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
            LargeThreshold = 500,
            ShardCount = this.Config.CurrentConfiguration.ShardCount,
            LoggerFactory = new SerilogLoggerFactory(dispose: true),
            Intents = DiscordIntents.All
                .RemoveIntent(DiscordIntents.GuildMessageTyping)
                .RemoveIntent(DiscordIntents.DirectMessageTyping)
        };

        var cl = new DiscordShardedClient(cfg);
        cl.Ready += (s, _) => {
            LogExt.Information(s.ShardId, "Client ready!");
            return Task.CompletedTask;
        };

        return cl;
    }

    private ServiceProvider SetupServices()
    {
        Log.Information("Initializing services...");
        var sc = new ServiceCollection()
            .AddSingleton(this.Config)
            .AddSingleton(this.Database)
            .AddSingleton(this.Client)
            .AddSharedServices();

        LavalinkConfig lavaConfig = this.Config.CurrentConfiguration.LavalinkConfig;
        if (lavaConfig.Enable) {
            sc.AddLavalink();
            sc.AddInactivityTracking();
            sc.ConfigureLavalink(cfg => {
                cfg.Label = lavaConfig.Password;
                cfg.Passphrase = lavaConfig.Password;
                cfg.ReadyTimeout = TimeSpan.FromSeconds(lavaConfig.ReadyTimeout);
                cfg.ResumptionOptions = new LavalinkSessionResumptionOptions(
                    TimeSpan.FromSeconds(lavaConfig.ResumptionTimeout)
                );
                cfg.BaseAddress = new Uri($"http://{lavaConfig.Hostname}:{lavaConfig.Port}");
            });
        }
        
        return sc
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
                    : this.Services.GetRequiredService<GuildConfigService>().GetGuildPrefix(m.Channel.Guild.Id);
                return Task.FromResult(m.GetStringPrefixLength(p));
            },
            Services = this.Services
        };
        IReadOnlyDictionary<int, CommandsNextExtension> cnextExtension = await this.Client.UseCommandsNextAsync(cfg);

        Log.Debug("Registering commands...");
        var assembly = Assembly.GetExecutingAssembly();
        foreach ((int _, CommandsNextExtension cne) in cnextExtension) {
            cne.RegisterCommands(assembly);
            cne.RegisterConverters(assembly);
            cne.SetHelpFormatter<LocalizedHelpFormatter>();
        }

        Log.Debug("Checking command translations...");
        LocalizationService lcs = this.Services.GetRequiredService<LocalizationService>();
        CommandService cs = this.Services.GetRequiredService<CommandService>();
        if (cs.TranslationsPresentForRegisteredCommands(lcs, cnextExtension.Values.First().GetRegisteredCommands()))
            Log.Debug("Found translations for all commands and arguments");
        else
            Log.Error("Failed to find translations for some commands/arguments");

        return cnextExtension;
    }

    private Task<IReadOnlyDictionary<int, InteractivityExtension>> SetupInteractivityAsync()
    {
        return this.Client.UseInteractivityAsync(new InteractivityConfiguration {
            PaginationBehaviour = PaginationBehaviour.WrapAround,
            PaginationDeletion = PaginationDeletion.KeepEmojis,
            PaginationEmojis = new PaginationEmojis(),
            PollBehaviour = PollBehaviour.KeepEmojis,
            Timeout = TimeSpan.FromMinutes(1)
        });
    }
    
    private async Task SetupLavalinkAsync()
    {
        if (this.services is null || !this.Config.CurrentConfiguration.LavalinkConfig.Enable)
            return;

        await this.services.GetRequiredService<IHostedService>().StartAsync(CancellationToken.None);
        await this.services.GetRequiredService<IAudioService>().StartAsync();
        await this.services.GetRequiredService<IInactivityTrackingService>().StartAsync();
    }

}