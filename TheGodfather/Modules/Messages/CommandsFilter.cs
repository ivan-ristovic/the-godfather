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
    [Group("filter", CanInvokeWithoutSubcommand = false)]
    [Description("Message filtering commands.")]
    [Aliases("f", "filters")]
    public class CommandsFilter
    {
        #region STATIC_FIELDS
        private static SortedDictionary<ulong, List<string>> _filters = new SortedDictionary<ulong, List<string>>();
        #endregion

        #region STATIC_FUNCTIONS
        public static void LoadFilters(DebugLogger log)
        {
            log.LogMessage(LogLevel.Info, "TheGodfather", "Loading filters...", DateTime.Now);
            if (File.Exists("Resources/filters.txt")) {
                try {
                    var lines = File.ReadAllLines("Resources/filters.txt");
                    foreach (string line in lines) {
                        if (line.Trim() == "" || line[0] == '#')
                            continue;
                        var values = line.Split('$');
                        ulong gid = ulong.Parse(values[0]);
                        if (!_filters.ContainsKey(gid))
                            _filters.Add(gid, new List<string>());
                        _filters[gid].AddRange(values.Skip(1));
                    }
                } catch (ArgumentException e) {
                    log.LogMessage(LogLevel.Warning, "TheGodfather", "Filter loading interrupted. Exception : " + e.ToString(), DateTime.Now);
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Warning, "TheGodfather", "Exception occured. Details : " + e.ToString(), DateTime.Now);
                    log.LogMessage(LogLevel.Warning, "TheGodfather", "Clearing filters...", DateTime.Now);
                    _filters.Clear();
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "filters.txt is missing.", DateTime.Now);
            }
        }

        public static void SaveFilters(DebugLogger log)
        {
            log.LogMessage(LogLevel.Info, "TheGodfather", "Saving filters...", DateTime.Now);
            try {
                List<string> filterlist = new List<string>();

                foreach (var guild_filter in _filters) {
                    string line = guild_filter.Key.ToString();
                    foreach (var filter in guild_filter.Value)
                        line += "$" + filter;
                    filterlist.Add(line);
                }

                File.WriteAllLines("Resources/filters.txt", filterlist);
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO Filter save error:" + e.ToString(), DateTime.Now);
                throw new IOException("IO error while saving filters.");
            }
        }

        public static bool ContainsFilter(ulong gid, string message)
        {
            message = message.ToLower();
            if (_filters.ContainsKey(gid) && _filters[gid].Any(f => message.Contains(f)))
                return true;
            else
                return false;
        }
        #endregion

        
        #region COMMAND_FILTER_ADD
        [Command("add")]
        [Description("Add filter to list.")]
        [Aliases("+", "new")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task AddFilter(CommandContext ctx,
                                   [Description("Filter trigger word (case sensitive).")] string filter = null)
        {
            if (string.IsNullOrWhiteSpace(filter))
                throw new ArgumentException("Filter trigger missing.");

            if (!_filters.ContainsKey(ctx.Guild.Id))
                _filters.Add(ctx.Guild.Id, new List<string>());

            filter = filter.ToLower();
            if (_filters[ctx.Guild.Id].Contains(filter)) {
                await ctx.RespondAsync("Filter already exists.");
            } else {
                _filters[ctx.Guild.Id].Add(filter);
                await ctx.RespondAsync($"Filter **{filter}** successfully added.");
            }
        }
        #endregion
        
        #region COMMAND_FILTER_DELETE
        [Command("delete")]
        [Description("Remove filter from list.")]
        [Aliases("-", "remove", "del")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task DeleteFilter(CommandContext ctx, [Description("Filter to remove.")] string filter = null)
        {
            if (string.IsNullOrWhiteSpace(filter))
                throw new ArgumentException("Alias name missing.");

            if (!_filters.ContainsKey(ctx.Guild.Id))
                throw new KeyNotFoundException("No aliases recorded in this guild.");

            _filters[ctx.Guild.Id].Remove(filter);
            await ctx.RespondAsync($"Filter **{filter}** successfully removed.");
        }
        #endregion
        
        #region COMMAND_FILTER_SAVE
        [Command("save")]
        [Description("Save filters to file.")]
        [RequireOwner]
        public async Task SaveFilters(CommandContext ctx)
        {
            SaveFilters(ctx.Client.DebugLogger);
            await ctx.RespondAsync("Filters successfully saved.");
        }
        #endregion
        
        #region COMMAND_FILTER_LIST
        [Command("list")]
        [Description("Show all filters for this guild.")]
        public async Task ListAliases(CommandContext ctx, [Description("Page")] int page = 1)
        {
            if (!_filters.ContainsKey(ctx.Guild.Id)) {
                await ctx.RespondAsync("No filters registered.");
                return;
            }

            if (page < 1 || page > _filters[ctx.Guild.Id].Count / 10 + 1)
                throw new ArgumentException("No filters on that page.");

            string s = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < _filters[ctx.Guild.Id].Count ? starti + 10 : _filters[ctx.Guild.Id].Count;
            var filters = _filters[ctx.Guild.Id].Take(page * 10).ToArray();
            for (var i = starti; i < endi; i++)
                s += $"**{filters[i]}**\n";

            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = $"Available filters (page {page}/{_filters[ctx.Guild.Id].Count / 10 + 1}) :",
                Description = s,
                Color = DiscordColor.Green
            });
        }
        #endregion
        /*
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
        */
    }
}
