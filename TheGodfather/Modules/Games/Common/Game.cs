#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Helpers.Collections;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public abstract class Game
    {
        public DiscordUser Winner { get; protected set; }
        public bool NoReply { get; protected set; }

        protected DiscordClient _client;
        protected DiscordChannel _channel;


        public static bool RunningInChannel(ulong cid, ConcurrentDictionary<ulong, Game> games)
            => games != null && games.ContainsKey(cid) && games[cid] != null;

        public static void RegisterGameInChannel(Game game, ulong cid, ConcurrentDictionary<ulong, Game> games)
            => games.AddOrUpdate(cid, game, (c, g) => game);

        public static bool UnregisterGameInChannel(ulong cid, ConcurrentDictionary<ulong, Game> games)
            => games.ContainsKey(cid) == false || games.TryRemove(cid, out _);
    }
}
