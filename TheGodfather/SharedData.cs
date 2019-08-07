using System;
using System.Threading;
using TheGodfather.Common;

namespace TheGodfather
{
    public sealed class SharedData : IDisposable
    {
        public bool IsBotListening { get; internal set; }
        public CancellationTokenSource MainLoopCts { get; internal set; }
        public bool StatusRotationEnabled { get; internal set; }
        public UptimeInformation UptimeInformation { get; internal set; }


        public SharedData()
        {
            this.IsBotListening = true;
            this.MainLoopCts = new CancellationTokenSource();
            this.StatusRotationEnabled = true;
        }

        public void Dispose()
        {
            this.MainLoopCts.Dispose();
        }
    }
}
