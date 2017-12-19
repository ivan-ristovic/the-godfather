#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

namespace TheGodfather.Commands.Messages
{
    [Group("filter", CanInvokeWithoutSubcommand = false)]
    [Description("Message filtering commands.")]
    [Aliases("f", "filters")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public class CommandsFilter
    {
        #region COMMAND_FILTER_ADD
        [Command("add")]
        [Description("Add filter to guild filter list.")]
        [Aliases("+", "new", "a")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [RemainingText, Description("Filter word. Can be a regex (case insensitive).")] string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                throw new InvalidCommandUsageException("Filter trigger missing.");

            if (ctx.Dependencies.GetDependency<GuildConfigManager>().TriggerExists(ctx.Guild.Id, filter))
                throw new CommandFailedException("You cannot add a filter if a trigger for that trigger exists!");

            if (filter.Contains("%") || filter.Length < 3 || filter.Length > 60)
                throw new CommandFailedException($"Filter must not contain {Formatter.Bold("%")} or have less than 3 characters and not more than 60 characters.");

            var regex = new Regex($"^{filter}$", RegexOptions.IgnoreCase);

            if (ctx.Client.GetCommandsNext().RegisteredCommands.Any(kv => regex.Match(kv.Key).Success))
                throw new CommandFailedException("You cannot add a filter that matches one of the commands!");
            
            if (ctx.Dependencies.GetDependency<GuildConfigManager>().TryAddGuildFilter(ctx.Guild.Id, regex))
                await ctx.RespondAsync($"Filter successfully added.").ConfigureAwait(false);
            else
                throw new CommandFailedException("Filter already exists!");
        }
        #endregion
        
        #region COMMAND_FILTER_DELETE
        [Command("delete")]
        [Description("Remove filter from guild filter list.")]
        [Aliases("-", "remove", "del")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx, 
                                     [Description("Index in list.")] int i)
        {
            if (ctx.Dependencies.GetDependency<GuildConfigManager>().TryRemoveGuildFilter(ctx.Guild.Id, i))
                await ctx.RespondAsync("Filter successfully removed.").ConfigureAwait(false);
            else
                throw new CommandFailedException("Filter at that index does not exist.");
        }
        #endregion
        
        #region COMMAND_FILTER_LIST
        [Command("list")]
        [Description("Show all filters for this guild.")]
        [Aliases("ls", "l")]
        public async Task ListAsync(CommandContext ctx, 
                                   [Description("Page")] int page = 1)
        {
            var filters = ctx.Dependencies.GetDependency<GuildConfigManager>().GetAllGuildFilters(ctx.Guild.Id);

            if (filters == null) {
                await ctx.RespondAsync("No filters registered.");
                return;
            }

            if (page < 1 || page > filters.Count / 20 + 1)
                throw new CommandFailedException("No filters on that page.");

            string desc = "";
            int starti = (page - 1) * 20;
            int endi = starti + 10 < filters.Count ? starti + 20 : filters.Count;
            var pagefilters = filters.Take(page * 20).ToArray();
            for (var i = starti; i < endi; i++) {
                var filter = pagefilters[i].ToString();
                desc += $"{Formatter.Bold(i.ToString())} : {filter.Substring(1, filter.Length - 2)}\n";
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available filters (page {page}/{filters.Count / 20 + 1}) :",
                Description = desc,
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
            ctx.Dependencies.GetDependency<GuildConfigManager>().ClearGuildFilters(ctx.Guild.Id);
            await ctx.RespondAsync("All filters for this guild successfully removed.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
