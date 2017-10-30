#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers.DataManagers
{
    public class ChannelManager
    {
        public IReadOnlyDictionary<ulong, ulong> WelcomeChannels => _welcomeChannelIds;
        public IReadOnlyDictionary<ulong, ulong> LeaveChannels => _leaveChannelIds;
        private ConcurrentDictionary<ulong, ulong> _welcomeChannelIds = new ConcurrentDictionary<ulong, ulong>();
        private ConcurrentDictionary<ulong, ulong> _leaveChannelIds = new ConcurrentDictionary<ulong, ulong>();
        private bool _ioerrWelcome = false;
        private bool _ioerrLeave = false;


        public ChannelManager()
        {

        }


        public void Load(DebugLogger log)
        {
            LoadWelcome(log);
            LoadLeave(log);
        }

        public void LoadWelcome(DebugLogger log)
        {
            if (File.Exists("Resources/welcomechannels.json")) {
                try {
                    _welcomeChannelIds = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, ulong>>(File.ReadAllText("Resources/welcomechannels.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "WelcomeChannels loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _ioerrWelcome = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "welcomechannels.json is missing.", DateTime.Now);
            }
        }

        public void LoadLeave(DebugLogger log)
        {
            if (File.Exists("Resources/leavechannels.json")) {
                try {
                    _welcomeChannelIds = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, ulong>>(File.ReadAllText("Resources/leavechannels.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "LeaveChannels loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _ioerrWelcome = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "leavechannels.json is missing.", DateTime.Now);
            }
        }

        public bool Save(DebugLogger log)
        {
            return SaveWelcome(log) && SaveLeave(log);
        }

        public bool SaveWelcome(DebugLogger log)
        {
            if (_ioerrWelcome) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "WelcomeChannels saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                File.WriteAllText("Resources/welcomechannels.json", JsonConvert.SerializeObject(_welcomeChannelIds, Formatting.Indented));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO WelcomeChannels save error. Details:\n" + e.ToString(), DateTime.Now);
                return false;
            }

            return true;
        }

        public bool SaveLeave(DebugLogger log)
        {
            if (_ioerrLeave) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "LeaveChannels saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                File.WriteAllText("Resources/leavechannels.json", JsonConvert.SerializeObject(_leaveChannelIds, Formatting.Indented));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO LeaveChannels save error. Details:\n" + e.ToString(), DateTime.Now);
                return false;
            }

            return true;
        }

        public bool TryAddWelcomeChannel(ulong gid, ulong cid)
        {
            if (!_welcomeChannelIds.ContainsKey(gid))
                return _welcomeChannelIds.TryAdd(gid, cid);

            _welcomeChannelIds[gid] = cid;
            return true;
        }

        public bool TryAddLeaveChannel(ulong gid, ulong cid)
        {
            if (!_leaveChannelIds.ContainsKey(gid))
                return _leaveChannelIds.TryAdd(gid, cid);

            _leaveChannelIds[gid] = cid;
            return true;
        }

        public bool TryRemoveWelcomeChannel(ulong gid)
        {
            if (!_welcomeChannelIds.ContainsKey(gid))
                return true;

            return _welcomeChannelIds.TryRemove(gid, out _);
        }

        public bool TryRemoveLeaveChannel(ulong gid)
        {
            if (!_leaveChannelIds.ContainsKey(gid))
                return true;

            return _leaveChannelIds.TryRemove(gid, out _);
        }

        public ulong GetWelcomeChannelId(ulong gid)
        {
            if (_welcomeChannelIds.ContainsKey(gid))
                return _welcomeChannelIds[gid];
            else
                return 0;
        }

        public ulong GetLeaveChannelId(ulong gid)
        {
            if (_leaveChannelIds.ContainsKey(gid))
                return _leaveChannelIds[gid];
            else
                return 0;
        }
    }
}
