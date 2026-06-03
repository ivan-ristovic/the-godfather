using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Modules.Misc.Services;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Services;

public sealed class PeriodicTasksService : IDisposable
{
    private static readonly Regex _feedImageRegex = new(
        "<span> *<a +href *= *\"([^\"]+)\"> *\\[link\\] *</a> *</span>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private readonly TheGodfatherBot bot;
    private readonly Timer botStatusUpdateTimer;
    private readonly Timer databaseSyncTimer;
    private readonly Timer feedCheckTimer;
    private readonly Timer miscActionsTimer;
    private readonly Timer starboardTimer;


    public PeriodicTasksService(TheGodfatherBot bot, BotConfig cfg)
    {
        this.bot = bot;
        this.botStatusUpdateTimer = new Timer(this.BotActivityChangeCallback, null, TimeSpan.FromSeconds(25), TimeSpan.FromMinutes(10));
        this.databaseSyncTimer = new Timer(this.XpSyncCallback, null, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(cfg.DatabaseSyncInterval));
        this.feedCheckTimer = new Timer(this.FeedCheckCallback, null, TimeSpan.FromSeconds(cfg.FeedCheckStartDelay), TimeSpan.FromSeconds(cfg.FeedCheckInterval));
        this.miscActionsTimer = new Timer(this.MiscellaneousActionsCallback, null, TimeSpan.FromSeconds(35), TimeSpan.FromHours(12));
        this.starboardTimer = new Timer(this.StarboardUpdateCallback, null, TimeSpan.FromSeconds(45), TimeSpan.FromMinutes(3));
    }


    public void Dispose()
    {
        this.botStatusUpdateTimer.Dispose();
        this.databaseSyncTimer.Dispose();
        this.feedCheckTimer.Dispose();
        this.miscActionsTimer.Dispose();
        this.starboardTimer.Dispose();
    }


    private void BotActivityChangeCallback(object? _)
    {
        try {
            BotActivityService bas = this.bot.Services.GetRequiredService<BotActivityService>();
            if (!bas.StatusRotationEnabled)
                return;

            BotStatus? status = bas.GetRandomStatus();
            AsyncExecutionService async = this.bot.Services.GetRequiredService<AsyncExecutionService>();
            if (status is not null) {
                async.Execute(this.bot.Client.SetGameAsync(status.Status, type: status.Activity));
                Log.Debug("Changed bot status to {ActivityType} {ActivityName}", status.Activity, status.Status);
            } else {
                async.Execute(this.bot.Client.SetGameAsync($"{this.bot.Config.CurrentConfiguration.Prefix}help", type: ActivityType.Playing));
                Log.Warning("No extra bot statuses present in the database");
            }
        } catch (Exception e) {
            Log.Error(e, "An error occurred during activity change");
        }
    }

    private void XpSyncCallback(object? _)
    {
        try {
            AsyncExecutionService async = this.bot.Services.GetRequiredService<AsyncExecutionService>();
            async.Execute(this.bot.Services.GetRequiredService<UserRanksService>().Sync());
            Log.Debug("XP data synced with the database");
        } catch (Exception e) {
            Log.Error(e, "An error occurred during database sync");
        }
    }

    private void FeedCheckCallback(object? _)
    {
        try {
            Log.Debug("Feed check starting...");
            RssFeedsService rss = this.bot.Services.GetRequiredService<RssFeedsService>();
            AsyncExecutionService async = this.bot.Services.GetRequiredService<AsyncExecutionService>();
            IReadOnlyList<(RssFeed, SyndicationItem)> updates = async.Execute(rss.CheckAsync());

            var notFound = new List<RssSubscription>();
            foreach ((RssFeed feed, SyndicationItem latest) in updates) {
                foreach (RssSubscription sub in feed.Subscriptions) {
                    if (!async.Execute(this.SendFeedUpdateAsync(sub, latest)))
                        notFound.Add(sub);
                    async.Execute(Task.Delay(10));
                }
            }

            if (notFound.Any()) {
                Log.Information("404 subscriptions found. Removing {Count} subscriptions", notFound.Count);
                async.Execute(rss.Subscriptions.RemoveAsync(notFound));
            }

            Log.Debug("Feed check finished");
        } catch (Exception e) {
            Log.Error(e, "An error occurred during periodic feed processing");
        }
    }

    private void MiscellaneousActionsCallback(object? _)
    {
        try {
            List<Birthday> todayBirthdays;
            using (TheGodfatherDbContext db = this.bot.Database.CreateContext()) {
                todayBirthdays = db.Birthdays
                    .AsQueryable()
                    .Where(b => b.Date.Month == DateTime.Now.Month && b.Date.Day == DateTime.Now.Day && b.LastUpdateYear < DateTime.Now.Year)
                    .ToList();
            }

            AsyncExecutionService async = this.bot.Services.GetRequiredService<AsyncExecutionService>();
            foreach (Birthday birthday in todayBirthdays) {
                IMessageChannel? channel = this.bot.Client.GetChannel(birthday.ChannelId) as IMessageChannel;
                if (channel is null)
                    continue;

                string mention = MentionUtils.MentionUser(birthday.UserId);
                async.Execute(channel.SendMessageAsync(embed: new EmbedBuilder {
                    Description = $"{Emojis.Tada} Happy birthday, {mention}! {Emojis.Cake}",
                    Color = Color.Teal
                }.Build()));

                using TheGodfatherDbContext db = this.bot.Database.CreateContext();
                birthday.LastUpdateYear = DateTime.Now.Year;
                db.Birthdays.Update(birthday);
                db.SaveChanges();
            }
            Log.Debug("Birthdays checked");

            using (TheGodfatherDbContext db = this.bot.Database.CreateContext()) {
                switch (this.bot.Database.Provider) {
                    case DbProvider.PostgreSql:
                        db.Database.ExecuteSqlRaw("UPDATE gf.bank_accounts SET balance = GREATEST(CEILING(1.0015 * balance), 10);");
                        break;
                    case DbProvider.Sqlite:
                    case DbProvider.SqliteInMemory:
                        db.Database.ExecuteSqlRaw("UPDATE bank_accounts SET balance = MAX(ROUND(0.5 + 1.0015 * balance), 10);");
                        break;
                    case DbProvider.SqlServer:
                        db.Database.ExecuteSqlRaw("UPDATE dbo.bank_accounts SET balance = MAX(CEILING(1.0015 * balance), 10);");
                        break;
                }
            }
            Log.Debug("Currency updated for all users");
        } catch (Exception e) {
            Log.Error(e, "An error occurred during misc timer callback");
        }
    }

    private void StarboardUpdateCallback(object? _)
    {
        try {
            AsyncExecutionService async = this.bot.Services.GetRequiredService<AsyncExecutionService>();
            StarboardService ss = this.bot.Services.GetRequiredService<StarboardService>();
            IReadOnlyDictionary<ulong, List<StarboardMessage>> updatedMessages = ss.GetUpdatedMessages();

            foreach ((ulong gid, List<StarboardMessage> toUpdate) in updatedMessages) {
                SocketGuild? guild = this.bot.Client.GetGuild(gid);
                if (guild is null || !ss.IsStarboardEnabled(gid, out ulong starChannelId, out string emoji))
                    continue;

                ITextChannel? starChannel = this.bot.Client.GetChannel(starChannelId) as ITextChannel;
                if (starChannel is null || !TryResolveEmote(guild, emoji, out IEmote? starEmote, out string starDisplay)) {
                    async.Execute(ss.ModifySettingsAsync(gid, null));
                    continue;
                }

                foreach (StarboardMessage updated in toUpdate)
                    async.Execute(this.UpdateStarboardMessageAsync(guild, starChannel, starEmote, starDisplay, updated));
            }

            Log.Debug("Starboards updated for all guilds. {Count} guilds checked", updatedMessages.Count);
        } catch (Exception e) {
            Log.Error(e, "An error occurred during starboard timer callback");
        }
    }

    private async Task<bool> SendFeedUpdateAsync(RssSubscription sub, SyndicationItem latest)
    {
        IMessageChannel? channel = this.bot.Client.GetChannel(sub.ChannelId) as IMessageChannel;
        if (channel is null)
            return false;

        var emb = new LocalizedEmbedBuilder(this.bot.Services.GetRequiredService<LocalizationService>(), sub.GuildId);
        emb.WithTitle(latest.Title.Text);
        emb.WithUrl(sub.Feed.LastPostUrl);
        emb.WithColor(Color.Gold);
        emb.WithLocalizedTimestamp(latest.LastUpdatedTime > latest.PublishDate ? latest.LastUpdatedTime : latest.PublishDate);

        if (latest.Content is TextSyndicationContent content) {
            Match match = _feedImageRegex.Match(content.Text);
            if (match.Success)
                emb.WithImageUrl(match.Groups[1].Value);
        }

        if (!string.IsNullOrWhiteSpace(sub.Name))
            emb.AddLocalizedField(TranslationKey.str_from, sub.Name);
        emb.AddLocalizedField(TranslationKey.str_content, sub.Feed.LastPostUrl);

        await channel.SendMessageAsync(embed: emb.Build());
        return true;
    }

    private async Task UpdateStarboardMessageAsync(
        SocketGuild guild,
        ITextChannel starChannel,
        IEmote starEmote,
        string starDisplay,
        StarboardMessage updated)
    {
        StarboardService ss = this.bot.Services.GetRequiredService<StarboardService>();

        ITextChannel? sourceChannel = this.bot.Client.GetChannel(updated.ChannelId) as ITextChannel;
        if (sourceChannel is null)
            return;

        IUserMessage? sourceMessage = await sourceChannel.GetMessageAsync(updated.MessageId) as IUserMessage;
        if (sourceMessage is null)
            return;

        updated.Stars = await CountReactionsAsync(sourceMessage, starEmote);
        StarboardModificationResult result = await ss.SyncWithDbAsync(updated);
        Log.Debug("Starboard action for message {MessageId}: {StarboardModResult}", sourceMessage.Id, result.ActionType);

        IUserMessage? starMessage = null;
        if (result.Entry is not null && result.Entry.StarMessageId != 0)
            starMessage = await starChannel.GetMessageAsync(result.Entry.StarMessageId) as IUserMessage;

        Embed embed = this.BuildStarboardEmbed(guild.Id, sourceMessage, starDisplay, updated.Stars);
        switch (result.ActionType) {
            case StarboardActionType.Send:
                starMessage = await starChannel.SendMessageAsync(embed: embed);
                await ss.AddStarboardLinkAsync(updated.GuildId, updated.ChannelId, updated.MessageId, starMessage.Id);
                break;
            case StarboardActionType.Delete:
                if (starMessage is not null)
                    await starMessage.DeleteAsync(new RequestOptions { AuditLogReason = "_gf: Starboard - delete" });
                break;
            case StarboardActionType.Modify:
                if (starMessage is null) {
                    starMessage = await starChannel.SendMessageAsync(embed: embed);
                    await ss.AddStarboardLinkAsync(updated.GuildId, updated.ChannelId, updated.MessageId, starMessage.Id);
                } else {
                    await starMessage.ModifyAsync(props => props.Embed = embed);
                }
                break;
        }
    }

    private Embed BuildStarboardEmbed(ulong guildId, IUserMessage message, string starDisplay, int count)
    {
        LocalizationService lcs = this.bot.Services.GetRequiredService<LocalizationService>();
        string jumpLink = BuildJumpLink(guildId, message.Channel.Id, message.Id);
        string localizedJumpLink = Format.Url(lcs.GetString(guildId, TranslationKey.str_jumplink), jumpLink);

        var emb = new LocalizedEmbedBuilder(lcs, guildId);
        emb.WithColor(Color.Gold);
        emb.WithUrl(jumpLink);
        emb.WithTitle(message.Author.ToString());
        emb.WithDescription(message.Content?.Truncate(DiscordLimits.EmbedDescriptionLimit - 5, " ..."), false);
        emb.AddLocalizedField(TranslationKey.str_votes, $"{Format.Bold(count.ToString())} {starDisplay}", true);
        emb.AddLocalizedField(TranslationKey.str_chn, MentionUtils.MentionChannel(message.Channel.Id), true);
        emb.AddLocalizedField(TranslationKey.str_link, localizedJumpLink, true);

        string? imageUrl = message.Attachments
            .Select(a => a.Url)
            .FirstOrDefault(u => u.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                 || u.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                                 || u.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                                 || u.EndsWith(".gif", StringComparison.OrdinalIgnoreCase));
        if (imageUrl is not null)
            emb.WithImageUrl(imageUrl);

        emb.WithLocalizedTimestamp(message.CreatedAt);
        return emb.Build();
    }

    private static async Task<int> CountReactionsAsync(IUserMessage message, IEmote emote)
    {
        if (message.Reactions.TryGetValue(emote, out ReactionMetadata metadata))
            return metadata.ReactionCount;

        return (await message.GetReactionUsersAsync(emote, int.MaxValue).FlattenAsync()).Count();
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

    private static string BuildJumpLink(ulong guildId, ulong channelId, ulong messageId)
        => $"https://discord.com/channels/{guildId}/{channelId}/{messageId}";
}
