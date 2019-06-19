using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using DSharpPlus.Entities;
using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Reactions.Common;

namespace TheGodfather
{
    public sealed class SharedData : IDisposable
    {
        #region Properties
        public AsyncExecutor AsyncExecutor { get; }
        public ConcurrentHashSet<ulong> BlockedChannels { get; internal set; }
        public ConcurrentHashSet<ulong> BlockedUsers { get; internal set; }
        public BotConfig BotConfiguration { get; internal set; }
        public ConcurrentDictionary<ulong, CachedGuildConfig> GuildConfigurations { get; internal set; }
        public Logger LogProvider { get; internal set; }
        public bool IsBotListening { get; internal set; }
        public CancellationTokenSource MainLoopCts { get; internal set; }
        public bool StatusRotationEnabled { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentDictionary<int, SavedTaskExecutor>> RemindExecuters { get; internal set; }
        public ConcurrentDictionary<int, SavedTaskExecutor> TaskExecuters { get; internal set; }
        public UptimeInformation UptimeInformation { get; internal set; }

        private ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>> PendingResponses { get; }
        #endregion


        public SharedData()
        {
            this.AsyncExecutor = new AsyncExecutor();
            this.BlockedChannels = new ConcurrentHashSet<ulong>();
            this.BlockedUsers = new ConcurrentHashSet<ulong>();
            this.BotConfiguration = BotConfig.Default;
            this.GuildConfigurations = new ConcurrentDictionary<ulong, CachedGuildConfig>();
            this.IsBotListening = true;
            this.MainLoopCts = new CancellationTokenSource();
            this.PendingResponses = new ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>>();
            this.RemindExecuters = new ConcurrentDictionary<ulong, ConcurrentDictionary<int, SavedTaskExecutor>>();
            this.StatusRotationEnabled = true;
            this.TaskExecuters = new ConcurrentDictionary<int, SavedTaskExecutor>();
        }


        public void Dispose()
        {
            this.MainLoopCts.Dispose();
            foreach ((int tid, SavedTaskExecutor texec) in this.TaskExecuters)
                texec.Dispose();
        }


        #region Guild config methods
        public CachedGuildConfig GetGuildConfig(ulong gid)
            => this.GuildConfigurations.GetOrAdd(gid, CachedGuildConfig.Default);

        public string GetGuildPrefix(ulong gid)
        {
            if (this.GuildConfigurations.TryGetValue(gid, out CachedGuildConfig gcfg) && !string.IsNullOrWhiteSpace(gcfg.Prefix))
                return this.GuildConfigurations[gid].Prefix;
            else
                return this.BotConfiguration.DefaultPrefix;
        }

        public DiscordChannel GetLogChannelForGuild(DiscordGuild guild)
        {
            CachedGuildConfig gcfg = this.GetGuildConfig(guild.Id);
            return gcfg.LoggingEnabled ? guild.GetChannel(gcfg.LogChannelId) : null;
        }

        public void UpdateGuildConfig(ulong gid, Func<CachedGuildConfig, CachedGuildConfig> modifier)
            => this.GuildConfigurations[gid] = modifier(this.GuildConfigurations[gid]);
        #endregion

        #region Pending responses methods
        public void AddPendingResponse(ulong cid, ulong uid)
        {
            this.PendingResponses.AddOrUpdate(
                cid,
                new ConcurrentHashSet<ulong> { uid },
                (k, v) => { v.Add(uid); return v; }
            );
        }

        public bool IsResponsePending(ulong cid, ulong uid)
            => this.PendingResponses.TryGetValue(cid, out ConcurrentHashSet<ulong> pending) && pending.Contains(uid);

        public bool TryRemovePendingResponse(ulong cid, ulong uid)
        {
            if (!this.PendingResponses.TryGetValue(cid, out ConcurrentHashSet<ulong> pending))
                return true;

            bool success = pending.TryRemove(uid);
            if (!this.PendingResponses[cid].Any())
                this.PendingResponses.TryRemove(cid, out _);
            return success;
        }
        #endregion





        // TODO remove next
        public ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>> Filters { get; set; } = new ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>>();
        public bool MessageContainsFilter(ulong gid, string message)
        {
            if (!this.Filters.TryGetValue(gid, out ConcurrentHashSet<Filter> filters) || filters is null)
                return false;

            message = message.ToLowerInvariant();
            return filters.Any(f => f.Trigger.IsMatch(message));
        }
    }
}
