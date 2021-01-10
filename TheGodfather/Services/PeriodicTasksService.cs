using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;
using TheGodfather.Misc.Services;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Modules.Misc.Extensions;
using TheGodfather.Modules.Misc.Services;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services.Common;
using TheGodfather.Services.Extensions;

namespace TheGodfather.Services
{
    public class PeriodicTasksService : IDisposable
    {
        private static readonly AsyncExecutionService _async = new AsyncExecutionService();

        #region Callbacks
        private static void BotActivityChangeCallback(object? _)
        {
            if (_ is TheGodfatherBot bot) {
                if (bot.Client is null || bot.Client.CurrentUser is null) {
                    Log.Error("BotActivityChangeCallback detected null client/user - this should not happen but is not nececarily an error");
                    return;
                }

                BotActivityService bas = bot.Services.GetRequiredService<BotActivityService>();
                if (!bas.StatusRotationEnabled)
                    return;

                try {
                    BotStatus? status = bas.GetRandomStatus();
                    if (status is null)
                        Log.Warning("No extra bot statuses present in the database.");

                    DiscordActivity activity = status is { }
                        ? new DiscordActivity(status.Status, status.Activity)
                        : new DiscordActivity($"@{bot.Client?.CurrentUser.Username} help", ActivityType.Playing);

                    _async.Execute(bot.Client!.UpdateStatusAsync(activity));
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
                if (bot.Client is null) {
                    Log.Error("XpSyncCallback detected null client - this should not happen");
                    return;
                }

                try {
                    _async.Execute(bot.Services.GetRequiredService<UserRanksService>().Sync());
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
                if (bot.Client is null) {
                    Log.Error("FeedCheckCallback detected null client - this should not happen");
                    return;
                }

                Log.Debug("Feed check starting...");
                try {
                    RssFeedsService rss = bot.Services.GetRequiredService<RssFeedsService>();
                    IReadOnlyList<(RssFeed, SyndicationItem)>? updates = _async.Execute(rss.CheckAsync());

                    var notFound = new List<RssSubscription>();
                    foreach ((RssFeed feed, SyndicationItem latest) in updates) {
                        foreach (RssSubscription sub in feed.Subscriptions) {
                            if (!_async.Execute(PeriodicTasksServiceExtensions.SendFeedUpdateAsync(bot, sub, latest)))
                                notFound.Add(sub);
                            _async.Execute(Task.Delay(10));
                        }
                    }

                    Log.Debug("Feed check finished");

                    if (notFound.Any()) {
                        Log.Information("404 subscriptions found. Removing {0} subscriptions", notFound.Count);
                        _async.Execute(rss.Subscriptions.RemoveAsync(notFound));
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
                if (bot.Client is null) {
                    Log.Error("MiscellaneousActionsCallback detected null client - this should not happen");
                    return;
                }

                try {
                    List<Birthday> todayBirthdays;
                    using (TheGodfatherDbContext db = bot.Database.CreateContext()) {
                        todayBirthdays = db.Birthdays
                            .Where(b => b.Date.Month == DateTime.Now.Month && b.Date.Day == DateTime.Now.Day && b.LastUpdateYear < DateTime.Now.Year)
                            .ToList();
                    }

                    foreach (Birthday birthday in todayBirthdays) {
                        DiscordChannel channel = _async.Execute(bot.Client.GetShard(birthday.GuildId).GetChannelAsync(birthday.ChannelId));
                        DiscordUser user = _async.Execute(bot.Client.GetShard(birthday.GuildId).GetUserAsync(birthday.UserId));
                        _async.Execute(channel.SendMessageAsync(user.Mention, embed: new DiscordEmbedBuilder {
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
                                db.Database.ExecuteSqlRaw("UPDATE bank_accounts SET balance = GREATEST(CEILING(1.0015 * balance), 10);");
                                break;
                            case DbProvider.SqlServer:
                                db.Database.ExecuteSqlRaw("UPDATE dbo.bank_accounts SET balance = GREATEST(CEILING(1.0015 * balance), 10);");
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
                if (bot.Client is null) {
                    Log.Error("StarboardUpdate detected null client - this should not happen");
                    return;
                }

                try {
                    LocalizationService lcs = bot.Services.GetRequiredService<LocalizationService>();
                    StarboardService ss = bot.Services.GetRequiredService<StarboardService>();

                    foreach ((ulong gid, List<StarboardMessage> toUpdate) in ss.GetUpdatedMessages()) {
                        if (!ss.IsStarboardEnabled(gid, out ulong starChannelId, out string emoji))
                            continue;

                        DiscordEmoji? starEmoji = null;
                        DiscordChannel? starChannel = null;
                        try {
                            starChannel = _async.Execute(bot.Client.GetShard(gid).GetChannelAsync(starChannelId));
                            starEmoji = DiscordEmoji.FromName(bot.Client.GetShard(gid), emoji);
                        } catch (NotFoundException) {
                            LogExt.Debug(bot.GetId(gid), "Failed to fetch starboard config {ChannelId} for guild {GuildId}", starChannelId, gid);
                        }

                        int threshold = ss.GetMinimumStarCount(gid);

                        if (starChannel is null || starEmoji is null) {
                            // TODO disable starboard and clear all starboard messages from db
                            continue;
                        }

                        foreach (StarboardMessage updMsg in toUpdate) {
                            DiscordChannel? channel = null;
                            DiscordMessage? message = null;
                            DiscordMessage? starMessage = null;

                            try {
                                channel = _async.Execute(bot.Client.GetShard(gid).GetChannelAsync(updMsg.ChannelId));
                                message = _async.Execute(channel.GetMessageAsync(updMsg.MessageId));
                                updMsg.Stars = message?.GetReactionsCount(starEmoji) ?? 0;
                            } catch (NotFoundException) {
                                LogExt.Debug(bot.GetId(gid), "Failed to fetch message {MessageId} in channel {ChannelId} for guild {GuildId}",
                                    updMsg.MessageId, updMsg.ChannelId, gid
                                );
                            }

                            if (message is null)
                                continue;

                            StarboardModificationResult res = _async.Execute(ss.SyncWithDbAsync(updMsg));
                            if (res.Entry is { } && res.Entry.StarMessageId != 0) {
                                try {
                                    starMessage = _async.Execute(starChannel.GetMessageAsync(res.Entry.StarMessageId));
                                } catch (NotFoundException) {
                                    LogExt.Debug(bot.GetId(gid), "Failed to fetch starboard message {MessageId} in channel {ChannelId} for guild {GuildId}",
                                        res.Entry.StarMessageId, starChannelId, gid
                                    );
                                }
                            }

                            try {
                                switch (res.ActionType) {
                                    case StarboardActionType.Send:
                                        starMessage = _async.Execute(
                                            starChannel.SendMessageAsync(embed: message.ToStarboardEmbed(lcs, starEmoji, updMsg.Stars))
                                        );
                                        _async.Execute(ss.AddStarboardLinkAsync(updMsg.GuildId, updMsg.ChannelId, updMsg.MessageId, starMessage.Id));
                                        break;
                                    case StarboardActionType.Delete:
                                        if (starMessage is { })
                                            _async.Execute(starMessage.DeleteAsync("_gf: Starboard - delete"));
                                        break;
                                    case StarboardActionType.Modify:
                                        if (starMessage is null)
                                            _async.Execute(starChannel.SendMessageAsync(embed: message.ToStarboardEmbed(lcs, starEmoji, updMsg.Stars)));
                                        else
                                            _async.Execute(starMessage.ModifyAsync(embed: message.ToStarboardEmbed(lcs, starEmoji, updMsg.Stars)));
                                        break;
                                }
                            } catch {
                                // TODO
                            }
                        }
                    }

                    Log.Debug("Starboards updated for all guilds");
                } catch (Exception e) {
                    Log.Error(e, "An error occured during starboard timer callback");
                }
            } else {
                Log.Error("StarboardUpdate failed to cast sender");
            }
        }
        #endregion

        #region Timers
        private Timer BotStatusUpdateTimer { get; set; }
        private Timer DatabaseSyncTimer { get; set; }
        private Timer FeedCheckTimer { get; set; }
        private Timer MiscActionsTimer { get; set; }
        private Timer StarboardTimer { get; set; }
        #endregion


        public PeriodicTasksService(TheGodfatherBot bot, BotConfig cfg)
        {
            this.BotStatusUpdateTimer = new Timer(BotActivityChangeCallback, bot, TimeSpan.FromSeconds(25), TimeSpan.FromMinutes(10));
            this.DatabaseSyncTimer = new Timer(XpSyncCallback, bot, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(cfg.DatabaseSyncInterval));
            this.FeedCheckTimer = new Timer(FeedCheckCallback, bot, TimeSpan.FromSeconds(cfg.FeedCheckStartDelay), TimeSpan.FromSeconds(cfg.FeedCheckInterval));
            this.MiscActionsTimer = new Timer(MiscellaneousActionsCallback, bot, TimeSpan.FromSeconds(35), TimeSpan.FromHours(12));
            this.StarboardTimer = new Timer(StarboardUpdateCallback, bot, TimeSpan.FromSeconds(45), TimeSpan.FromMinutes(1));
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
}
