#region USING_DIRECTIVES
using System;
using System.Threading;
#endregion

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class UserRatelimitInfo
    {
        private static readonly TimeSpan _resetAfter = TimeSpan.FromSeconds(5);

        public int RemainingUses => Volatile.Read(ref this.remainingUses);
        public bool IsActive => DateTimeOffset.UtcNow <= this.resetsAt;

        private DateTimeOffset resetsAt;
        private int remainingUses;
        private readonly int maxAmount;
        private readonly object decrementLock;


        public UserRatelimitInfo(int maxMessages)
        {
            this.maxAmount = maxMessages;
            this.remainingUses = maxMessages;
            this.resetsAt = DateTimeOffset.UtcNow + _resetAfter;
            this.decrementLock = new object();
        }


        public bool TryDecrementAllowedMessageCount()
        {
            bool success = false;

            lock (this.decrementLock) {
                var now = DateTimeOffset.UtcNow;
                if (now >= this.resetsAt) {
                    Interlocked.Exchange(ref this.remainingUses, this.maxAmount);
                    this.resetsAt = now + _resetAfter;
                }

                if (this.RemainingUses > 0) {
                    Interlocked.Decrement(ref this.remainingUses);
                    success = true;
                }
            }

            return success;
        }
    }
}
