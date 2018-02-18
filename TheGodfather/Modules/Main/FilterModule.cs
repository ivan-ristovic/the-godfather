#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions.Collections;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Messages
{
    [Group("filter")]
    [Description("Message filtering commands. If invoked without subcommand, adds a new filter for the given word list. Words can be regular expressions.")]
    [Aliases("f", "filters")]
    [UsageExample("!filter fuck fk f+u+c+k+")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class FilterModule : GodfatherBaseModule
    {

        public FilterModule(SharedData shared, DatabaseService db) : base(shared, db) { }


        [GroupCommand]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Trigger word list.")] params string[] filters)
            => await AddAsync(ctx, filters).ConfigureAwait(false);


        #region COMMAND_FILTER_ADD
        [Command("add")]
        [Description("Add filter to guild filter list.")]
        [Aliases("+", "new", "a")]
        [UsageExample("!filter add fuck f+u+c+k+")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [RemainingText, Description("Filter. Can be a regex (case insensitive).")] params string[] filters)
        {
            if (filters == null || !filters.Any())
                throw new InvalidCommandUsageException("Filter words missing.");

            List<string> failed = new List<string>();
            foreach (var filter in filters) {
                if (filter.Contains('%') || filter.Length < 3 || filter.Length > 60 || SharedData.TextTriggerExists(ctx.Guild.Id, filter)) {
                    failed.Add(filter);
                    continue;
                }

                Regex regex;
                try {
                    regex = new Regex($@"\b{filter}\b", RegexOptions.IgnoreCase);
                } catch (ArgumentException) {
                    failed.Add(filter);
                    continue;
                }

                if (ctx.Client.GetCommandsNext().RegisteredCommands.Any(kvp => regex.IsMatch(kvp.Key))) {
                    failed.Add(filter);
                    continue;
                }

                if (SharedData.GuildFilters.ContainsKey(ctx.Guild.Id)) {
                    if (SharedData.GuildFilters[ctx.Guild.Id].Any(r => r.ToString() == regex.ToString())) {
                        failed.Add(filter);
                        continue;
                    }
                    SharedData.GuildFilters[ctx.Guild.Id].Add(regex);
                } else {
                    SharedData.GuildFilters.TryAdd(ctx.Guild.Id, new ConcurrentHashSet<Regex>() { regex });
                }

                await DatabaseService.AddFilterAsync(ctx.Guild.Id, filter)
                    .ConfigureAwait(false);
            }

            if (failed.Any())
                await ReplyWithEmbedAsync(ctx, $"Failed to add: {string.Join(", ", failed.Select(s => Formatter.Bold(s)))}.\n\nFilters must be valid regular expressions and cannot be added if they already exist or if they are longer than 60 or less than 3 characters. Also filters must not overlap an existing text reaction or bot command.", ":negative_squared_cross_mark:").ConfigureAwait(false);
            else
                await ReplyWithEmbedAsync(ctx).ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_FILTER_DELETE
        [Command("delete")]
        [Description("Remove filter from guild filter list.")]
        [Aliases("-", "remove", "del")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx, 
                                     [RemainingText, Description("Filter to remove.")] string filter)
        {
            if (!SharedData.GuildFilters.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no filters registered.");

            //var rstr = $@"\b{filter}\b";
            //GuildFilters[gid].RemoveWhere(r => r.ToString() == rstr) > 0;

            /*
            if (SharedData.TryRemoveGuildFilter(ctx.Guild.Id, filter)) {
                await ctx.RespondAsync($"Filter successfully removed.")
                    .ConfigureAwait(false);
                await ctx.Services.GetService<DatabaseService>().RemoveFilterAsync(ctx.Guild.Id, filter)
                    .ConfigureAwait(false);
            } else {
                throw new CommandFailedException("Given filter does not exist.");
            }*/
        }
        #endregion
        
        #region COMMAND_FILTER_LIST
        [Command("list")]
        [Description("Show all filters for this guild.")]
        [Aliases("ls", "l")]
        public async Task ListAsync(CommandContext ctx, 
                                   [Description("Page")] int page = 1)
        {
            var filters = await ctx.Services.GetService<DatabaseService>().GetFiltersForGuildAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            if (filters == null || !filters.Any()) {
                await ctx.RespondAsync("No filters registered for this guild.")
                    .ConfigureAwait(false);
                return;
            }

            if (page < 1 || page > filters.Count / 20 + 1)
                throw new CommandFailedException("No filters on that page.");
            
            int starti = (page - 1) * 20;
            int len = starti + 20 < filters.Count ? 20 : filters.Count - starti;

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available filters (page {page}/{filters.Count / 20 + 1}) :",
                Description = string.Join(", ", filters.OrderBy(v => v).ToList().GetRange(starti, len)),
                Color = DiscordColor.Green
            }.Build()).ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_FILTERS_CLEAR
        [Command("clear")]
        [Description("Delete all filters for the current guild.")]
        [Aliases("c", "da")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAsync(CommandContext ctx)
        {
            ctx.Services.GetService<SharedData>().ClearGuildFilters(ctx.Guild.Id);
            await ctx.RespondAsync("All filters for this guild successfully removed.")
                .ConfigureAwait(false);
            await ctx.Services.GetService<DatabaseService>().RemoveAllGuildFiltersAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
