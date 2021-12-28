using System.Threading;

namespace TheGodfather.Modules.Administration.Common;

public sealed class UserMentionInfo
{
    private static readonly TimeSpan _resetAfter = TimeSpan.FromMinutes(30);

    public int RemainingUses => Volatile.Read(ref this.remainingUses);
    public bool IsActive => DateTimeOffset.UtcNow <= this.resetsAt;

    private DateTimeOffset resetsAt;
    private int remainingUses;
    private readonly int maxAmount;
    private readonly object decrementLock;


    public UserMentionInfo(int maxMentions)
    {
        this.maxAmount = maxMentions;
        this.remainingUses = maxMentions;
        this.resetsAt = DateTimeOffset.UtcNow + _resetAfter;
        this.decrementLock = new object();
    }


    public bool TryDecrementAllowedMentionCount(int count)
    {
        lock (this.decrementLock) {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (now >= this.resetsAt) {
                Interlocked.Exchange(ref this.remainingUses, this.maxAmount);
                this.resetsAt = now + _resetAfter;
            }

            if (this.RemainingUses > 0)
                Interlocked.Add(ref this.remainingUses, -count);
        }

        return this.remainingUses > 0;
    }

    public void Reset()
    {
        this.resetsAt = DateTimeOffset.UtcNow;
    }
}