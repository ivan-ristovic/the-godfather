#region USING_DIRECTIVES
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Modules.Reactions.Common
{
    public class TextReaction : Reaction
    {
        public int RemainingUses => Volatile.Read(ref _remaining);
        public int MaxUses { get; }
        public DateTimeOffset ResetsAt { get; internal set; }

        private SemaphoreSlim UsageSemaphore { get; }
        private int _remaining;


        public TextReaction(string trigger, string response, bool is_regex_trigger = false)
            : base(trigger, response, is_regex_trigger)
        {
            _remaining = 1;
            MaxUses = 1;
            ResetsAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(15);
            UsageSemaphore = new SemaphoreSlim(1, 1);
        }


        public async Task<bool> CanSendAsync()
        {
            await UsageSemaphore.WaitAsync()
                .ConfigureAwait(false);
            
            var now = DateTimeOffset.UtcNow;
            if (now >= ResetsAt) {
                Interlocked.Exchange(ref _remaining, MaxUses);
                ResetsAt = now + TimeSpan.FromSeconds(15);
            }
            
            var success = false;
            if (RemainingUses > 0) {
                Interlocked.Decrement(ref _remaining);
                success = true;
            }
            
            UsageSemaphore.Release();
            return success;
        }
    }
}
