using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Extensions.Logging;
using TheGodfather.EventListeners;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Services.Extensions;

namespace TheGodfather;

public sealed class TheGodfatherBot
{
    public ServiceProvider Services => this.services ?? throw new BotUninitializedException();
    public BotConfigService Config => this.config ?? throw new BotUninitializedException();
    public DbContextBuilder Database => this.database ?? throw new BotUninitializedException();
    public DiscordShardedClient Client => this.client ?? throw new BotUninitializedException();
    public IReadOnlyDictionary<int, InteractivityExtension> Interactivity => this.interactivity ?? throw new BotUninitializedException();
    public IReadOnlyDictionary<int, CommandsNextExtension> CNext => this.cnext ?? throw new BotUninitializedException();
    public IReadOnlyDictionary<int, VoiceNextExtension> VNext => this.vnext ?? throw new BotUninitializedException();
    public IReadOnlyDictionary<int, LavalinkExtension> Lavalink => this.lavalink ?? throw new BotUninitializedException();
    public IReadOnlyDictionary<string, Command> Commands => this.commands ?? throw new BotUninitializedException();

    private readonly BotConfigService? config;
    private readonly DbContextBuilder? database;
    private DiscordShardedClient? client;
    private ServiceProvider? services;
    private IReadOnlyDictionary<int, InteractivityExtension>? interactivity;
    private IReadOnlyDictionary<int, CommandsNextExtension>? cnext;
    private IReadOnlyDictionary<int, VoiceNextExtension>? vnext;
    private IReadOnlyDictionary<int, LavalinkExtension>? lavalink;
    private IReadOnlyDictionary<string, Command>? commands;


    public TheGodfatherBot(BotConfigService cfg, DbContextBuilder dbb)
    {
        this.config = cfg;
        this.database = dbb;
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
        this.lavalink = await this.client.UseLavalinkAsync();
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
                .RemoveIntent(DiscordIntents.DirectMessageTyping),
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
            EnableMentionPrefix = false,
            IgnoreExtraArguments = false,
            PrefixResolver = m => {
                string p = m.Channel.Guild is null
                    ? this.Config.CurrentConfiguration.Prefix
                    : this.Services.GetRequiredService<GuildConfigService>().GetGuildPrefix(m.Channel.Guild.Id);
                return Task.FromResult(m.GetStringPrefixLength(p));
            },
            Services = this.Services,
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
}