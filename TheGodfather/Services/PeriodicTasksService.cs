using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Misc.Services;
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
            if (_ is TheGodfatherBot shard) {
                if (shard.Client is null || shard.Client.CurrentUser is null) {
                    Log.Error("BotActivityChangeCallback detected null client/user - this should not happen but is not nececarily an error");
                    return;
                }

                BotActivityService bas = shard.Services.GetRequiredService<BotActivityService>();
                if (!bas.StatusRotationEnabled)
                    return;

                try {
                    BotStatus? status = bas.GetRandomStatus();
                    if (status is null)
                        Log.Warning("No extra bot statuses present in the database.");

                    DiscordActivity activity = status is { }
                        ? new DiscordActivity(status.Status, status.Activity)
                        : new DiscordActivity($"@{shard.Client?.CurrentUser.Username} help", ActivityType.Playing);

                    _async.Execute(shard.Client!.UpdateStatusAsync(activity));
                    Log.Debug("Changed bot status to {ActivityType} {ActivityName}", activity.ActivityType, activity.Name);
                } catch (Exception e) {
                    Log.Error(e, "An error occured during activity change");
                }
            } else {
                Log.Error("BotActivityChangeCallback failed to cast sender to TheGodfatherShard");
            }
        }

        private static void DatabaseSyncCallback(object? _)
        {
            if (_ is TheGodfatherBot shard) {
                if (shard.Client is null) {
                    Log.Error("DatabaseSyncCallback detected null client - this should not happen");
                    return;
                }

                try {
                    shard.Services.GetRequiredService<UserRanksService>().Sync();
                    Log.Debug("Database sync successful");
                } catch (Exception e) {
                    Log.Error(e, "An error occured during database sync");
                }
            } else {
                Log.Error("DatabaseSyncCallback failed to cast sender to TheGodfatherShard");
            }
        }

        private static void FeedCheckCallback(object? _)
        {
            if (_ is TheGodfatherBot shard) {
                if (shard.Client is null) {
                    Log.Error("FeedCheckCallback detected null client - this should not happen");
                    return;
                }

                Log.Debug("Feed check starting...");
                try {
                    RssFeedsService rss = shard.Services.GetRequiredService<RssFeedsService>();
                    IReadOnlyList<(RssFeed, SyndicationItem)>? updates = _async.Execute(rss.CheckAsync());

                    var notFound = new List<RssSubscription>();
                    foreach ((RssFeed feed, SyndicationItem latest) in updates) {
                        foreach (RssSubscription sub in feed.Subscriptions) {
                            if (!_async.Execute(PeriodicTasksServiceExtensions.SendFeedUpdateAsync(shard, sub, latest)))
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
                Log.Error("FeedCheckCallback failed to cast sender to TheGodfatherShard");
            }
        }

        private static void MiscellaneousActionsCallback(object? _)
        {
            if (_ is TheGodfatherBot shard) {
                if (shard.Client is null) {
                    Log.Error("MiscellaneousActionsCallback detected null client - this should not happen");
                    return;
                }

                try {
                    List<Birthday> todayBirthdays;
                    using (TheGodfatherDbContext db = shard.Database.CreateContext()) {
                        todayBirthdays = db.Birthdays
                            .Where(b => b.Date.Month == DateTime.Now.Month && b.Date.Day == DateTime.Now.Day && b.LastUpdateYear < DateTime.Now.Year)
                            .ToList();
                    }

                    foreach (Birthday birthday in todayBirthdays) {
                        DiscordChannel channel = _async.Execute(shard.Client.GetShard(birthday.GuildId).GetChannelAsync(birthday.ChannelId));
                        DiscordUser user = _async.Execute(shard.Client.GetShard(birthday.GuildId).GetUserAsync(birthday.UserId));
                        _async.Execute(channel.SendMessageAsync(user.Mention, embed: new DiscordEmbedBuilder {
                            Description = $"{Emojis.Tada} Happy birthday, {user.Mention}! {Emojis.Cake}",
                            Color = DiscordColor.Aquamarine
                        }));

                        using TheGodfatherDbContext db = shard.Database.CreateContext();
                        birthday.LastUpdateYear = DateTime.Now.Year;
                        db.Birthdays.Update(birthday);
                        db.SaveChanges();
                    }
                    Log.Debug("Birthdays checked");

                    using (TheGodfatherDbContext db = shard.Database.CreateContext()) {
                        switch (shard.Database.Provider) {
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
                Log.Error("MiscellaneousActionsCallback failed to cast sender to TheGodfatherShard");
            }
        }
        #endregion

        #region Timers
        private Timer BotStatusUpdateTimer { get; set; }
        private Timer DatabaseSyncTimer { get; set; }
        private Timer FeedCheckTimer { get; set; }
        private Timer MiscActionsTimer { get; set; }
        #endregion


        public PeriodicTasksService(TheGodfatherBot shard, BotConfig cfg)
        {
            this.BotStatusUpdateTimer = new Timer(BotActivityChangeCallback, shard, TimeSpan.FromSeconds(25), TimeSpan.FromMinutes(10));
            this.DatabaseSyncTimer = new Timer(DatabaseSyncCallback, shard, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(cfg.DatabaseSyncInterval));
            this.FeedCheckTimer = new Timer(FeedCheckCallback, shard, TimeSpan.FromSeconds(cfg.FeedCheckStartDelay), TimeSpan.FromSeconds(cfg.FeedCheckInterval));
            this.MiscActionsTimer = new Timer(MiscellaneousActionsCallback, shard, TimeSpan.FromSeconds(35), TimeSpan.FromHours(12));
        }


        public void Dispose()
        {
            this.BotStatusUpdateTimer.Dispose();
            this.DatabaseSyncTimer.Dispose();
            this.FeedCheckTimer.Dispose();
            this.MiscActionsTimer.Dispose();
        }
    }
}
