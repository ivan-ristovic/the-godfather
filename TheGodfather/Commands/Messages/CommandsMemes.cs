#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Messages
{
    [Group("meme", CanInvokeWithoutSubcommand = true)]
    [Description("Contains some memes. When invoked without name, returns a random one.")]
    [Aliases("pic", "memes", "mm")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    public class CommandsMemes
    {
        #region STATIC_FIELDS
        private static ConcurrentDictionary<string, string> _memes = new ConcurrentDictionary<string, string>();
        private static bool _error = false;
        #endregion

        #region STATIC_FUNCTIONS
        public static void LoadMemes(DebugLogger log)
        {
            if (File.Exists("Resources/memes.json")) {
                try {
                    _memes = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(File.ReadAllText("Resources/memes.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Meme loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _error = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "memes.json is missing.", DateTime.Now);
            }
        }

        public static void SaveMemes(DebugLogger log)
        {
            if (_error) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Memes saving skipped until file conflicts are resolved!", DateTime.Now);
                return;
            }

            try {
                File.WriteAllText("Resources/memes.json", JsonConvert.SerializeObject(_memes));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "Meme save error. Details:\n" + e.ToString(), DateTime.Now);
                throw new IOException("Error while saving memes.");
            }
        }
        #endregion


        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [RemainingText, Description("Meme name.")] string name = null)
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


        #region COMMAND_MEME_ADD
        [Command("add")]
        [Description("Add a new meme to the list.")]
        [Aliases("+", "new")]
        [RequireOwner]
        public async Task AddMeme(CommandContext ctx,
                                 [Description("Short name (case insensitive).")] string name = null,
                                 [Description("URL.")] string url = null)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("Name or URL missing or invalid.");

            name = name.Trim().ToLower();
            url = url.Trim();

            if (_memes.ContainsKey(name)) {
                await ctx.RespondAsync("Meme with that name already exists!");
            } else {
                if (!_memes.TryAdd(name, url))
                    throw new CommandFailedException("Meme adding failed.");
                await ctx.RespondAsync($"Meme {Formatter.Bold(name)} successfully added!");
            }
        }
        #endregion

        #region COMMAND_MEME_DELETE
        [Command("delete")]
        [Description("Deletes a meme from list.")]
        [Aliases("-", "del", "remove")]
        [RequireOwner]
        public async Task DeleteMeme(CommandContext ctx, 
                                    [Description("Short name (case insensitive).")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");

            name = name.Trim().ToLower();
            if (!_memes.ContainsKey(name))
                throw new CommandFailedException("Meme with that name doesn't exist!", new KeyNotFoundException());

            if (!_memes.TryRemove(name, out _))
                throw new CommandFailedException("Meme removing failed.");
            await ctx.RespondAsync($"Meme {Formatter.Bold(name)} successfully removed!");
        }
        #endregion

        #region COMMAND_MEME_LIST
        [Command("list")]
        [Description("List all registered memes.")]
        public async Task List(CommandContext ctx, 
                              [Description("Page.")] int page = 1)
        {
            if (page < 1 || page > _memes.Count / 10 + 1)
                throw new CommandFailedException("No memes on that page.", new ArgumentOutOfRangeException());

            string s = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < _memes.Count ? starti + 10 : _memes.Count;
            var keys = _memes.Keys.Take(page * 10).ToArray();
            for (var i = starti; i < endi; i++)
                s += $"{Formatter.Bold(keys[i])} : {_memes[keys[i]]}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available memes (page {page}/{_memes.Count / 10 + 1}) :",
                Description = s,
                Color = DiscordColor.Green
            });
        }
        #endregion

        #region COMMAND_MEME_SAVE
        [Command("save")]
        [Description("Saves all the memes.")]
        [RequireOwner]
        public async Task SaveMemes(CommandContext ctx)
        {
            SaveMemes(ctx.Client.DebugLogger);
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
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder{ ImageUrl = url });
        }
        #endregion
    }
}
