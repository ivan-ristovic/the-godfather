#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

using TheGodfather.Helpers;
using TheGodfather.Services;
using TheGodfather.Helpers.DataManagers;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers
{
    public class BotDependencyList
    {
        internal GameStatsManager   GameStatsControl    { get; private set; }


        internal BotDependencyList(BotConfig cfg, DatabaseService db)
        {
            GameStatsControl = new GameStatsManager(db);
        }


        internal DependencyCollectionBuilder GetDependencyCollectionBuilder()
        {
            return new DependencyCollectionBuilder()
                .AddInstance(GameStatsControl);
        }
    }
}
