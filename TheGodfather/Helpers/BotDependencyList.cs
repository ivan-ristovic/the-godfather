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
        internal InsultManager      InsultControl       { get; private set; }
        internal RankManager        RankControl         { get; private set; }
        internal GameStatsManager   GameStatsControl    { get; private set; }
        internal GuildConfigManager GuildConfigControl  { get; private set; }
        internal GiphyService       GiphyService        { get; private set; }
        internal ImgurService       ImgurService        { get; private set; }
        internal JokesService       JokesService        { get; private set; }
        internal SteamService       SteamService        { get; private set; }
        internal YoutubeService     YoutubeService      { get; private set; }


        internal BotDependencyList(BotConfig cfg, DatabaseService db)
        {
            FeedControl = new FeedManager();
            InsultControl = new InsultManager();
            RankControl = new RankManager();
            GameStatsControl = new GameStatsManager(db);
            GuildConfigControl = new GuildConfigManager(cfg);
            GiphyService = new GiphyService(cfg.GiphyKey);
            ImgurService = new ImgurService(cfg.ImgurKey);
            SteamService = new SteamService(cfg.SteamKey);
            YoutubeService = new YoutubeService(cfg.YoutubeKey);
            JokesService = new JokesService();
        }


        internal void LoadData()
        {
            FeedControl.Load();
            InsultControl.Load();
            RankControl.Load();
            GuildConfigControl.Load();
        }

        internal void SaveData()
        {
            FeedControl.Save();
            InsultControl.Save();
            RankControl.Save();
            GuildConfigControl.Save();
        }

        internal DependencyCollectionBuilder GetDependencyCollectionBuilder()
        {
            return new DependencyCollectionBuilder()
                .AddInstance(FeedControl)
                .AddInstance(InsultControl)
                .AddInstance(RankControl)
                .AddInstance(GiphyService)
                .AddInstance(ImgurService)
                .AddInstance(SteamService)
                .AddInstance(YoutubeService)
                .AddInstance(JokesService)
                .AddInstance(GameStatsControl)
                .AddInstance(GuildConfigControl);
        }
    }
}
