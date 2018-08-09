#region USING_DIRECTIVES
using System.Collections.Concurrent;

using TheGodfather.Modules.Polls.Common;
#endregion

namespace TheGodfather.Modules.Polls.Services
{
    public static  class PollService
    {
        private static readonly ConcurrentDictionary<ulong, Poll> _polls = new ConcurrentDictionary<ulong, Poll>();


        public static Poll GetPollInChannel(ulong cid)
            => _polls.ContainsKey(cid) ? _polls[cid] : null;

        public static bool IsPollRunningInChannel(ulong cid)
            => _polls.ContainsKey(cid) && _polls[cid] != null;

        public static bool RegisterPollInChannel(Poll poll, ulong cid)
        {
            if (_polls.ContainsKey(cid)) {
                _polls[cid] = poll;
                return true;
            }

            return _polls.TryAdd(cid, poll);
        }

        public static void UnregisterPollInChannel(ulong cid)
        {
            if (!_polls.ContainsKey(cid))
                return;
            if (!_polls.TryRemove(cid, out _))
                _polls[cid] = null;
        }
    }
}
