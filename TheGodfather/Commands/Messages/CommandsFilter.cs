#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Services;
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
                                  [RemainingText, Description("Filter. Can be a regex (case insensitive).")] string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                throw new InvalidCommandUsageException("Filter trigger missing.");

            if (ctx.Dependencies.GetDependency<GuildConfigManager>().TriggerExists(ctx.Guild.Id, filter))
                throw new CommandFailedException("You cannot add a filter if a trigger for that trigger exists!");

            if (filter.Contains("%") || filter.Length < 3 || filter.Length > 60)
                throw new CommandFailedException($"Filter must not contain {Formatter.Bold("%")} or have less than 3 characters and not more than 60 characters.");

            Regex regex;
            try {
                regex = new Regex($@"\b{filter}\b", RegexOptions.IgnoreCase);
            } catch (ArgumentException e) {
                throw new CommandFailedException($"Invalid filter regex: {e.Message}");
            }

            if (ctx.Client.GetCommandsNext().RegisteredCommands.Any(kv => regex.Match(kv.Key).Success))
                throw new CommandFailedException("You cannot add a filter that matches one of the commands!");
            
            if (ctx.Dependencies.GetDependency<SharedData>().TryAddGuildFilter(ctx.Guild.Id, regex)) {
                await ctx.RespondAsync($"Filter successfully added.")
                    .ConfigureAwait(false);
                try {
                    await ctx.Dependencies.GetDependency<DatabaseService>().AddFilterAsync(ctx.Guild.Id, filter)
                        .ConfigureAwait(false);
                } catch (Npgsql.NpgsqlException e) {
                    throw new DatabaseServiceException(e);
                }
            } else {
                throw new CommandFailedException("Filter already exists!");
            }
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
            if (ctx.Dependencies.GetDependency<SharedData>().TryRemoveGuildFilter(ctx.Guild.Id, filter)) {
                await ctx.RespondAsync($"Filter successfully removed.")
                    .ConfigureAwait(false);
                try {
                    await ctx.Dependencies.GetDependency<DatabaseService>().DeleteFilterAsync(ctx.Guild.Id, filter)
                        .ConfigureAwait(false);
                } catch (Npgsql.NpgsqlException e) {
                    throw new DatabaseServiceException(e);
                }
            } else {
                throw new CommandFailedException("Given filter does not exist.");
            }
        }
        #endregion
        
        #region COMMAND_FILTER_LIST
        [Command("list")]
        [Description("Show all filters for this guild.")]
        [Aliases("ls", "l")]
        public async Task ListAsync(CommandContext ctx, 
                                   [Description("Page")] int page = 1)
        {
            IReadOnlyList<string> filters;
            try {
                filters = await ctx.Dependencies.GetDependency<DatabaseService>().GetFiltersForGuildAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
            } catch (Npgsql.NpgsqlException e) {
                throw new DatabaseServiceException(e);
            }

            if (filters == null || !filters.Any()) {
                await ctx.RespondAsync("No filters registered.")
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
            ctx.Dependencies.GetDependency<SharedData>().ClearGuildFilters(ctx.Guild.Id);
            await ctx.RespondAsync("All filters for this guild successfully removed.")
                .ConfigureAwait(false);
            try {
                await ctx.Dependencies.GetDependency<DatabaseService>().ClearGuildFiltersAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
            } catch (Npgsql.NpgsqlException e) {
                throw new DatabaseServiceException(e);
            }

        }
        #endregion
    }
}
