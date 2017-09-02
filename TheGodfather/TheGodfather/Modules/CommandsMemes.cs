#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.IO;
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
                        if (line.Trim() == "" || line[0] == '#')
                            continue;
                        var values = line.Split('$');
                        string name = values[0];
                        if (!_memes.ContainsKey(name))
                            _memes.Add(name, values[1]);
                    }
                } catch (Exception) {
                    _memes.Clear();
                    return;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "memes.txt is missing.", DateTime.Now);
            }
        }
        #endregion


        public async Task ExecuteGroup(CommandContext ctx, [RemainingText, Description("Meme name")] string name = null)
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
            var embed = new DiscordEmbed() {
                Title = "Memes:"
            };

            foreach (var entry in _memes) {
                var embedfield = new DiscordEmbedField() {
                    Name = entry.Key,
                    Value = entry.Value
                };
                embed.Fields.Add(embedfield);
            }

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
            if (name == null || url == null || (name = name.Trim().ToLower()) == "" || (url = url.Trim()) == "") {
                await ctx.RespondAsync("Name or URL missing or invalid.");
                return;
            }

            if (_memes.ContainsKey(name)) {
                await ctx.RespondAsync("Meme with that name already exists!");
            } else {
                _memes.Add(name, url);
                await ctx.RespondAsync($"Meme '{name}' successfully added!");
            }
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
            } catch (Exception) {
                await ctx.RespondAsync("Error while saving memes.");
                return;
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

            var embed = new DiscordEmbed {
                Image = new DiscordEmbedImage {
                    Url = url
                }
            };
            await ctx.RespondAsync("", embed: embed);
        }
        #endregion
    }
}
