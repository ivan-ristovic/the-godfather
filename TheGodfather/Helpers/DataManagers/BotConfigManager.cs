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
    internal class BotConfigManager
    {
        internal BotConfig CurrentConfig { get; private set; }


        internal BotConfigManager()
        {

        }


        internal bool Load()
        {
            if (File.Exists("Resources/config.json")) {
                try {
                    CurrentConfig = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("Resources/config.json"));
                } catch (Exception e) {
                    Console.WriteLine("EXCEPTION OCCURED WHILE LOADING CONFIG FILE: " + Environment.NewLine + e.ToString());
                    return false;
                }
            } else {
                Console.WriteLine("config.json is missing!");
                return false;
            }

            return true;
        }
    }
}
