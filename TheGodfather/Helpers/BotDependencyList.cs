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
        internal FeedManager        FeedControl         { get; private set; }
        internal RankManager        RankControl         { get; private set; }
        internal GameStatsManager   GameStatsControl    { get; private set; }


        internal BotDependencyList(BotConfig cfg, DatabaseService db)
        {
            FeedControl = new FeedManager();
            RankControl = new RankManager();
            GameStatsControl = new GameStatsManager(db);
        }


        internal void LoadData()
        {
            FeedControl.Load();
            RankControl.Load();
        }

        internal void SaveData()
        {
            FeedControl.Save();
            RankControl.Save();
        }

        internal DependencyCollectionBuilder GetDependencyCollectionBuilder()
        {
            return new DependencyCollectionBuilder()
                .AddInstance(FeedControl)
                .AddInstance(RankControl)
                .AddInstance(GameStatsControl);
        }
    }
}
