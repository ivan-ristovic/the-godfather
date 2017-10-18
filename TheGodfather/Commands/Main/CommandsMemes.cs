#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Helpers.DataManagers;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Main
{
    [Group("meme", CanInvokeWithoutSubcommand = true)]
    [Description("Contains some memes. When invoked without name, returns a random one.")]
    [Aliases("pic", "memes", "mm")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    public class CommandsMemes
    {
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [RemainingText, Description("Meme name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name)) {
                await SendMeme(ctx, ctx.Dependencies.GetDependency<MemeManager>().GetRandomMeme());
                return;
            }

            string url = ctx.Dependencies.GetDependency<MemeManager>().GetUrl(name);
            if (url == null) {
                await ctx.RespondAsync("No meme registered with that name, here is a random one: ");
                await Task.Delay(200);
                await SendMeme(ctx, ctx.Dependencies.GetDependency<MemeManager>().GetRandomMeme());
            } else {
                await SendMeme(ctx, url);
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

            if (ctx.Dependencies.GetDependency<MemeManager>().TryAdd(name, url))
                await ctx.RespondAsync($"Meme {Formatter.Bold(name)} successfully added!");
            else
                await ctx.RespondAsync("Meme with that name already exists!");
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

            if (ctx.Dependencies.GetDependency<MemeManager>().TryRemove(name))
                await ctx.RespondAsync($"Meme {Formatter.Bold(name)} successfully removed!");
            else
                throw new CommandFailedException("Meme with that name doesn't exist!", new KeyNotFoundException());
        }
        #endregion

        #region COMMAND_MEME_LIST
        [Command("list")]
        [Description("List all registered memes.")]
        public async Task List(CommandContext ctx, 
                              [Description("Page.")] int page = 1)
        {
            var memes = ctx.Dependencies.GetDependency<MemeManager>().Memes;

            if (page < 1 || page > memes.Count / 10 + 1)
                throw new CommandFailedException("No memes on that page.", new ArgumentOutOfRangeException());

            string s = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < memes.Count ? starti + 10 : memes.Count;
            var keys = memes.Keys.Take(page * 10).ToArray();
            for (var i = starti; i < endi; i++)
                s += $"{Formatter.Bold(keys[i])} : {memes[keys[i]]}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available memes (page {page}/{memes.Count / 10 + 1}) :",
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
            if (ctx.Dependencies.GetDependency<MemeManager>().Save(ctx.Client.DebugLogger))
                await ctx.RespondAsync("Memes successfully saved.");
            else
                throw new CommandFailedException("Failed saving memes.", new IOException());
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task SendMeme(CommandContext ctx, string url)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder{ ImageUrl = url });
        }
        #endregion
    }
}
