#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot
{
    [Group("meme", CanInvokeWithoutSubcommand = true)]
    [Description("Contains some memes. When invoked without name, returns a random one.")]
    [Aliases("pic", "memes", "mm")]
    public class CommandsMemes
    {
        #region STATIC_FIELDS
        private static Dictionary<string, string> _memes = new Dictionary<string, string>();
        #endregion
        
        #region STATIC_FUNCTIONS
        public static void LoadMemes(DebugLogger log)
        {
            if (File.Exists("memes.txt")) {
                try {
                    var lines = File.ReadAllLines("memes.txt");
                    foreach (string line in lines) {
                        if (string.IsNullOrWhiteSpace(line) || line[0] == '#')
                            continue;
                        var values = line.Split('$');
                        string name = values[0];
                        if (!_memes.ContainsKey(name))
                            _memes.Add(name, values[1]);
                    }
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Meme loading failed: " + e.ToString(), DateTime.Now);
                    _memes.Clear();
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "memes.txt is missing.", DateTime.Now);
            }
        }
        #endregion


        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                            [RemainingText, Description("Meme name")] string name = null)
        {
            if (name == null || (name = name.Trim().ToLower()) == "") {
                await ReturnRandomMeme(ctx);
            } else if (!_memes.ContainsKey(name)) {
                await ctx.RespondAsync("No meme registered with that name, here is a random one: ");
                await ReturnRandomMeme(ctx);
            } else {
                await SendMeme(ctx, _memes[name]);
            }
        }


        #region COMMAND_MEME_LIST
        [Command("list")]
        [Description("List all registered memes.")]
        public async Task List(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder() { Title = "Memes:" };
            foreach (var entry in _memes)
                embed.AddField(entry.Key, entry.Value);
            await ctx.RespondAsync("", embed: embed);
        }
        #endregion

        #region COMMAND_MEME_ADD
        [Command("add")]
        [Description("Add a new meme to the list.")]
        public async Task AddMeme(CommandContext ctx,
                                 [Description("Short name (case insensitive).")] string name = null,
                                 [Description("URL")] string url = null)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Name or URL missing or invalid.");

            name = name.Trim().ToLower();
            url = url.Trim();

            if (_memes.ContainsKey(name)) {
                await ctx.RespondAsync("Meme with that name already exists!");
            } else {
                _memes.Add(name, url);
                await ctx.RespondAsync($"Meme '{name}' successfully added!");
            }
        }
        #endregion

        #region COMMAND_MEME_DELETE
        [Command("delete")]
        [Description("Deletes a meme from list.")]
        [Aliases("del", "remove")]
        public async Task DeleteMeme(CommandContext ctx, [Description("Short name (case insensitive).")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name missing.");

            name = name.Trim().ToLower();
            if (!_memes.ContainsKey(name))
                throw new KeyNotFoundException("Meme with that name doesn't exist!");

            _memes.Remove(name);
            await ctx.RespondAsync($"Meme '{name}' successfully deleted!");
        }
        #endregion

        #region COMMAND_MEME_SAVE
        [Command("save")]
        [Description("Saves all the memes.")]
        [RequireOwner]
        public async Task SaveMemes(CommandContext ctx)
        {
            try {
                FileStream f = File.Open("memes.txt", FileMode.Create);
                f.Close();

                List<string> memelist = new List<string>();
                foreach (var entry in _memes)
                    memelist.Add(entry.Key + "$" + entry.Value);
                
                File.WriteAllLines("memes.txt", memelist);
            } catch (Exception e) {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "Meme save error: " + e.ToString(), DateTime.Now);
                throw new IOException("Error while saving memes.");
            }

            await ctx.RespondAsync("Memes successfully saved.");
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task ReturnRandomMeme(CommandContext ctx)
        {
            if (_memes.Count == 0) {
                await ctx.RespondAsync("No memes saved.");
                return;
            }
            var rnd = new Random();
            List<string> names = new List<string>(_memes.Keys);
            await SendMeme(ctx, _memes[names[rnd.Next(names.Count)]]);
        }
        
        private async Task SendMeme(CommandContext ctx, string url)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder{ Url = url });
        }
        #endregion
    }
}
