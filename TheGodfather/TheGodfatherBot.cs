using System.Text;
using Discord.Commands;
using System.Net;
using TheGodfather.Attributes;
using TheGodfather.Common.TypeReaders;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Chickens.Services;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Modules.Games.Services;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Modules.Misc.Services;
using TheGodfather.Modules.Owner.Services;
using TheGodfather.Modules.Reactions.Services;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather;

public sealed class TheGodfatherBot
{
    public ServiceProvider Services => this.services ?? throw new BotUninitializedException();
    public BotConfigService Config => this.config;
    public DbContextBuilder Database => this.database;
    public DiscordShardedClient Client => this.client ?? throw new BotUninitializedException();
    public CommandService Commands => this.commands ?? throw new BotUninitializedException();

    private readonly BotConfigService config;
    private readonly DbContextBuilder database;
    private DiscordShardedClient? client;
    private CommandService? commands;
    private ServiceProvider? services;


    public TheGodfatherBot(BotConfigService cfg, DbContextBuilder dbb)
    {
        this.config = cfg;
        this.database = dbb;
    }

    public async Task StartAsync()
    {
        Log.Information("Initializing Discord.Net bot...");

        this.client = this.SetupClient();
        this.commands = this.SetupCommands();
        this.services = this.SetupServices();

        await this.Commands.AddModulesAsync(Assembly.GetExecutingAssembly(), this.Services);
        _ = this.Services.GetRequiredService<SchedulingService>();
        _ = this.Services.GetRequiredService<PeriodicTasksService>();

        this.Client.MessageReceived += this.HandleMessageAsync;
        this.Client.UserJoined += this.HandleUserJoinedAsync;
        this.Client.UserLeft += this.HandleUserLeftAsync;
        this.Client.GuildMemberUpdated += this.HandleGuildMemberUpdatedAsync;
        this.Client.ReactionAdded += this.HandleReactionAddedAsync;
        this.Client.ReactionRemoved += this.HandleReactionRemovedAsync;
        this.Client.ReactionsCleared += this.HandleReactionsClearedAsync;
        this.Client.ReactionsRemovedForEmote += this.HandleReactionsRemovedForEmoteAsync;
        this.Client.JoinedGuild += this.HandleJoinedGuildAsync;
        this.Client.LeftGuild += this.HandleLeftGuildAsync;
        this.Commands.CommandExecuted += this.HandleCommandExecutedAsync;

        string? token = this.Config.CurrentConfiguration.Token;
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Discord token is missing from the Discord.Net config file.");

        await this.Client.LoginAsync(TokenType.Bot, token);
        await this.Client.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (this.client is not null) {
            await this.client.StopAsync();
            await this.client.LogoutAsync();
            this.client.Dispose();
        }

        if (this.services is not null)
            await this.services.DisposeAsync();
    }


    private DiscordShardedClient SetupClient()
    {
        int shardCount = Math.Max(1, this.Config.CurrentConfiguration.ShardCount);
        var cfg = new DiscordSocketConfig {
            AlwaysDownloadUsers = true,
            GatewayIntents = GatewayIntents.AllUnprivileged
                             | GatewayIntents.GuildMembers
                             | GatewayIntents.GuildPresences
                             | GatewayIntents.MessageContent,
            LogGatewayIntentWarnings = true,
            TotalShards = shardCount
        };

        var cl = new DiscordShardedClient(cfg);
        cl.Log += LogDiscordNetAsync;
        cl.ShardReady += async shard => {
            Log.Information("Shard {ShardId} ready", shard.ShardId);
            if (this.services is null)
                return;

            GuildConfigService gcs = this.services.GetRequiredService<GuildConfigService>();
            foreach (SocketGuild guild in shard.Guilds)
                if (!gcs.IsGuildRegistered(guild.Id))
                    await gcs.RegisterGuildAsync(guild.Id);
        };
        return cl;
    }

    private CommandService SetupCommands()
    {
        var cfg = new CommandServiceConfig {
            CaseSensitiveCommands = false,
            DefaultRunMode = RunMode.Async,
            IgnoreExtraArgs = false,
            LogLevel = LogSeverity.Debug
        };

        var cs = new CommandService(cfg);
        cs.AddTypeReader<bool>(new BoolTypeReader());
        cs.AddTypeReader<ActivityType>(new ActivityTypeReader());
        cs.AddTypeReader<Filter.Action>(new FilterActionTypeReader());
        cs.AddTypeReader<IPAddress>(new IPAddressTypeReader());
        cs.AddTypeReader<LogEventLevel>(new LogEventLevelTypeReader());
        cs.AddTypeReader<ModuleType>(new ModuleTypeTypeReader());
        cs.AddTypeReader<Punishment.Action>(new PunishmentActionTypeReader());
        cs.AddTypeReader<TimeWindow>(new TimeWindowTypeReader());
        cs.AddTypeReader<Uri>(new UriTypeReader());
        cs.Log += LogDiscordNetAsync;
        return cs;
    }

    private ServiceProvider SetupServices()
    {
        Log.Information("Initializing services...");
        return new ServiceCollection()
            .AddSingleton(this)
            .AddSingleton(this.Config)
            .AddSingleton(this.Database)
            .AddSingleton(this.Config.CurrentConfiguration)
            .AddSingleton(this.Client)
            .AddSingleton(this.Commands)
            .AddSingleton<AsyncExecutionService>()
            .AddSingleton<GuildConfigService>()
            .AddSingleton<ActionHistoryService>()
            .AddSingleton<AntiInstantLeaveService>()
            .AddSingleton<AntiMentionService>()
            .AddSingleton<AntifloodService>()
            .AddSingleton<AntispamService>()
            .AddSingleton<AutoRoleService>()
            .AddSingleton<BackupService>()
            .AddSingleton<CommandRulesService>()
            .AddSingleton<ForbiddenNamesService>()
            .AddSingleton<LevelRoleService>()
            .AddSingleton<LinkfilterService>()
            .AddSingleton<LoggingService>()
            .AddSingleton<ProtectionService>()
            .AddSingleton<RatelimitService>()
            .AddSingleton<SelfRoleService>()
            .AddSingleton<ReactionRoleService>()
            .AddSingleton<FilteringService>()
            .AddSingleton<LocalizationService>()
            .AddSingleton(sp => new InteractivityService(sp.GetRequiredService<DiscordShardedClient>()))
            .AddSingleton<SchedulingService>()
            .AddSingleton<PeriodicTasksService>()
            .AddSingleton<DynamicCommandService>()
            .AddSingleton<FontsService>()
            .AddSingleton<BotActivityService>()
            .AddSingleton<PrivilegedUserService>()
            .AddSingleton<BlockingService>()
            .AddSingleton<RandomService>()
            .AddSingleton<BirthdayService>()
            .AddSingleton<InsultService>()
            .AddSingleton<MemeService>()
            .AddSingleton<MemeGenService>()
            .AddSingleton<ImagingService>()
            .AddSingleton<GuildRanksService>()
            .AddSingleton<UserRanksService>()
            .AddSingleton<StarboardService>()
            .AddSingleton<BankAccountService>()
            .AddSingleton<WorkService>()
            .AddSingleton<ShopService>()
            .AddSingleton<ChickenService>()
            .AddSingleton<ChickenUpgradeService>()
            .AddSingleton<ChickenBoughtUpgradeService>()
            .AddSingleton<GameStatsService>()
            .AddSingleton<QuizService>()
            .AddSingleton<ReactionsService>()
            .AddSingleton<CatFactsService>()
            .AddSingleton<QuoteService>()
            .AddSingleton<WeatherService>()
            .AddSingleton<OMDbService>()
            .AddSingleton<GoodreadsService>()
            .AddSingleton<CryptoCurrencyService>()
            .AddSingleton<SteamService>()
            .AddSingleton<GiphyService>()
            .AddSingleton<ImgurService>()
            .AddSingleton<YtService>()
            .AddSingleton<RssFeedsService>()
            .AddSingleton<RedditService>()
            .BuildServiceProvider();
    }

    private async Task HandleMessageAsync(SocketMessage rawMessage)
    {
        if (rawMessage is not SocketUserMessage message || message.Author.IsBot || message.Source != MessageSource.User)
            return;

        var context = new GodfatherCommandContext(this.Client, message, this.Services);
        GuildConfigService gcs = this.Services.GetRequiredService<GuildConfigService>();
        if (context.Guild is not null && !gcs.IsGuildRegistered(context.Guild.Id))
            await gcs.RegisterGuildAsync(context.Guild.Id);

        BlockingService blocking = this.Services.GetRequiredService<BlockingService>();
        if (blocking.IsBlocked(context.Guild?.Id, context.Channel.Id, context.User.Id))
            return;

        string prefix = context.Guild is null
            ? this.Config.CurrentConfiguration.Prefix
            : gcs.GetGuildPrefix(context.Guild.Id);

        int argPos = 0;
        bool hasPrefix = message.HasStringPrefix(prefix, ref argPos, StringComparison.OrdinalIgnoreCase);

        if (context.Guild is not null)
            await this.HandleGuildMessageServicesAsync(context, message, hasPrefix);

        if (!hasPrefix)
            return;

        if (this.Services.GetRequiredService<InteractivityService>().IsResponsePending(context.Channel.Id, context.User.Id))
            return;

        BotActivityService activity = this.Services.GetRequiredService<BotActivityService>();
        if (!activity.IsBotListening && !await this.IsOwnerAsync(context.User.Id))
            return;

        string commandInput = message.Content[argPos..];
        if (context.Guild is not null && this.IsCommandRuleBlocked(context, commandInput))
            return;

        CommandInfo? resolvedCommand = this.ResolveCommand(commandInput);
        if (resolvedCommand is null &&
            await this.Services.GetRequiredService<DynamicCommandService>().TryExecuteAsync(context, commandInput))
            return;

        if (resolvedCommand is not null)
            Log.Debug(
                "Command: Executing \"{Command}\" for {User} in {Guild}/{Channel}",
                BuildQualifiedCommandName(resolvedCommand),
                context.User,
                context.Guild?.Name ?? "DM",
                context.Channel
            );

        await this.Commands.ExecuteAsync(context, argPos, this.Services);
    }

    private async Task HandleJoinedGuildAsync(SocketGuild guild)
    {
        GuildConfigService gcs = this.Services.GetRequiredService<GuildConfigService>();
        if (!gcs.IsGuildRegistered(guild.Id))
            await gcs.RegisterGuildAsync(guild.Id);
    }

    private Task HandleLeftGuildAsync(SocketGuild guild)
    {
        GuildConfigService gcs = this.Services.GetRequiredService<GuildConfigService>();
        return gcs.UnregisterGuildAsync(guild.Id);
    }

    private async Task HandleUserJoinedAsync(SocketGuildUser member)
    {
        if (!this.Services.GetRequiredService<GuildConfigService>().IsGuildRegistered(member.Guild.Id))
            await this.Services.GetRequiredService<GuildConfigService>().RegisterGuildAsync(member.Guild.Id);

        if (member.IsBot)
            return;

        GuildConfigService gcs = this.Services.GetRequiredService<GuildConfigService>();
        GuildConfig gcfg = await gcs.GetConfigAsync(member.Guild.Id);

        Punishment? loggedPunishment = await this.Services.GetRequiredService<ProtectionService>().GetStrongestLoggedPunishmentAsync(member.Guild.Id, member.Id);
        if (loggedPunishment is not null)
            await this.PunishMemberAsync(member.Guild, member, loggedPunishment.Type, "_gf: Re-applied punishment due to rejoin");

        foreach (ulong offenderId in this.Services.GetRequiredService<AntifloodService>().RegisterJoin(member.Guild.Id, member.Id, gcfg.AntifloodSettings)) {
            SocketGuildUser? offender = member.Guild.GetUser(offenderId);
            if (offender is not null)
                await this.PunishMemberAsync(member.Guild, offender, gcfg.AntifloodAction, "_gf: Flooding");
        }

        this.Services.GetRequiredService<AntiInstantLeaveService>().RegisterJoin(member.Guild.Id, member.Id, gcfg.AntiInstantLeaveSettings);

        ITextChannel? welcomeChannel = member.Guild.GetTextChannel(gcfg.WelcomeChannelId);
        if (welcomeChannel is not null) {
            string welcome = string.IsNullOrWhiteSpace(gcfg.WelcomeMessage)
                ? this.Services.GetRequiredService<LocalizationService>().GetString(member.Guild.Id, TranslationKey.fmt_welcome(member.Guild.Name, member.Mention))
                : gcfg.WelcomeMessage.Replace("%user%", member.Mention);
            await TryDiscordAsync(() => welcomeChannel.SendMessageAsync(embed: SimpleEmbed(welcome, Color.Teal)));
        }

        await this.GrantAutoRolesAsync(member);
        await this.EnforceForbiddenNameAsync(member);
    }

    private async Task HandleUserLeftAsync(SocketGuild guild, SocketUser user)
    {
        if (user.IsBot)
            return;

        GuildConfigService gcs = this.Services.GetRequiredService<GuildConfigService>();
        GuildConfig gcfg = await gcs.GetConfigAsync(guild.Id);
        if (gcfg.AntiInstantLeaveEnabled && this.Services.GetRequiredService<AntiInstantLeaveService>().TryConsumeInstantLeave(guild.Id, user.Id))
            await TryDiscordAsync(() => guild.AddBanAsync(user, 0, "_gf: Instant leave"));

        ITextChannel? leaveChannel = guild.GetTextChannel(gcfg.LeaveChannelId);
        if (leaveChannel is null)
            return;

        string leave = string.IsNullOrWhiteSpace(gcfg.LeaveMessage)
            ? this.Services.GetRequiredService<LocalizationService>().GetString(guild.Id, TranslationKey.fmt_leave(user.Mention))
            : gcfg.LeaveMessage.Replace("%user%", user.Mention);
        await TryDiscordAsync(() => leaveChannel.SendMessageAsync(embed: SimpleEmbed(leave, Color.Teal)));
    }

    private async Task HandleGuildMemberUpdatedAsync(Cacheable<SocketGuildUser, ulong> _, SocketGuildUser after)
    {
        if (after.IsBot)
            return;

        await this.EnforceForbiddenNameAsync(after);
    }

    private Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> messageRef, Cacheable<IMessageChannel, ulong> channelRef, SocketReaction reaction)
        => this.HandleReactionChangedAsync(messageRef, channelRef, reaction, added: true);

    private Task HandleReactionRemovedAsync(Cacheable<IUserMessage, ulong> messageRef, Cacheable<IMessageChannel, ulong> channelRef, SocketReaction reaction)
        => this.HandleReactionChangedAsync(messageRef, channelRef, reaction, added: false);

    private async Task HandleReactionsClearedAsync(Cacheable<IUserMessage, ulong> messageRef, Cacheable<IMessageChannel, ulong> channelRef)
    {
        (SocketGuild? guild, IMessageChannel? channel, IUserMessage? message) = await this.ResolveReactionContextAsync(messageRef, channelRef);
        if (guild is null || channel is null || message is null)
            return;

        StarboardService ss = this.Services.GetRequiredService<StarboardService>();
        if (ss.IsStarboardEnabled(guild.Id, out ulong starboardChannelId, out _) && starboardChannelId != channel.Id)
            ss.RegisterModifiedMessage(guild.Id, channel.Id, message.Id);
    }

    private async Task HandleReactionsRemovedForEmoteAsync(
        Cacheable<IUserMessage, ulong> messageRef,
        Cacheable<IMessageChannel, ulong> channelRef,
        IEmote emote)
    {
        (SocketGuild? guild, IMessageChannel? channel, IUserMessage? message) = await this.ResolveReactionContextAsync(messageRef, channelRef);
        if (guild is null || channel is null || message is null)
            return;

        StarboardService ss = this.Services.GetRequiredService<StarboardService>();
        if (ss.IsStarboardEnabled(guild.Id, out ulong starboardChannelId, out string star) &&
            starboardChannelId != channel.Id &&
            EmoteMatches(guild, emote, star))
            ss.RegisterModifiedMessage(guild.Id, channel.Id, message.Id);
    }

    private async Task HandleCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (result.IsSuccess) {
            if (command.IsSpecified)
                Log.Information(
                    "Executed command {Command} by {User} in {Guild}/{Channel}",
                    BuildQualifiedCommandName(command.Value),
                    context.User,
                    context.Guild?.Name ?? "DM",
                    context.Channel
                );
            return;
        }

        Log.Debug(
            "Command failed ({ErrorReason}) by {User} in {Guild}/{Channel}",
            result.ErrorReason,
            context.User,
            context.Guild?.Name ?? "DM",
            context.Channel
        );

        LocalizationService lcs = this.Services.GetRequiredService<LocalizationService>();
        string description = result.ErrorReason;
        if (result is ExecuteResult { Exception: LocalizedCommandException localized })
            description = lcs.GetString(context.Guild?.Id, localized.Key);
        else if (result is ExecuteResult { Exception: Exception ex } && !string.IsNullOrWhiteSpace(ex.Message))
            description = ex.Message;

        var emb = new LocalizedEmbedBuilder(lcs, context.Guild?.Id)
            .WithColor(Color.Red)
            .WithLocalizedTitle(TranslationKey.cmd_err(command.IsSpecified ? BuildQualifiedCommandName(command.Value) : ""))
            .WithDescription(description);

        await context.Channel.SendMessageAsync(embed: emb.Build());
    }

    private async Task HandleGuildMessageServicesAsync(ShardedCommandContext context, SocketUserMessage message, bool isCommand)
    {
        try {
            await this.BackupMessageAsync(context, message);

            if (!string.IsNullOrWhiteSpace(message.Content)) {
                if (await this.ProcessProtectionAsync(context, message))
                    return;

                if (await this.ProcessLinkfilterAsync(context, message))
                    return;

                if (!isCommand)
                    await this.ProcessXpAsync(context, message);

                if (await this.ProcessFiltersAsync(context, message))
                    return;

                await this.ProcessReactionsAsync(context, message);
            }
        } catch (Exception e) {
            Log.Debug(e, "Guild message service processing failed for message {MessageId}", message.Id);
        }
    }

    private async Task BackupMessageAsync(ShardedCommandContext context, SocketUserMessage message)
    {
        if (context.Guild is null)
            return;

        BackupService backups = this.Services.GetRequiredService<BackupService>();
        if (!backups.IsBackupEnabledFor(context.Guild.Id) || backups.IsChannelExempted(context.Guild.Id, context.Channel.Id))
            return;

        var sb = new StringBuilder();
        sb.Append('[').Append(message.CreatedAt).Append("] ").Append(message.Author).AppendLine(":");
        if (!string.IsNullOrWhiteSpace(message.Content))
            sb.AppendLine(message.Content);

        foreach (IEmbed embed in message.Embeds) {
            sb.Append("----- ").Append(embed.Title).AppendLine(" -----");
            sb.AppendLine(embed.Description);
            foreach (EmbedField field in embed.Fields) {
                sb.Append("# ").Append(field.Name).AppendLine(" #");
                sb.AppendLine(field.Value);
            }
            if (embed.Image?.Url is { } imageUrl)
                sb.AppendLine(imageUrl);
            if (!string.IsNullOrWhiteSpace(embed.Footer?.Text))
                sb.AppendLine("---").AppendLine(embed.Footer.Value.Text);
        }

        foreach (IAttachment attachment in message.Attachments)
            sb.Append("(A): ").Append(attachment.Filename).Append(" (").Append(attachment.Size).Append("): ").AppendLine(attachment.Url);

        await backups.BackupAsync(context.Guild.Id, context.Channel.Id, sb.ToString());
    }

    private async Task ProcessXpAsync(ShardedCommandContext context, SocketUserMessage message)
    {
        if (context.Guild is null || message.Author is not SocketGuildUser member)
            return;

        short rank = this.Services.GetRequiredService<UserRanksService>().ChangeXp(context.Guild.Id, message.Author.Id);
        if (rank == 0)
            return;

        LocalizationService lcs = this.Services.GetRequiredService<LocalizationService>();
        LevelRole? levelRoleConfig = await this.Services.GetRequiredService<LevelRoleService>().GetAsync(context.Guild.Id, rank);
        SocketRole? levelRole = levelRoleConfig is null ? null : context.Guild.GetRole(levelRoleConfig.RoleId);
        if (levelRole is not null)
            await TryDiscordAsync(() => member.AddRoleAsync(levelRole, new RequestOptions { AuditLogReason = "Level role" }));

        GuildConfig cfg = await this.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(context.Guild.Id);
        if (cfg.SilentLevelUpEnabled)
            return;

        XpRank? rankInfo = await this.Services.GetRequiredService<GuildRanksService>().GetAsync(context.Guild.Id, rank);
        TranslationKey response = levelRole is not null
            ? TranslationKey.fmt_rankup_lr(message.Author.Mention, Format.Bold(rank.ToString()), rankInfo?.Name ?? "/", levelRole.Mention)
            : TranslationKey.fmt_rankup(message.Author.Mention, Format.Bold(rank.ToString()), rankInfo?.Name ?? "/");

        await context.Channel.SendMessageAsync(embed: SimpleEmbed($"{Emojis.Medal} {lcs.GetString(context.Guild.Id, response)}", Color.Gold));
    }

    private async Task<bool> ProcessFiltersAsync(ShardedCommandContext context, SocketUserMessage message)
    {
        if (context.Guild is null)
            return false;

        FilteringService filters = this.Services.GetRequiredService<FilteringService>();
        if (!filters.TextContainsFilter(context.Guild.Id, message.Content, out Filter? match))
            return false;

        await this.SanitizeFilteredMessageAsync(context, message, match?.OnHitAction);
        return true;
    }

    private async Task ProcessReactionsAsync(ShardedCommandContext context, SocketUserMessage message)
    {
        if (context.Guild is null)
            return;

        ReactionsService reactions = this.Services.GetRequiredService<ReactionsService>();

        EmojiReaction? er = reactions.FindMatchingEmojiReactions(context.Guild.Id, message.Content).Shuffle().FirstOrDefault();
        if (er is not null) {
            if (TryResolveEmote(context.Guild, er.Response, out IEmote? emote, out _)) {
                await TryDiscordAsync(() => message.AddReactionAsync(emote));
            } else {
                await reactions.RemoveEmojiReactionsWhereAsync(context.Guild.Id, r => r.HasSameResponseAs(er));
            }
        }

        TextReaction? tr = reactions.FindMatchingTextReaction(context.Guild.Id, message.Content);
        if (tr is not null && tr.CanSend())
            await context.Channel.SendMessageAsync(tr.Response.Replace("%user%", message.Author.Mention));
    }

    private async Task<bool> ProcessProtectionAsync(ShardedCommandContext context, SocketUserMessage message)
    {
        if (context.Guild is null || message.Author is not SocketGuildUser member)
            return false;

        CachedGuildConfig cfg = this.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(context.Guild.Id);
        ulong? parentId = GetParentChannelId(context.Channel);
        ulong[] roleIds = member.Roles.Select(r => r.Id).ToArray();

        if (this.Services.GetRequiredService<RatelimitService>().ShouldPunish(context.Guild.Id, member.Id, context.Channel.Id, parentId, roleIds, cfg.RatelimitSettings)) {
            await this.PunishMemberAsync(context.Guild, member, cfg.RatelimitSettings.Action, "_gf: Ratelimit hit");
            return true;
        }

        if (this.Services.GetRequiredService<AntispamService>().ShouldPunish(context.Guild.Id, member.Id, context.Channel.Id, parentId, roleIds, message.Content, cfg.AntispamSettings)) {
            await this.PunishMemberAsync(context.Guild, member, cfg.AntispamSettings.Action, "_gf: Antispam");
            return true;
        }

        int mentionCount = (message.MentionedEveryone ? 1 : 0) + message.MentionedUsers.Count + message.MentionedRoles.Count + message.MentionedChannels.Count;
        if (this.Services.GetRequiredService<AntiMentionService>().ShouldPunish(context.Guild.Id, member.Id, context.Channel.Id, parentId, roleIds, mentionCount, cfg.AntiMentionSettings)) {
            await this.PunishMemberAsync(context.Guild, member, cfg.AntiMentionSettings.Action, "_gf: Anti-mention");
            return true;
        }

        return false;
    }

    private async Task<bool> ProcessLinkfilterAsync(ShardedCommandContext context, SocketUserMessage message)
    {
        if (context.Guild is null || message.Author is not SocketGuildUser member)
            return false;

        LinkfilterSettings settings = this.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(context.Guild.Id).LinkfilterSettings;
        LinkfilterResult? match = this.Services.GetRequiredService<LinkfilterService>().Match(message.Content, settings, member.GuildPermissions.Administrator);
        if (match is null)
            return false;

        await TryDiscordAsync(() => message.DeleteAsync(new RequestOptions { AuditLogReason = $"_gf: {match.Cause} linkfilter" }));
        await this.LogLinkfilterMatchAsync(context.Guild.Id, context.Channel.Id, member.Id, member.Roles.Select(r => r.Id), match);
        await TryDiscordAsync(() => member.SendMessageAsync($"Your message was automatically filtered from {Format.Bold(context.Guild.Name)}."));
        return true;
    }

    private async Task LogLinkfilterMatchAsync(ulong gid, ulong cid, ulong uid, IEnumerable<ulong> roleIds, LinkfilterResult match)
    {
        LoggingService logging = this.Services.GetRequiredService<LoggingService>();
        if (!logging.IsLogEnabledFor(gid, out LocalizedEmbedBuilder emb) || logging.IsExempted(gid, cid, null, uid, roleIds))
            return;

        emb.WithLocalizedTitle(TranslationKey.evt_lf_triggered);
        emb.WithDescription(match.Cause);
        emb.WithColor(Color.Red);
        if (!string.IsNullOrWhiteSpace(match.Matched))
            emb.AddLocalizedField(TranslationKey.str_matched, match.Matched);
        await logging.LogAsync(gid, emb.Build());
    }

    private async Task SanitizeFilteredMessageAsync(ShardedCommandContext context, SocketUserMessage message, Filter.Action? action)
    {
        if (context.Guild is null || message.Author is not SocketGuildUser member)
            return;

        LocalizationService lcs = this.Services.GetRequiredService<LocalizationService>();
        string reason = lcs.GetString(context.Guild.Id, TranslationKey.rsn_filter_match);

        try {
            await message.DeleteAsync(new RequestOptions { AuditLogReason = reason });
            if (action.GetValueOrDefault(Filter.Action.Delete) == Filter.Action.Sanitize)
                await context.Channel.SendMessageAsync(embed: SimpleEmbed(
                    lcs.GetString(context.Guild.Id, TranslationKey.fmt_filter(member.Mention, Format.Spoiler(Format.Sanitize(message.Content)))),
                    Color.Gold
                ));
        } catch (Exception e) when (e is Discord.Net.HttpException or UnauthorizedAccessException) {
            Log.Debug(e, "Failed to delete filtered message {MessageId}", message.Id);
        }

        if (action is null or Filter.Action.Delete or Filter.Action.Sanitize)
            return;

        if (Enum.TryParse(action.ToString(), out Punishment.Action punishment))
            await this.PunishMemberAsync(context.Guild, member, punishment, reason);
    }

    private bool IsCommandRuleBlocked(ShardedCommandContext context, string input)
    {
        CommandInfo? command = this.ResolveCommand(input);
        if (command is null)
            return false;

        string qualifiedName = BuildQualifiedCommandName(command);
        ulong? parentId = GetParentChannelId(context.Channel);
        return this.Services.GetRequiredService<CommandRulesService>().IsBlocked(qualifiedName, context.Guild.Id, context.Channel.Id, parentId);
    }

    private CommandInfo? ResolveCommand(string input)
    {
        string commandText = input.Trim();
        return this.Commands.Commands
            .Select(command => new {
                Command = command,
                MatchLength = GetInvocationAliases(command)
                    .Where(alias => CommandTextMatches(commandText, alias))
                    .Select(alias => alias.Length)
                    .DefaultIfEmpty(-1)
                    .Max()
            })
            .Where(match => match.MatchLength >= 0)
            .OrderByDescending(match => match.MatchLength)
            .Select(match => match.Command)
            .FirstOrDefault();
    }

    private async Task HandleReactionChangedAsync(
        Cacheable<IUserMessage, ulong> messageRef,
        Cacheable<IMessageChannel, ulong> channelRef,
        SocketReaction reaction,
        bool added)
    {
        (SocketGuild? guild, IMessageChannel? channel, IUserMessage? message) = await this.ResolveReactionContextAsync(messageRef, channelRef);
        if (guild is null || channel is null || message is null)
            return;

        SocketGuildUser? user = guild.GetUser(reaction.UserId);
        if (added && (user?.IsBot ?? false))
            return;

        StarboardService ss = this.Services.GetRequiredService<StarboardService>();
        if (ss.IsStarboardEnabled(guild.Id, out ulong starboardChannelId, out string star) &&
            starboardChannelId != channel.Id &&
            EmoteMatches(guild, reaction.Emote, star))
            ss.RegisterModifiedMessage(guild.Id, channel.Id, message.Id);

        ReactionRoleService rrs = this.Services.GetRequiredService<ReactionRoleService>();
        ReactionRole? rr = await rrs.GetAsync(guild.Id, GetEmoteName(reaction.Emote), channel.Id, message.Id);
        rr ??= await rrs.GetAsync(guild.Id, reaction.Emote.ToString() ?? reaction.Emote.Name, channel.Id, message.Id);
        if (rr is null || user is null)
            return;

        SocketRole? role = guild.GetRole(rr.RoleId);
        if (role is null)
            return;

        if (added)
            await TryDiscordAsync(() => user.AddRoleAsync(role, new RequestOptions { AuditLogReason = "_gf: Reaction role" }));
        else
            await TryDiscordAsync(() => user.RemoveRoleAsync(role, new RequestOptions { AuditLogReason = "_gf: Reaction role" }));
    }

    private async Task<(SocketGuild? Guild, IMessageChannel? Channel, IUserMessage? Message)> ResolveReactionContextAsync(
        Cacheable<IUserMessage, ulong> messageRef,
        Cacheable<IMessageChannel, ulong> channelRef)
    {
        IMessageChannel? channel = channelRef.HasValue ? channelRef.Value : await channelRef.GetOrDownloadAsync();
        if (channel is not IGuildChannel guildChannel)
            return (null, channel, null);

        SocketGuild? guild = this.Client.GetGuild(guildChannel.GuildId);
        IUserMessage? message = messageRef.HasValue ? messageRef.Value : await messageRef.GetOrDownloadAsync();
        return (guild, channel, message);
    }

    private async Task GrantAutoRolesAsync(SocketGuildUser member)
    {
        AutoRoleService autoRoles = this.Services.GetRequiredService<AutoRoleService>();
        IEnumerable<SocketRole> roles = autoRoles.GetIds(member.Guild.Id)
            .Select(member.Guild.GetRole)
            .Where(role => role is not null)
            .Cast<SocketRole>();

        foreach (SocketRole role in roles)
            await TryDiscordAsync(() => member.AddRoleAsync(role, new RequestOptions { AuditLogReason = "_gf: Auto role" }));
    }

    private async Task EnforceForbiddenNameAsync(SocketGuildUser member)
    {
        ForbiddenNamesService forbidden = this.Services.GetRequiredService<ForbiddenNamesService>();
        if (!forbidden.IsNameForbidden(member.Guild.Id, member.DisplayName, out ForbiddenName? match) || match is null)
            return;

        LocalizationService lcs = this.Services.GetRequiredService<LocalizationService>();
        string reason = lcs.GetString(member.Guild.Id, TranslationKey.rsn_fname_match(match.RegexString));

        if (match.ActionOverride is null) {
            await TryDiscordAsync(() => member.ModifyAsync(props => props.Nickname = member.Id.ToString(), new RequestOptions { AuditLogReason = reason }));
        } else {
            await this.PunishMemberAsync(member.Guild, member, match.ActionOverride.Value, reason);
        }

        await this.LogActionHistoryAsync(member.Guild.Id, member.Id, ActionHistoryEntry.Action.ForbiddenName, reason);
        await TryDiscordAsync(() => member.SendMessageAsync(lcs.GetString(null, TranslationKey.dm_fname_match(Format.Italics(member.Guild.Name)))));
    }

    private async Task PunishMemberAsync(SocketGuild guild, SocketGuildUser member, Punishment.Action action, string reason)
    {
        GuildConfigService gcs = this.Services.GetRequiredService<GuildConfigService>();
        GuildConfig cfg = await gcs.GetConfigAsync(guild.Id);

        switch (action) {
            case Punishment.Action.TemporaryMute: {
                IRole muteRole = await this.GetOrCreateMuteRoleAsync(guild, cfg);
                await TryDiscordAsync(() => member.AddRoleAsync(muteRole, new RequestOptions { AuditLogReason = reason }));
                await this.Services.GetRequiredService<ProtectionService>().LogPunishmentAsync(guild.Id, member.Id, action);
                await this.Services.GetRequiredService<SchedulingService>().ScheduleAsync(new GuildTask {
                    GuildId = guild.Id,
                    UserId = member.Id,
                    RoleId = muteRole.Id,
                    ExecutionTime = DateTimeOffset.Now + cfg.TempMuteCooldown,
                    Type = ScheduledTaskType.Unmute
                });
                await this.LogActionHistoryAsync(guild.Id, member.Id, ActionHistoryEntry.Action.TemporaryMute, reason);
                break;
            }
            case Punishment.Action.PermanentMute: {
                IRole muteRole = await this.GetOrCreateMuteRoleAsync(guild, cfg);
                await TryDiscordAsync(() => member.AddRoleAsync(muteRole, new RequestOptions { AuditLogReason = reason }));
                await this.Services.GetRequiredService<ProtectionService>().LogPunishmentAsync(guild.Id, member.Id, action);
                await this.LogActionHistoryAsync(guild.Id, member.Id, ActionHistoryEntry.Action.IndefiniteMute, reason);
                break;
            }
            case Punishment.Action.Kick:
                await TryDiscordAsync(() => member.KickAsync(reason));
                await this.LogActionHistoryAsync(guild.Id, member.Id, ActionHistoryEntry.Action.Kick, reason);
                break;
            case Punishment.Action.TemporaryBan:
                await TryDiscordAsync(() => guild.AddBanAsync(member, 0, reason));
                await this.Services.GetRequiredService<SchedulingService>().ScheduleAsync(new GuildTask {
                    GuildId = guild.Id,
                    UserId = member.Id,
                    ExecutionTime = DateTimeOffset.Now + cfg.TempBanCooldown,
                    Type = ScheduledTaskType.Unban
                });
                await this.LogActionHistoryAsync(guild.Id, member.Id, ActionHistoryEntry.Action.TemporaryBan, reason);
                break;
            case Punishment.Action.PermanentBan:
                await TryDiscordAsync(() => guild.AddBanAsync(member, 0, reason));
                await this.LogActionHistoryAsync(guild.Id, member.Id, ActionHistoryEntry.Action.PermanentBan, reason);
                break;
        }
    }

    private async Task<IRole> GetOrCreateMuteRoleAsync(SocketGuild guild, GuildConfig cfg)
    {
        if (cfg.MuteRoleId != 0 && guild.GetRole(cfg.MuteRoleId) is { } existing)
            return existing;

        IRole role = await guild.CreateRoleAsync(
            "_gf: Muted",
            GuildPermissions.None,
            Color.DarkGrey,
            false,
            false,
            new RequestOptions { AuditLogReason = "Create mute role" }
        );
        await this.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(guild.Id, c => c.MuteRoleId = role.Id);
        return role;
    }

    private async Task LogActionHistoryAsync(ulong guildId, ulong userId, ActionHistoryEntry.Action action, string? notes)
    {
        GuildConfig cfg = await this.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(guildId);
        if (!cfg.ActionHistoryEnabled)
            return;

        await this.Services.GetRequiredService<ActionHistoryService>().LimitedAddAsync(new ActionHistoryEntry {
            GuildId = guildId,
            UserId = userId,
            Time = DateTimeOffset.Now,
            Type = action,
            Notes = notes
        });
    }

    private async Task<bool> IsOwnerAsync(ulong userId)
    {
        IApplication app = await this.Client.GetApplicationInfoAsync();
        return app.Owner?.Id == userId;
    }

    private static Task LogDiscordNetAsync(LogMessage msg)
    {
        LogEventLevel level = msg.Severity switch {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };

        if (msg.Source == "Command" && msg.Message.StartsWith("Executing ", StringComparison.Ordinal))
            return Task.CompletedTask;

        Log.Write(level, msg.Exception, "{Source}: {Message}", msg.Source, msg.Message);
        return Task.CompletedTask;
    }

    private static Embed SimpleEmbed(string description, Color color)
        => new EmbedBuilder {
            Color = color,
            Description = description
        }.Build();

    private static Task TryDiscordAsync(Func<Task> action)
        => TryDiscordAsync(async () => {
            await action();
            return true;
        });

    private static async Task<T?> TryDiscordAsync<T>(Func<Task<T>> action)
    {
        try {
            return await action();
        } catch (Exception e) when (e is Discord.Net.HttpException or TimeoutException or InvalidOperationException) {
            Log.Debug(e, "Discord operation failed");
            return default;
        }
    }

    private static bool TryResolveEmote(SocketGuild guild, string value, out IEmote emote, out string display)
    {
        if (Emote.TryParse(value, out Emote? custom)) {
            emote = custom;
            display = custom.ToString();
            return true;
        }

        string trimmed = value.Trim();
        string name = trimmed.Length > 2 && trimmed.StartsWith(':') && trimmed.EndsWith(':')
            ? trimmed[1..^1]
            : trimmed;

        GuildEmote? guildEmote = guild.Emotes.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
        if (guildEmote is not null) {
            emote = guildEmote;
            display = guildEmote.ToString();
            return true;
        }

        if (string.Equals(trimmed, ":star:", StringComparison.OrdinalIgnoreCase))
            trimmed = Emojis.Star;

        try {
            var emoji = new Emoji(trimmed);
            emote = emoji;
            display = emoji.ToString();
            return true;
        } catch {
            emote = new Emoji(Emojis.Question);
            display = value;
            return false;
        }
    }

    private static bool EmoteMatches(SocketGuild guild, IEmote emote, string configured)
    {
        if (TryResolveEmote(guild, configured, out IEmote resolved, out _)) {
            if (resolved is Emote resolvedCustom && emote is Emote reactionCustom)
                return resolvedCustom.Id == reactionCustom.Id;
            if (resolved is GuildEmote resolvedGuild && emote is GuildEmote reactionGuild)
                return resolvedGuild.Id == reactionGuild.Id;
            if (string.Equals(resolved.Name, emote.Name, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        string reactionName = GetEmoteName(emote);
        return string.Equals(configured, reactionName, StringComparison.OrdinalIgnoreCase)
               || string.Equals(configured, emote.ToString(), StringComparison.OrdinalIgnoreCase)
               || (string.Equals(configured, ":star:", StringComparison.OrdinalIgnoreCase) && emote.Name == Emojis.Star);
    }

    private static string GetEmoteName(IEmote emote)
        => emote switch {
            Emote custom => custom.ToString(),
            _ => emote.Name
        };

    private static ulong? GetParentChannelId(IMessageChannel channel)
        => channel switch {
            SocketTextChannel text => text.CategoryId,
            ITextChannel text => text.CategoryId,
            _ => null
        };

    private static string BuildQualifiedCommandName(CommandInfo command)
    {
        var parts = new List<string>();
        for (ModuleInfo? module = command.Module; module is not null; module = module.Parent) {
            string? group = module.Group ?? module.Aliases.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(group))
                parts.Insert(0, group);
        }

        string? name = command.Name;
        if (!string.IsNullOrWhiteSpace(name))
            parts.Add(name);

        return string.Join(' ', parts).Trim().ToLowerInvariant();
    }

    private static IEnumerable<string> GetInvocationAliases(CommandInfo command)
    {
        IEnumerable<string> moduleAliases = GetModuleAliasChains(command.Module);
        string[] commandAliases = command.Aliases.Any() ? command.Aliases.ToArray() : new[] { command.Name };

        foreach (string moduleAlias in moduleAliases) {
            foreach (string commandAlias in commandAliases) {
                string alias = string.IsNullOrWhiteSpace(commandAlias)
                    ? moduleAlias
                    : string.IsNullOrWhiteSpace(moduleAlias)
                        ? commandAlias
                        : $"{moduleAlias} {commandAlias}";
                if (!string.IsNullOrWhiteSpace(alias))
                    yield return alias.Trim().ToLowerInvariant();
            }
        }
    }

    private static IEnumerable<string> GetModuleAliasChains(ModuleInfo? module)
    {
        if (module is null)
            return new[] { string.Empty };

        IEnumerable<string> parentAliases = GetModuleAliasChains(module.Parent);
        string[] aliases = module.Aliases.Any()
            ? module.Aliases.ToArray()
            : string.IsNullOrWhiteSpace(module.Group)
                ? new[] { string.Empty }
                : new[] { module.Group };

        return from parent in parentAliases
               from alias in aliases
               select string.IsNullOrWhiteSpace(parent)
                   ? alias
                   : string.IsNullOrWhiteSpace(alias)
                       ? parent
                       : $"{parent} {alias}";
    }

    private static bool CommandTextMatches(string input, string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
            return false;

        return input.Equals(alias, StringComparison.OrdinalIgnoreCase)
               || input.StartsWith(alias + " ", StringComparison.OrdinalIgnoreCase);
    }
}
