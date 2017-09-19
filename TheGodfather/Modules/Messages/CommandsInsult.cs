#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Modules.Messages
{

    [Group("insult", CanInvokeWithoutSubcommand = true)]
    [Description("Burns a user!")]
    [Aliases("burn", "insults")]
    public class CommandsInsult
    {
        #region STATIC_FIELDS
        private static List<string> _insults = new List<string>();
        #endregion

        #region STATIC_FUNCTIONS
        public static void LoadInsults(DebugLogger log)
        {
            log.LogMessage(LogLevel.Info, "TheGodfather", "Loading insults...", DateTime.Now);
            if (File.Exists("Resources/insults.txt")) {
                try {
                    var lines = File.ReadAllLines("Resources/insults.txt");
                    foreach (string line in lines) {
                        if (line.Trim() == "" || line[0] == '#')
                            continue;
                        _insults.Add(line);
                    }
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Warning, "TheGodfather", "Exception occured, clearing insults. Details : " + e.ToString(), DateTime.Now);
                    _insults.Clear();
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "insults.txt is missing.", DateTime.Now);
            }
        }

        public static void SaveInsults(DebugLogger log)
        {
            log.LogMessage(LogLevel.Info, "TheGodfather", "Saving insults...", DateTime.Now);
            try {
                File.WriteAllLines("Resources/insults.txt", _insults);
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO insults save error:" + e.ToString(), DateTime.Now);
                throw new IOException("IO error while saving insults.");
            }
        }
        #endregion


        public async Task ExecuteGroupAsync(CommandContext ctx, [Description("User")] DiscordUser u = null)
        {
            if (u == null)
                u = ctx.User;

            if (_insults.Count == 0)
                throw new Exception("No available insults.");

            var rnd = new Random();
            var split = _insults[rnd.Next(_insults.Count)].Split('%');
            string response = split[0];
            for (int i = 1; i < split.Length; i++)
                response += u.Mention + split[i];
            await ctx.RespondAsync(response);
        }


        #region COMMAND_INSULTS_ADD
        [Command("add")]
        [Description("Add insult to list.")]
        [Aliases("+", "new")]
        public async Task AddInsult(CommandContext ctx,
                                   [RemainingText, Description("Response")] string insult = null)
        {
            if (string.IsNullOrWhiteSpace(insult))
                throw new ArgumentException("Missing insult string.");

            if (insult.Length >= 200)
                throw new ArgumentException("Too long insult. I know it is hard, but keep it shorter than 200 please.");

            if (insult.Split().Count() < 2)
                throw new ArgumentException("Insult not in correct format (missing %)!");

            _insults.Add(insult);
            await ctx.RespondAsync("Insult added.");
        }
        #endregion

        #region COMMAND_INSULTS_DELETE
        [Command("delete")]
        [Description("Remove insult with a given index from list. (use !insults list to view indexes)")]
        [Aliases("-", "remove", "del")]
        [RequireOwner]
        public async Task DeleteInsult(CommandContext ctx, [Description("Index")] int i = 0)
        {
            if (i < 0 || i > _insults.Count)
                throw new ArgumentException("There is no insult with such index.");

            _insults.RemoveAt(i);
            await ctx.RespondAsync("Insult successfully removed.");
        }
        #endregion

        #region COMMAND_INSULTS_SAVE
        [Command("save")]
        [Description("Save insults to file.")]
        [RequireOwner]
        public async Task SaveInsults(CommandContext ctx)
        {
            SaveInsults(ctx.Client.DebugLogger);
            await ctx.RespondAsync("Insults successfully saved.");
        }
        #endregion

        #region COMMAND_INSULTS_LIST
        [Command("list")]
        [Description("Show all insults.")]
        public async Task ListInsults(CommandContext ctx, [Description("Page")] int page = 1)
        {
            if (page < 1 || page > _insults.Count / 10 + 1)
                throw new ArgumentException("No insults on that page.");

            string s = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < _insults.Count ? starti + 10 : _insults.Count;
            for (int i = starti; i < endi; i++)
                s += "**" + i.ToString() + "** : " + _insults[i] + "\n";

            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = $"Available insults (page {page}) :",
                Description = s,
                Color = DiscordColor.Turquoise
            });
        }
        #endregion

        #region COMMAND_ALIAS_CLEAR
        [Command("clear")]
        [Description("Delete all insults.")]
        [RequireOwner]
        public async Task ClearAliases(CommandContext ctx)
        {
            _insults.Clear();
            await ctx.RespondAsync("All insults successfully removed.");
        }
        #endregion
    }
}
