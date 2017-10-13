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

namespace TheGodfather.Helpers
{
    public class StatusManager
    {
        public IReadOnlyList<string> Statuses => _statuses;
        private List<string> _statuses = new List<string> { "!help", "worldmafia.net", "worldmafia.net/discord" };


        public StatusManager()
        {

        }


        public void Load(DebugLogger log)
        {
            // TODO
        }

        public bool Save(DebugLogger log)
        {
            // TODO
            return true;
        }

        public void AddStatus(string status)
        {
            if (_statuses.Contains(status))
                return;
            _statuses.Add(status);
        }

        public void DeleteStatus(string status)
        {
            _statuses.RemoveAll(s => s.ToLower() == status.ToLower());
        }
        
        public string GetRandomStatus()
        {
            return Statuses[new Random().Next(Statuses.Count)];
        }
    }
}
