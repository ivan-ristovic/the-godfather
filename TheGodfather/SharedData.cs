using System;
using System.Collections.Concurrent;
using System.Threading;
using TheGodfather.Common;

namespace TheGodfather
{
    public sealed class SharedData : IDisposable
    {
        public bool IsBotListening { get; internal set; }
        public CancellationTokenSource MainLoopCts { get; internal set; }
        public bool StatusRotationEnabled { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentDictionary<int, SavedTaskExecutor>> RemindExecuters { get; internal set; }
        public ConcurrentDictionary<int, SavedTaskExecutor> TaskExecuters { get; internal set; }
        public UptimeInformation UptimeInformation { get; internal set; }


        public SharedData()
        {
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
    }
}
