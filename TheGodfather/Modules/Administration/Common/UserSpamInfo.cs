#region USING_DIRECTIVES

using System;
using System.Threading;

using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class UserSpamInfo
    {
        private static readonly TimeSpan _resetAfter = TimeSpan.FromHours(1);

        public int RemainingUses => Volatile.Read(ref this.remainingUses);
        public bool IsActive => DateTimeOffset.UtcNow <= this.resetsAt;

        private DateTimeOffset resetsAt;
        private int remainingUses;
        private readonly int maxAmount;
        private readonly object decrementLock;
        private string lastContent;


        public UserSpamInfo(int maxRepeats)
        {
            this.maxAmount = maxRepeats;
            this.remainingUses = maxRepeats;
            this.resetsAt = DateTimeOffset.UtcNow + _resetAfter;
            this.decrementLock = new object();
            this.lastContent = string.Empty;
        }


        public bool TryDecrementAllowedMessageCount(string newContent)
        {
            newContent = newContent.ToLowerInvariant();

            lock (this.decrementLock) {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                if (now >= this.resetsAt || (!string.IsNullOrWhiteSpace(newContent) && this.lastContent.LevenshteinDistance(newContent) > 2)) {
                    Interlocked.Exchange(ref this.remainingUses, this.maxAmount);
                    this.resetsAt = now + _resetAfter;
                    this.lastContent = newContent;
                }

                if (this.RemainingUses > 0)
                    Interlocked.Decrement(ref this.remainingUses);
            }

            return this.remainingUses > 0;
        }

        public void Reset()
        {
            this.resetsAt = DateTimeOffset.UtcNow;
        }
    }
}
