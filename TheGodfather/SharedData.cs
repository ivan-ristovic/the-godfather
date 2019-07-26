using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using DSharpPlus.Entities;
using TheGodfather.Common;
using TheGodfather.Common.Collections;

namespace TheGodfather
{
    public sealed class SharedData : IDisposable
    {
        #region Properties
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
        #endregion


        public SharedData()
        {
            this.BlockedChannels = new ConcurrentHashSet<ulong>();
            this.BlockedUsers = new ConcurrentHashSet<ulong>();
            this.BotConfiguration = BotConfig.Default;
            this.GuildConfigurations = new ConcurrentDictionary<ulong, CachedGuildConfig>();
            this.IsBotListening = true;
            this.MainLoopCts = new CancellationTokenSource();
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
            return this.GuildConfigurations.TryGetValue(gid, out CachedGuildConfig gcfg) && !string.IsNullOrWhiteSpace(gcfg.Prefix)
                ? this.GuildConfigurations[gid].Prefix
                : this.BotConfiguration.DefaultPrefix;
        }

        public DiscordChannel GetLogChannelForGuild(DiscordGuild guild)
        {
            CachedGuildConfig gcfg = this.GetGuildConfig(guild.Id);
            return gcfg.LoggingEnabled ? guild.GetChannel(gcfg.LogChannelId) : null;
        }

        public void UpdateGuildConfig(ulong gid, Func<CachedGuildConfig, CachedGuildConfig> modifier)
            => this.GuildConfigurations[gid] = modifier(this.GuildConfigurations[gid]);
        #endregion
    }
}
