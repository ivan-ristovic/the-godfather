#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

using TheGodfather.Helpers.DataManagers;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers
{
    internal class BotDependencyList
    {
        internal AliasManager AliasControl { get; private set; }
        internal PrefixManager PrefixControl { get; private set; }
        internal StatusManager StatusControl { get; private set; }


        internal BotDependencyList()
        {
            AliasControl = new AliasManager();
            PrefixControl = new PrefixManager();
            StatusControl = new StatusManager();
        }


        internal void LoadData(DebugLogger log)
        {
            AliasControl.Load(log);
            PrefixControl.Load(log);
            StatusControl.Load(log);
        }

        internal void SaveData(DebugLogger log)
        {
            AliasControl.Save(log);
            PrefixControl.Save(log);
            StatusControl.Save(log);
        }

        internal DependencyCollectionBuilder GetDependencyCollectionBuilder()
        {
            return new DependencyCollectionBuilder()
                .AddInstance(AliasControl)
                .AddInstance(PrefixControl)
                .AddInstance(StatusControl);
        }
    }
}
