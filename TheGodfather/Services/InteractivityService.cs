using System.Collections.Concurrent;
using System.Linq;
using TheGodfather.Common.Collections;

namespace TheGodfather.Services
{
    public sealed class InteractivityService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private readonly ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>> PendingResponses;


        public InteractivityService()
        {
            this.PendingResponses = new ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>>();
        }


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
    }
}
