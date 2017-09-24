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
    [Group("alias", CanInvokeWithoutSubcommand = true)]
    [Description("Alias handling commands.")]
    [Aliases("a", "aliases")]
    public class CommandsAlias
    {
        #region STATIC_FIELDS
        private static SortedDictionary<ulong, SortedDictionary<string, string>> _aliases = new SortedDictionary<ulong, SortedDictionary<string, string>>();
        #endregion

        #region STATIC_FUNCTIONS
        public static void LoadAliases(DebugLogger log)
        {
            log.LogMessage(LogLevel.Info, "TheGodfather", "Loading aliases...", DateTime.Now);
            if (File.Exists("Resources/aliases.txt")) {
                try {
                    var lines = File.ReadAllLines("Resources/aliases.txt");
                    foreach (string line in lines) {
                        if (line.Trim() == "" || line[0] == '#')
                            continue;
                        var values = line.Split('$');
                        ulong gid = ulong.Parse(values[0]);
                        if (!_aliases.ContainsKey(gid))
                            _aliases.Add(gid, new SortedDictionary<string, string>());
                        _aliases[gid].Add(values[1], values[2]);
                    }
                } catch (ArgumentException e) {
                    log.LogMessage(LogLevel.Warning, "TheGodfather", "Alias loading interrupted. Exception : " + e.ToString(), DateTime.Now);
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Warning, "TheGodfather", "Exception occured. Details : " + e.ToString(), DateTime.Now);
                    log.LogMessage(LogLevel.Warning, "TheGodfather", "Clearing aliases...", DateTime.Now);
                    _aliases.Clear();
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "aliases.txt is missing.", DateTime.Now);
            }
        }

        public static void SaveAliases(DebugLogger log)
        {
            log.LogMessage(LogLevel.Info, "TheGodfather", "Saving aliases...", DateTime.Now);
            try {
                List<string> aliaslist = new List<string>();

                foreach (var guild_aliases in _aliases)
                    foreach (var alias in guild_aliases.Value)
                        aliaslist.Add(guild_aliases.Key + "$" + alias.Key + "$" + alias.Value);

                File.WriteAllLines("Resources/aliases.txt", aliaslist);
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO Alias save error:" + e.ToString(), DateTime.Now);
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
                throw new Exception("Alias name is missing.");
            
            if (_aliases != null && _aliases.ContainsKey(ctx.Guild.Id) && _aliases[ctx.Guild.Id].ContainsKey(name))
                await ctx.RespondAsync(_aliases[ctx.Guild.Id][name]);
            else
                await ctx.RespondAsync("Unknown alias.");
        }
        

        #region COMMAND_ALIAS_ADD
        [Command("add")]
        [Description("Add alias to list.")]
        [Aliases("+", "new")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task AddAlias(CommandContext ctx,
                                  [Description("Alias name (case sensitive).")] string alias = null,
                                  [Description("Response")] string response = null)
        {
            if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(response))
                throw new ArgumentException("Alias name or response missing or invalid.");

            if (!_aliases.ContainsKey(ctx.Guild.Id))
                _aliases.Add(ctx.Guild.Id, new SortedDictionary<string, string>());

            alias = alias.ToLower();
            if (_aliases[ctx.Guild.Id].ContainsKey(alias)) {
                await ctx.RespondAsync("Alias already exists.");
            } else {
                _aliases[ctx.Guild.Id].Add(alias, response);
                await ctx.RespondAsync($"Alias **{alias}** successfully added.");
            }
        }
        #endregion

        #region COMMAND_ALIAS_DELETE
        [Command("delete")]
        [Description("Remove alias from list.")]
        [Aliases("-", "remove", "del")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task DeleteAlias(CommandContext ctx, [Description("Alias to remove.")] string alias = null)
        {
            if (string.IsNullOrWhiteSpace(alias))
                throw new ArgumentException("Alias name missing.");

            if (!_aliases.ContainsKey(ctx.Guild.Id))
                throw new KeyNotFoundException("No aliases recorded in this guild.");

            _aliases[ctx.Guild.Id].Remove(alias);
            await ctx.RespondAsync($"Alias **{alias}** successfully removed.");
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
        public async Task ListAliases(CommandContext ctx, [Description("Page")] int page = 1)
        {
            if (!_aliases.ContainsKey(ctx.Guild.Id)) {
                await ctx.RespondAsync("No aliases registered.");
                return;
            }

            if (page < 1 || page > _aliases[ctx.Guild.Id].Count / 10 + 1)
                throw new ArgumentException("No aliases on that page.");

            string s = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < _aliases[ctx.Guild.Id].Count ? starti + 10 : _aliases[ctx.Guild.Id].Count;
            var keys = _aliases[ctx.Guild.Id].Keys.Take(page * 10).ToArray();
            for (var i = starti; i < endi; i++)
                s += $"**{keys[i]}** : {_aliases[ctx.Guild.Id][keys[i]]}\n";

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
            if (_aliases.ContainsKey(ctx.Guild.Id))
                _aliases[ctx.Guild.Id].Clear();
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
