#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
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
    [Group("filter", CanInvokeWithoutSubcommand = false)]
    [Description("Message filtering commands.")]
    [Aliases("f", "filters")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    public class CommandsFilter
    {
        #region STATIC_FIELDS
        private static SortedDictionary<ulong, List<Regex>> _filters = new SortedDictionary<ulong, List<Regex>>();
        private static bool _error = false;
        #endregion

        #region STATIC_FUNCTIONS
        public static void LoadFilters(DebugLogger log)
        {
            if (File.Exists("Resources/filters.json")) {
                try {
                    _filters = JsonConvert.DeserializeObject<SortedDictionary<ulong, List<Regex>>>(File.ReadAllText("Resources/filters.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Filter loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _error = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "filters.json is missing.", DateTime.Now);
            }
        }

        public static void SaveFilters(DebugLogger log)
        {
            if (_error) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Filter saving skipped until file conflicts are resolved!", DateTime.Now);
                return;
            }

            try {
                File.WriteAllText("Resources/filters.json", JsonConvert.SerializeObject(_filters));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO Filter save error. Details:\n" + e.ToString(), DateTime.Now);
                throw new IOException("IO error while saving filters.");
            }
        }

        public static bool ContainsFilter(ulong gid, string message)
        {
            message = message.ToLower();
            if (_filters.ContainsKey(gid) && _filters[gid].Any(f => f.Match(message).Success))
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
                                   [RemainingText, Description("Filter. Can be a regex (case insensitive).")] string filter = null)
        {
            if (string.IsNullOrWhiteSpace(filter))
                throw new InvalidCommandUsageException("Filter trigger missing.");
            
            if (CommandsAlias.FindAlias(ctx.Guild.Id, filter) != null)
                throw new CommandFailedException("You cannot add a filter if an alias for that trigger exists!");

            if (filter.Contains("%") || filter.Length < 3)
                throw new CommandFailedException($"Filter must not contain {Formatter.Bold("%")} or have less than 3 characters.");

            if (!_filters.ContainsKey(ctx.Guild.Id))
                _filters.Add(ctx.Guild.Id, new List<Regex>());

            var regex = new Regex($"^{filter}$", RegexOptions.IgnoreCase);

            if (ctx.Client.GetCommandsNext().RegisteredCommands.Any(kv => regex.Match(kv.Key).Success))
                throw new CommandFailedException("You cannot add a filter that matches one of the commands!");
            
            if (_filters[ctx.Guild.Id].Any(r => r.ToString() == regex.ToString())) {
                await ctx.RespondAsync($"Filter {Formatter.Bold(filter)} already exists.");
            } else {
                _filters[ctx.Guild.Id].Add(regex);
                await ctx.RespondAsync($"Filter {Formatter.Bold(filter)} successfully added.");
            }
        }
        #endregion
        
        #region COMMAND_FILTER_DELETE
        [Command("delete")]
        [Description("Remove filter from list.")]
        [Aliases("-", "remove", "del")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task DeleteFilter(CommandContext ctx, 
                                      [Description("Filter index.")] int i = 0)
        {
            if (!_filters.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("No filters recorded in this guild.", new KeyNotFoundException());

            if (i < 0 || i > _filters[ctx.Guild.Id].Count)
                throw new CommandFailedException("There is no filter with such index.", new ArgumentOutOfRangeException());

            _filters[ctx.Guild.Id].RemoveAt(i);
            await ctx.RespondAsync("Filter successfully removed.");
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
        public async Task ListFilters(CommandContext ctx, 
                                     [Description("Page")] int page = 1)
        {
            if (!_filters.ContainsKey(ctx.Guild.Id)) {
                await ctx.RespondAsync("No filters registered.");
                return;
            }

            if (page < 1 || page > _filters[ctx.Guild.Id].Count / 10 + 1)
                throw new CommandFailedException("No filters on that page.");

            string s = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < _filters[ctx.Guild.Id].Count ? starti + 10 : _filters[ctx.Guild.Id].Count;
            var filters = _filters[ctx.Guild.Id].Take(page * 10).ToArray();
            for (var i = starti; i < endi; i++) {
                var filter = filters[i].ToString();
                s += $"{Formatter.Bold(i.ToString())} : {filter.Substring(1, filter.Length - 2)}\n";
            }

            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = $"Available filters (page {page}/{_filters[ctx.Guild.Id].Count / 10 + 1}) :",
                Description = s,
                Color = DiscordColor.Green
            });
        }
        #endregion
        
        #region COMMAND_FILTERS_CLEAR
        [Command("clear")]
        [Description("Delete all filters for the current guild.")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearFilters(CommandContext ctx)
        {
            if (_filters.ContainsKey(ctx.Guild.Id))
                _filters.Remove(ctx.Guild.Id);
            await ctx.RespondAsync("All filters successfully removed.");
        }
        #endregion

        #region COMMAND_FILTER_CLEARALL
        [Command("clearall")]
        [Description("Delete all filters stored for ALL guilds.")]
        [RequireOwner]
        public async Task ClearAllFilters(CommandContext ctx)
        {
            _filters.Clear();
            await ctx.RespondAsync("All filters successfully removed.");
        }
        #endregion
    }
}
