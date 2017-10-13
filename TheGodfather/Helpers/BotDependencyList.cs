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
    internal class BotDependencyList
    {
        internal AliasTable Aliases { get; private set; }


        internal BotDependencyList()
        {
            Aliases = new AliasTable();
        }


        internal void LoadData(DebugLogger log)
        {
            Aliases.Load(log);
        }

        internal void SaveData(DebugLogger log)
        {
            Aliases.Save(log);
        }

        internal DependencyCollectionBuilder GetDependencyCollectionBuilder()
        {
            return new DependencyCollectionBuilder()
                .AddInstance(Aliases);
        }
    }
}
