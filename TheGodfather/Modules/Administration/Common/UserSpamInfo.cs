#region USING_DIRECTIVES
using DSharpPlus.Entities;

using System;
using System.Collections.Concurrent;
using System.Threading;

using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class UserSpamInfo : IDisposable
    {
        public int Count => this.timers.Count;
        public TimeSpan ResetTimeSpan => TimeSpan.FromMinutes(1/*30*/);

        private readonly ConcurrentQueue<Timer> timers;
        private readonly object msgApplyLock;
        private readonly int maxMessages;

        private string lastContent;


        public UserSpamInfo(int maxMessages)
        {
            this.timers = new ConcurrentQueue<Timer>();
            this.msgApplyLock = new object();
            this.maxMessages = maxMessages;
            this.lastContent = string.Empty;
        }


        public bool ApplyMessage(DiscordMessage message)
        {
            lock (this.msgApplyLock) {
                string content = message.Content?.ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(content) && this.lastContent.LevenshteinDistance(content) > 2) {
                    this.lastContent = content;
                    this.Dispose();
                }
                var timer = new Timer(this.TimerCallback, null, this.ResetTimeSpan, this.ResetTimeSpan);
                this.timers.Enqueue(timer);
            }

            return this.Count <= this.maxMessages;
        }

        public void Dispose()
        {
            while (this.timers.TryDequeue(out Timer oldest))
                oldest.Dispose();
        }


        private void TimerCallback(object _)
        {
            if (this.timers.TryDequeue(out Timer oldest))
                oldest.Dispose();
        }
    }
}
