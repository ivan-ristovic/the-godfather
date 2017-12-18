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
        internal BankManager        BankControl         { get; private set; }
        internal FeedManager        FeedControl         { get; private set; }
        internal InsultManager      InsultControl       { get; private set; }
        internal MemeManager        MemeControl         { get; private set; }
        internal RankManager        RankControl         { get; private set; }
        internal StatusManager      StatusControl       { get; private set; }
        internal SwatServerManager  SwatServerControl   { get; private set; }
        internal GameStatsManager   GameStatsControl    { get; private set; }
        internal GuildConfigManager GuildConfigControl  { get; private set; }
        internal DatabaseService    DatabaseService     { get; private set; }
        internal GiphyService       GiphyService        { get; private set; }
        internal ImgurService       ImgurService        { get; private set; }
        internal JokesService       JokesService        { get; private set; }
        internal SteamService       SteamService        { get; private set; }
        internal YoutubeService     YoutubeService      { get; private set; }


        internal BotDependencyList(BotConfig cfg)
        {
            DatabaseService = new DatabaseService(cfg.DatabaseConfig);

            BankControl = new BankManager();
            FeedControl = new FeedManager();
            InsultControl = new InsultManager();
            MemeControl = new MemeManager();
            RankControl = new RankManager();
            StatusControl = new StatusManager();
            SwatServerControl = new SwatServerManager();
            GameStatsControl = new GameStatsManager(DatabaseService);
            GuildConfigControl = new GuildConfigManager(cfg);
            GiphyService = new GiphyService(cfg.GiphyKey);
            ImgurService = new ImgurService(cfg.ImgurKey);
            SteamService = new SteamService(cfg.SteamKey);
            YoutubeService = new YoutubeService(cfg.YoutubeKey);
            JokesService = new JokesService();
        }


        internal void LoadData()
        {
            BankControl.Load();
            FeedControl.Load();
            InsultControl.Load();
            MemeControl.Load();
            RankControl.Load();
            StatusControl.Load();
            SwatServerControl.Load();
            GuildConfigControl.Load();
        }

        internal void SaveData()
        {
            BankControl.Save();
            FeedControl.Save();
            InsultControl.Save();
            MemeControl.Save();
            RankControl.Save();
            StatusControl.Save();
            SwatServerControl.Save();
            GuildConfigControl.Save();
        }

        internal DependencyCollectionBuilder GetDependencyCollectionBuilder()
        {
            return new DependencyCollectionBuilder()
                .AddInstance(BankControl)
                .AddInstance(FeedControl)
                .AddInstance(InsultControl)
                .AddInstance(MemeControl)
                .AddInstance(RankControl)
                .AddInstance(StatusControl)
                .AddInstance(SwatServerControl)
                .AddInstance(DatabaseService)
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
