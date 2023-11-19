﻿using System.ServiceModel.Syndication;
using System.Threading;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Modules.Misc.Extensions;
using TheGodfather.Modules.Misc.Services;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services.Common;
using TheGodfather.Services.Extensions;

namespace TheGodfather.Services;

public sealed class PeriodicTasksService : IDisposable
{
    #region Callbacks
    private static void BotActivityChangeCallback(object? _)
    {
        if (_ is TheGodfatherBot bot) {
            if (bot.Client.CurrentUser is null) {
                Log.Error("BotActivityChangeCallback detected null client/user - this should not happen but is not nececarily an error");
                return;
            }

            BotActivityService bas = bot.Services.GetRequiredService<BotActivityService>();
            if (!bas.StatusRotationEnabled)
                return;

            try {
                BotStatus? status = bas.GetRandomStatus();
                if (status is null)
                    Log.Warning("No extra bot statuses present in the database");

                DiscordActivity activity = status is not null
                                               ? new DiscordActivity(status.Status, status.Activity)
                    : new DiscordActivity($"@{bot.Client.CurrentUser.Username} help", ActivityType.Playing);

                AsyncExecutionService async = bot.Services.GetRequiredService<AsyncExecutionService>();
                async.Execute(bot.Client.UpdateStatusAsync(activity));
                Log.Debug("Changed bot status to {ActivityType} {ActivityName}", activity.ActivityType, activity.Name);
            } catch (Exception e) {
                Log.Error(e, "An error occured during activity change");
            }
        } else {
            Log.Error("BotActivityChangeCallback failed to cast sender");
        }
    }

    private static void XpSyncCallback(object? _)
    {
        if (_ is TheGodfatherBot bot) {
            try {
                AsyncExecutionService async = bot.Services.GetRequiredService<AsyncExecutionService>();
                async.Execute(bot.Services.GetRequiredService<UserRanksService>().Sync());
                Log.Debug("XP data synced with the database");
            } catch (Exception e) {
                Log.Error(e, "An error occured during database sync");
            }
        } else {
            Log.Error("XpSyncCallback failed to cast sender");
        }
    }

    private static void FeedCheckCallback(object? _)
    {
        if (_ is TheGodfatherBot bot) {
            Log.Debug("Feed check starting...");
            try {
                RssFeedsService rss = bot.Services.GetRequiredService<RssFeedsService>();
                AsyncExecutionService async = bot.Services.GetRequiredService<AsyncExecutionService>();

                IReadOnlyList<(RssFeed, SyndicationItem)> updates = async.Execute(rss.CheckAsync());

                var notFound = new List<RssSubscription>();
                foreach ((RssFeed feed, SyndicationItem latest) in updates)
                foreach (RssSubscription sub in feed.Subscriptions) {
                    if (!async.Execute(PeriodicTasksServiceExtensions.SendFeedUpdateAsync(bot, sub, latest)))
                        notFound.Add(sub);
                    async.Execute(Task.Delay(10));
                }

                Log.Debug("Feed check finished");

                if (notFound.Any()) {
                    Log.Information("404 subscriptions found. Removing {Count} subscriptions", notFound.Count);
                    async.Execute(rss.Subscriptions.RemoveAsync(notFound));
                }
            } catch (Exception e) {
                Log.Error(e, "An error occured during periodic feed processing");
            }
        } else {
            Log.Error("FeedCheckCallback failed to cast sender");
        }
    }

    private static void MiscellaneousActionsCallback(object? _)
    {
        if (_ is TheGodfatherBot bot) {
            try {
                List<Birthday> todayBirthdays;
                using (TheGodfatherDbContext db = bot.Database.CreateContext()) {
                    todayBirthdays = db.Birthdays
                        .AsQueryable()
                        .Where(b => b.Date.Month == DateTime.Now.Month && b.Date.Day == DateTime.Now.Day && b.LastUpdateYear < DateTime.Now.Year)
                        .ToList();
                }

                AsyncExecutionService async = bot.Services.GetRequiredService<AsyncExecutionService>();
                foreach (Birthday birthday in todayBirthdays) {
                    DiscordChannel channel = async.Execute(bot.Client.GetShard(birthday.GuildId).GetChannelAsync(birthday.ChannelId));
                    DiscordUser user = async.Execute(bot.Client.GetShard(birthday.GuildId).GetUserAsync(birthday.UserId));
                    async.Execute(channel.SendMessageAsync(user.Mention, new DiscordEmbedBuilder {
                        Description = $"{Emojis.Tada} Happy birthday, {user.Mention}! {Emojis.Cake}",
                        Color = DiscordColor.Aquamarine
                    }));

                    using TheGodfatherDbContext db = bot.Database.CreateContext();
                    birthday.LastUpdateYear = DateTime.Now.Year;
                    db.Birthdays.Update(birthday);
                    db.SaveChanges();
                }
                Log.Debug("Birthdays checked");

                using (TheGodfatherDbContext db = bot.Database.CreateContext()) {
                    switch (bot.Database.Provider) {
                        case DbProvider.PostgreSql:
                            db.Database.ExecuteSqlRaw("UPDATE gf.bank_accounts SET balance = GREATEST(CEILING(1.0015 * balance), 10);");
                            break;
                        case DbProvider.Sqlite:
                        case DbProvider.SqliteInMemory:
                            db.Database.ExecuteSqlRaw("UPDATE bank_accounts SET balance = MAX(ROUND(0.5 + 1.0015 * balance), 10);");
                            break;
                        case DbProvider.SqlServer:
                            // TODO
                            db.Database.ExecuteSqlRaw("UPDATE dbo.bank_accounts SET balance = MAX(CEILING(1.0015 * balance), 10);");
                            break;
                    }
                }
                Log.Debug("Currency updated for all users");

            } catch (Exception e) {
                Log.Error(e, "An error occured during misc timer callback");
            }
        } else {
            Log.Error("MiscellaneousActionsCallback failed to cast sender");
        }
    }

    private static void StarboardUpdateCallback(object? _)
    {
        if (_ is TheGodfatherBot bot) {
            try {
                LocalizationService lcs = bot.Services.GetRequiredService<LocalizationService>();
                StarboardService ss = bot.Services.GetRequiredService<StarboardService>();
                AsyncExecutionService async = bot.Services.GetRequiredService<AsyncExecutionService>();

                IReadOnlyDictionary<ulong, List<StarboardMessage>> updatedMessages = ss.GetUpdatedMessages();
                foreach ((ulong gid, List<StarboardMessage> toUpdate) in updatedMessages) {
                    if (!ss.IsStarboardEnabled(gid, out ulong starChannelId, out string emoji))
                        continue;

                    DiscordEmoji? starEmoji = null;
                    DiscordChannel? starChannel = null;
                    try {
                        starChannel = async.Execute(bot.Client.GetShard(gid).GetChannelAsync(starChannelId));
                        starEmoji = DiscordEmoji.FromName(bot.Client.GetShard(gid), emoji);
                    } catch (NotFoundException) {
                        LogExt.Debug(bot.GetId(gid), "Failed to fetch starboard config {ChannelId} for guild {GuildId}", starChannelId, gid);
                    }

                    if (starChannel is null || starEmoji is null) {
                        async.Execute(ss.ModifySettingsAsync(gid, null));
                        continue;
                    }

                    foreach (StarboardMessage updMsg in toUpdate) {
                        DiscordChannel? channel;
                        DiscordMessage? message = null;
                        DiscordMessage? starMessage = null;

                        try {
                            channel = async.Execute(bot.Client.GetShard(gid).GetChannelAsync(updMsg.ChannelId));
                            message = async.Execute(channel.GetMessageAsync(updMsg.MessageId));
                            updMsg.Stars = message?.GetReactionsCount(starEmoji) ?? 0;
                        } catch (NotFoundException) {
                            LogExt.Debug(bot.GetId(gid), "Failed to fetch message {MessageId} in channel {ChannelId} for guild {GuildId}",
                                updMsg.MessageId, updMsg.ChannelId, gid
                            );
                        }

                        if (message is null)
                            continue;

                        StarboardModificationResult res = async.Execute(ss.SyncWithDbAsync(updMsg));
                        Log.Debug("Starboard action for message {MessageId}: {StarboardModResult}", message.Id, res.ActionType);

                        if (res.Entry is not null && res.Entry.StarMessageId != 0)
                            try {
                                starMessage = async.Execute(starChannel.GetMessageAsync(res.Entry.StarMessageId));
                            } catch (NotFoundException) {
                                LogExt.Debug(bot.GetId(gid), "Failed to fetch starboard message {MessageId} in channel {ChannelId} for guild {GuildId}",
                                    res.Entry.StarMessageId, starChannelId, gid
                                );
                            }

                        try {
                            switch (res.ActionType) {
                                case StarboardActionType.Send:
                                    starMessage = async.Execute(
                                        starChannel.SendMessageAsync(message.ToStarboardEmbed(lcs, starEmoji, updMsg.Stars))
                                    );
                                    async.Execute(ss.AddStarboardLinkAsync(updMsg.GuildId, updMsg.ChannelId, updMsg.MessageId, starMessage.Id));
                                    Log.Debug("Sent starboard message {MessageId} to {Channel}", message.Id, starChannel);
                                    break;
                                case StarboardActionType.Delete:
                                    if (starMessage is not null)
                                        async.Execute(starMessage.DeleteAsync("_gf: Starboard - delete"));
                                    Log.Debug("Removed starboard {MessageId} from {Channel}", message.Id, starChannel);
                                    break;
                                case StarboardActionType.Modify:
                                    if (starMessage is null)
                                        async.Execute(starChannel.SendMessageAsync(message.ToStarboardEmbed(lcs, starEmoji, updMsg.Stars)));
                                    else
                                        async.Execute(starMessage.ModifyAsync(message.ToStarboardEmbed(lcs, starEmoji, updMsg.Stars)));
                                    Log.Debug("Modified/Resent starboard message {MessageId} in {Channel}", message.Id, starChannel);
                                    break;
                            }
                        } catch {
                            LoggingService ls = bot.Services.GetRequiredService<LoggingService>();
                            if (ls.IsLogEnabledFor(gid, out LocalizedEmbedBuilder emb)) {
                                emb.WithLocalizedDescription(TranslationKey.err_starboard_fail);
                                async.Execute(ls.LogAsync(message.Channel.Guild, emb));
                            }
                        }
                    }
                }

                Log.Debug("Starboards updated for all guilds. {Count} messages checked", updatedMessages.Count);
            } catch (Exception e) {
                Log.Error(e, "An error occured during starboard timer callback");
            }
        } else {
            Log.Error("StarboardUpdate failed to cast sender");
        }
    }
    #endregion

    #region Timers
    private Timer BotStatusUpdateTimer { get; }
    private Timer DatabaseSyncTimer { get; }
    private Timer FeedCheckTimer { get; }
    private Timer MiscActionsTimer { get; }
    private Timer StarboardTimer { get; }
    #endregion


    public PeriodicTasksService(TheGodfatherBot bot, BotConfig cfg)
    {
        this.BotStatusUpdateTimer = new Timer(BotActivityChangeCallback, bot, TimeSpan.FromSeconds(25), TimeSpan.FromMinutes(10));
        this.DatabaseSyncTimer = new Timer(XpSyncCallback, bot, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(cfg.DatabaseSyncInterval));
        this.FeedCheckTimer = new Timer(FeedCheckCallback, bot, TimeSpan.FromSeconds(cfg.FeedCheckStartDelay), TimeSpan.FromSeconds(cfg.FeedCheckInterval));
        this.MiscActionsTimer = new Timer(MiscellaneousActionsCallback, bot, TimeSpan.FromSeconds(35), TimeSpan.FromHours(12));
        this.StarboardTimer = new Timer(StarboardUpdateCallback, bot, TimeSpan.FromSeconds(45), TimeSpan.FromMinutes(3));
    }


    public void Dispose()
    {
        this.BotStatusUpdateTimer.Dispose();
        this.DatabaseSyncTimer.Dispose();
        this.FeedCheckTimer.Dispose();
        this.MiscActionsTimer.Dispose();
        this.StarboardTimer.Dispose();
    }
}