#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfatherBot.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Commands.Messages
{
    [Group("alias", CanInvokeWithoutSubcommand = true)]
    [Description("Alias handling commands.")]
    [Aliases("a", "aliases")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    public class CommandsAlias
    {
        #region STATIC_FIELDS
        private static ConcurrentDictionary<ulong, ConcurrentDictionary<string, string>> _aliases = new ConcurrentDictionary<ulong, ConcurrentDictionary<string, string>>();
        private static bool _error = false;
        #endregion

        #region STATIC_FUNCTIONS
        public static void LoadAliases(DebugLogger log)
        {
            if (File.Exists("Resources/aliases.json")) {
                try {
                    _aliases = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, ConcurrentDictionary<string, string>>>(File.ReadAllText("Resources/aliases.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Alias loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _error = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "aliases.json is missing.", DateTime.Now);
            }
        }

        public static void SaveAliases(DebugLogger log)
        {
            if (_error) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Alias saving skipped until file conflicts are resolved!", DateTime.Now);
                return;
            }

            try {
                File.WriteAllText("Resources/aliases.json", JsonConvert.SerializeObject(_aliases));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO Alias save error. Details:\n" + e.ToString(), DateTime.Now);
                throw new IOException("IO error while saving aliases.");
            }
        }

        public static string FindAlias(ulong gid, string trigger)
        {
            trigger = trigger.ToLower();
            if (_aliases.ContainsKey(gid) && _aliases[gid].ContainsKey(trigger))
                return _aliases[gid][trigger];
            else
                return null;
        }
        #endregion

        
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [RemainingText, Description("Alias name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Alias name is missing.");

            if (_aliases != null && _aliases.ContainsKey(ctx.Guild.Id) && _aliases[ctx.Guild.Id].ContainsKey(name)) {
                var split = _aliases[ctx.Guild.Id][name].Split(new string[] { "%user%" }, StringSplitOptions.None);
                await ctx.RespondAsync(string.Join(ctx.User.Mention, split));
            } else {
                await ctx.RespondAsync("Unknown alias.");
            }
        }
        

        #region COMMAND_ALIAS_ADD
        [Command("add")]
        [Description("Add alias to list.")]
        [Aliases("+", "new")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task AddAlias(CommandContext ctx,
                                  [Description("Alias name (case sensitive).")] string alias = null,
                                  [RemainingText, Description("Response.")] string response = null)
        {
            if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(response))
                throw new InvalidCommandUsageException("Alias name or response missing or invalid.");

            if (!_aliases.ContainsKey(ctx.Guild.Id))
                if (!_aliases.TryAdd(ctx.Guild.Id, new ConcurrentDictionary<string, string>()))
                    throw new CommandFailedException("Adding alias failed.");

            alias = alias.ToLower();
            if (_aliases[ctx.Guild.Id].ContainsKey(alias)) {
                await ctx.RespondAsync("Alias already exists.");
            } else {
                if (!_aliases[ctx.Guild.Id].TryAdd(alias, response))
                    throw new CommandFailedException("Adding alias failed.");
                await ctx.RespondAsync($"Alias {Formatter.Bold(alias)} successfully added.");
            }
        }
        #endregion

        #region COMMAND_ALIAS_DELETE
        [Command("delete")]
        [Description("Remove alias from list.")]
        [Aliases("-", "remove", "del")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task DeleteAlias(CommandContext ctx, 
                                     [RemainingText, Description("Alias to remove.")] string alias = null)
        {
            if (string.IsNullOrWhiteSpace(alias))
                throw new InvalidCommandUsageException("Alias name missing.");

            if (!_aliases.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("No aliases recorded in this guild.", new KeyNotFoundException());

            string response;
            if (!_aliases[ctx.Guild.Id].TryRemove(alias, out response))
                throw new CommandFailedException("Removing alias failed.");
            await ctx.RespondAsync($"Alias {Formatter.Bold(alias)} ({Formatter.Bold(response)}) successfully removed.");
        }
        #endregion

        #region COMMAND_ALIAS_SAVE
        [Command("save")]
        [Description("Save aliases to file.")]
        [RequireOwner]
        public async Task SaveAliases(CommandContext ctx)
        {
            SaveAliases(ctx.Client.DebugLogger);
            await ctx.RespondAsync("Aliases successfully saved.");
        }
        #endregion

        #region COMMAND_ALIAS_LIST
        [Command("list")]
        [Description("Show all aliases.")]
        public async Task ListAliases(CommandContext ctx, 
                                     [Description("Page.")] int page = 1)
        {
            if (!_aliases.ContainsKey(ctx.Guild.Id)) {
                await ctx.RespondAsync("No aliases registered.");
                return;
            }

            if (page < 1 || page > _aliases[ctx.Guild.Id].Count / 10 + 1)
                throw new CommandFailedException("No aliases on that page.");

            string s = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < _aliases[ctx.Guild.Id].Count ? starti + 10 : _aliases[ctx.Guild.Id].Count;
            var keys = _aliases[ctx.Guild.Id].Keys.Take(page * 10).ToArray();
            for (var i = starti; i < endi; i++)
                s += $"{Formatter.Bold(keys[i])} : {_aliases[ctx.Guild.Id][keys[i]]}\n";

            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = $"Available aliases (page {page}/{_aliases[ctx.Guild.Id].Count / 10 + 1}) :",
                Description = s,
                Color = DiscordColor.Green
            });
        }
        #endregion

        #region COMMAND_ALIAS_CLEAR
        [Command("clear")]
        [Description("Delete all aliases for the current guild.")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAliases(CommandContext ctx)
        {
            ConcurrentDictionary<string, string> guild_aliases;
            if (_aliases.ContainsKey(ctx.Guild.Id))
                if (!_aliases.TryRemove(ctx.Guild.Id, out guild_aliases))
                    throw new CommandFailedException("Clearing guild aliases failed");
            await ctx.RespondAsync("All aliases successfully removed.");
        }
        #endregion

        #region COMMAND_ALIAS_CLEARALL
        [Command("clearall")]
        [Description("Delete all aliases stored for ALL guilds.")]
        [RequireOwner]
        public async Task ClearAllAliases(CommandContext ctx)
        {
            _aliases.Clear();
            await ctx.RespondAsync("All aliases successfully removed.");
        }
        #endregion
    }
}
