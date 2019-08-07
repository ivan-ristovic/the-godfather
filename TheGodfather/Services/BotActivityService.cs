using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using TheGodfather.Services.Common;

namespace TheGodfather.Services
{
    public sealed class BotActivityService : ITheGodfatherService, IDisposable
    {
        public bool IsDisabled => false;
        public bool IsBotListening {
            get => this.isBotListening;
            set {
                lock (this.lck)
                    this.isBotListening = value;
            }
        }
        public bool StatusRotationEnabled {
            get => this.statusRotationEnabled;
            set {
                lock (this.lck)
                    this.statusRotationEnabled = value;
            }
        }
        public CancellationTokenSource MainLoopCts { get; }
        public ImmutableDictionary<int, UptimeInformation> ShardUptimeInformation { get; }

        private bool statusRotationEnabled;
        private bool isBotListening;
        private readonly object lck = new object();


        public BotActivityService(int shardCount)
        {
            this.IsBotListening = true;
            this.MainLoopCts = new CancellationTokenSource();
            this.StatusRotationEnabled = true;
            var uptimeDict = new Dictionary<int, UptimeInformation>();
            for (int i = 0; i < shardCount; i++)
                uptimeDict.Add(i, new UptimeInformation(Process.GetCurrentProcess().StartTime));
            this.ShardUptimeInformation = uptimeDict.ToImmutableDictionary();
        }


        public bool ToggleListeningStatus()
        {
            lock (this.lck)
                this.IsBotListening = !this.IsBotListening;
            return this.IsBotListening;
        }

        public void Dispose()
        {
            this.MainLoopCts.Dispose();
        }
    }
}
