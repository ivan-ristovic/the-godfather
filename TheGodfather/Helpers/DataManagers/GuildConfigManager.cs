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
    public class GuildConfigManager
    {
        #region PRIVATE_FIELDS
        private ConcurrentDictionary<ulong, GuildConfig> _gcfg = new ConcurrentDictionary<ulong, GuildConfig>();
        private BotConfig _cfg { get; set; }
        private bool _ioerr = false;
        #endregion


        public GuildConfigManager(BotConfig cfg)
        {
            _cfg = cfg;
        }


        #region LOAD/SAVE
        public void Load(DebugLogger log)
        {
            if (File.Exists("Resources/guilds.json")) {
                try {
                    _gcfg = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, GuildConfig>>(File.ReadAllText("Resources/guilds.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Guild config loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _ioerr = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "guilds.json is missing.", DateTime.Now);
            }
        }

        public bool Save(DebugLogger log)
        {
            if (_ioerr) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Guild info saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                File.WriteAllText("Resources/guilds.json", JsonConvert.SerializeObject(_gcfg, Formatting.Indented));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO Guild info save error. Details:\n" + e.ToString(), DateTime.Now);
                return false;
            }

            return true;
        }
        #endregion

        #region PREFIXES
        public string GetGuildPrefix(ulong gid)
        {
            if (_gcfg.ContainsKey(gid) && !string.IsNullOrWhiteSpace(_gcfg[gid].Prefix))
                return _gcfg[gid].Prefix;
            else
                return _cfg.DefaultPrefix;
        }

        public bool TrySetGuildPrefix(ulong gid, string prefix)
        {
            if (_gcfg.ContainsKey(gid)) {
                _gcfg[gid].Prefix = prefix;
                return true;
            } else {
                return _gcfg.TryAdd(gid, new GuildConfig() { Prefix = prefix });
            }
        }
        #endregion

        #region W/L channels
        public ulong GetGuildWelcomeChannelId(ulong gid)
        {
            if (_gcfg.ContainsKey(gid) && _gcfg[gid].WelcomeChannelId.HasValue)
                return _gcfg[gid].WelcomeChannelId.Value;
            else
                return 0;
        }

        public ulong GetGuildLeaveChannelId(ulong gid)
        {
            if (_gcfg.ContainsKey(gid) && _gcfg[gid].LeaveChannelId.HasValue)
                return _gcfg[gid].LeaveChannelId.Value;
            else
                return 0;
        }

        public bool TrySetGuildWelcomeChannelId(ulong gid, ulong cid)
        {
            if (_gcfg.ContainsKey(gid)) {
                _gcfg[gid].WelcomeChannelId = cid;
                return true;
            } else {
                return _gcfg.TryAdd(gid, new GuildConfig() { WelcomeChannelId = cid });
            }
        }

        public bool TrySetGuildLeaveChannelId(ulong gid, ulong cid)
        {
            if (_gcfg.ContainsKey(gid)) {
                _gcfg[gid].LeaveChannelId = cid;
                return true;
            } else {
                return _gcfg.TryAdd(gid, new GuildConfig() { LeaveChannelId = cid });
            }
        }

        public void RemoveGuildWelcomeChannel(ulong gid)
        {
            if (_gcfg.ContainsKey(gid))
                _gcfg[gid].WelcomeChannelId = null;
        }

        public void RemoveGuildLeaveChannel(ulong gid)
        {
            if (_gcfg.ContainsKey(gid))
                _gcfg[gid].LeaveChannelId = null;
        }
        #endregion

        #region TRIGGERS
        public IReadOnlyDictionary<string, string> GetAllGuildTriggers(ulong gid)
        {
            if (_gcfg.ContainsKey(gid) && _gcfg[gid].Triggers != null)
                return _gcfg[gid].Triggers;
            else
                return null;
        }

        public bool TriggerExists(ulong gid, string trigger)
        {
            return _gcfg.ContainsKey(gid) && _gcfg[gid].Triggers != null && _gcfg[gid].Triggers.ContainsKey(trigger);
        }

        public string GetResponseForTrigger(ulong gid, string trigger)
        {
            trigger = trigger.ToLower();
            if (TriggerExists(gid, trigger))
                return _gcfg[gid].Triggers[trigger];
            else
                return null;
        }

        public bool TryAddTrigger(ulong gid, string trigger, string response)
        {
            trigger = trigger.ToLower();
            if (_gcfg.ContainsKey(gid)) {
                if (_gcfg[gid].Triggers == null)
                    _gcfg[gid].Triggers = new ConcurrentDictionary<string, string>();
            } else {
                if (!_gcfg.TryAdd(gid, new GuildConfig() { Triggers = new ConcurrentDictionary<string, string>() }))
                    return false;
            }

            return _gcfg[gid].Triggers.TryAdd(trigger, response);
        }

        public bool TryRemoveTrigger(ulong gid, string trigger)
        {
            if (!_gcfg.ContainsKey(gid) || !_gcfg[gid].Triggers.ContainsKey(trigger))
                return true;

            return _gcfg[gid].Triggers.TryRemove(trigger, out _);
        }

        public bool ClearGuildTriggers(ulong gid)
        {
            if (!_gcfg.ContainsKey(gid))
                return true;

            return _gcfg.TryRemove(gid, out _);
        }
        #endregion


        internal sealed class GuildConfig
        {
            [JsonProperty("Prefix")]
            public string Prefix { get; set; }
            
            [JsonProperty("WelcomeChannelId")]
            public ulong? WelcomeChannelId { get; set; }

            [JsonProperty("LeaveChannelId")]
            public ulong? LeaveChannelId { get; set; }
            
            [JsonProperty("Triggers")]
            public ConcurrentDictionary<string, string> Triggers { get; set; }
        }
    }
}
