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
    public class BankManager
    {
        public IReadOnlyDictionary<ulong, int> Accounts => _accounts;
        private static ConcurrentDictionary<ulong, int> _accounts = new ConcurrentDictionary<ulong, int>();
        private bool _ioerr = false;


        public BankManager()
        {

        }


        public void Load(DebugLogger log)
        {
            if (File.Exists("Resources/bankaccounts.json")) {
                try {
                    _accounts = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, int>>(File.ReadAllText("Resources/bankaccounts.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Bank accounts loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _ioerr = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "bankaccounts.json is missing.", DateTime.Now);
            }
        }

        public bool Save(DebugLogger log)
        {
            if (_ioerr) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Bank account saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                File.WriteAllText("Resources/bankaccounts.json", JsonConvert.SerializeObject(_accounts));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO Bank accounts save error. Details:\n" + e.ToString(), DateTime.Now);
                return false;
            }

            return true;
        }

        public bool OpenAccount(ulong uid)
        {
            return _accounts.TryAdd(uid, 25);
        }

        public bool RetrieveCredits(ulong uid, int ammount)
        {
            if (!_accounts.ContainsKey(uid) || _accounts[uid] < ammount)
                return false;
            _accounts[uid] -= ammount;
            return true;
        }

        public void IncreaseBalance(ulong uid, int ammount)
        {
            if (!_accounts.ContainsKey(uid))
                if (!_accounts.TryAdd(uid, 0))
                    return;
            _accounts[uid] += ammount;
        }
    }
}
