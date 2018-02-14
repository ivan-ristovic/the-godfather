#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public abstract class Game
    {
        private static ConcurrentDictionary<ulong, Game> _games = new ConcurrentDictionary<ulong, Game>();


        public static Game GetRunningGameInChannel(ulong cid)
            => _games.ContainsKey(cid) ? _games[cid] : null;

        public static bool RunningInChannel(ulong cid)
            => _games.ContainsKey(cid) && _games[cid] != null;

        public static void RegisterGameInChannel(Game game, ulong cid)
            => _games.AddOrUpdate(cid, game, (c, g) => game);

        public static void UnregisterGameInChannel(ulong cid)
        {
            if (!_games.ContainsKey(cid))
                return;
            if (!_games.TryRemove(cid, out _))
                _games[cid] = null;
        }


        protected Game(InteractivityExtension interactivity, DiscordChannel channel)
        {
            _interactivity = interactivity;
            _channel = channel;
        }


        public DiscordUser Winner { get; protected set; }
        public bool NoReply { get; protected set; }

        protected DiscordChannel _channel;
        protected InteractivityExtension _interactivity;

        public abstract Task RunAsync();
    }
}
