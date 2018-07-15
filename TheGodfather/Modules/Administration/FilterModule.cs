#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Common.Collections;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services.Database.Filters;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("filter"), Module(ModuleType.Administration)]
    [Description("Message filtering commands. If invoked without subcommand, either lists all filters or adds a new filter for the given word list. Words can be regular expressions.")]
    [Aliases("f", "filters")]
    [UsageExamples("!filter fuck fk f+u+c+k+")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class FilterModule : TheGodfatherBaseModule
    {

        public FilterModule(SharedData shared, DBService db) : base(shared, db) { }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => ListAsync(ctx);

        [GroupCommand, Priority(0)]
        [RequirePermissions(Permissions.ManageGuild)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Filter list. Filter is a regular expression (case insensitive).")] params string[] filters)
            => AddAsync(ctx, filters);


        #region COMMAND_FILTER_ADD
        [Command("add"), Module(ModuleType.Administration)]
        [Description("Add filter to guild filter list.")]
        [Aliases("+", "new", "a")]
        [UsageExamples("!filter add fuck f+u+c+k+")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [RemainingText, Description("Filter list. Filter is a regular expression (case insensitive).")] params string[] filters)
        {
            if (filters == null || !filters.Any())
                throw new InvalidCommandUsageException("Filter regexes missing.");

            var errors = new StringBuilder();
            foreach (var filter in filters) {
                if (filter.Contains('%')) {
                    errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} cannot contain '%' character.");
                    continue;
                }

                if (filter.Length < 3 || filter.Length > 60) {
                    errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} doesn't fit the size requirement. Filters cannot be shorter than 3 and longer than 60 characters.");
                    continue;
                }

                if (Shared.GuildHasTextReaction(ctx.Guild.Id, filter)) {
                    errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} cannot be added because of a conflict with an existing text trigger in this guild.");
                    continue;
                }

                if (!TryParseRegex(filter, out var regex)) {
                    errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} is not a valid regular expression.");
                    continue;
                }

                if (ctx.Client.GetCommandsNext().RegisteredCommands.Any(kvp => regex.IsMatch(kvp.Key))) {
                    errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} collides with an existing bot command.");
                    continue;
                }

                if (Shared.Filters.ContainsKey(ctx.Guild.Id)) {
                    if (Shared.Filters[ctx.Guild.Id].Any(f => f.Trigger.ToString() == regex.ToString())) {
                        errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} already exists.");
                        continue;
                    }
                } else {
                    if (!Shared.Filters.TryAdd(ctx.Guild.Id, new ConcurrentHashSet<Filter>())) {
                        errors.AppendLine($"Error: Failed to create a filter data structure for this guild.");
                        continue;
                    }
                }

                int id = 0;
                try {
                    id = await Database.AddFilterAsync(ctx.Guild.Id, filter)
                        .ConfigureAwait(false);
                } catch (Exception e) {
                    Shared.LogProvider.LogException(LogLevel.Warning, e);
                    errors.AppendLine($"Warning: Failed to add filter {Formatter.Bold(filter)} to the database.");
                }

                if (id == 0 || !Shared.Filters[ctx.Guild.Id].Add(new Filter(id, regex)))
                    errors.AppendLine($"Error: Failed to add filter {Formatter.Bold(filter)}.");
            }

            string errlist = errors.ToString();
            var logchn = Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "New filters added",
                    Color = DiscordColor.DarkGreen
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Tried adding filters", string.Join("\n", filters));
                if (!string.IsNullOrWhiteSpace(errlist))
                    emb.AddField("With errors", errlist);
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }

            await ctx.InformSuccessAsync($"Done!\n\n{errlist}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_FILTERS_CLEAR
        [Command("clear"), Module(ModuleType.Administration)]
        [Description("Delete all filters for the current guild.")]
        [Aliases("da", "c", "ca", "cl", "clearall")]
        [UsageExamples("!filter clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        [UsesInteractivity]
        public async Task ClearAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all filters for this guild?").ConfigureAwait(false))
                return;

            if (Shared.Filters.ContainsKey(ctx.Guild.Id))
                Shared.Filters.TryRemove(ctx.Guild.Id, out _);

            try {
                await Database.RemoveFiltersForGuildAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                Shared.LogProvider.LogException(LogLevel.Warning, e);
                throw new CommandFailedException("Failed to delete filters from the database.");
            }

            var logchn = Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "All filters have been deleted",
                    Color = DiscordColor.DarkGreen
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }

            await ctx.InformSuccessAsync("Removed all filters!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_FILTER_DELETE
        [Command("delete"), Priority(1)]
        [Module(ModuleType.Administration)]
        [Description("Remove filters from guild filter list.")]
        [Aliases("-", "remove", "del", "rm", "rem", "d")]
        [UsageExamples("!filter delete fuck f+u+c+k+")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Filters IDs to remove.")] params int[] ids)
        {
            if (!Shared.Filters.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no filters registered.");

            var errors = new StringBuilder();
            foreach (var id in ids) {
                if (Shared.Filters[ctx.Guild.Id].RemoveWhere(f => f.Id == id) == 0) {
                    errors.AppendLine($"Error: Filter with ID {Formatter.Bold(id.ToString())} does not exist.");
                    continue;
                }

                try {
                    await Database.RemoveFilterAsync(ctx.Guild.Id, id)
                        .ConfigureAwait(false);
                } catch (Exception e) {
                    Shared.LogProvider.LogException(LogLevel.Warning, e);
                    errors.AppendLine($"Warning: Failed to remove filter with ID {Formatter.Bold(id.ToString())} from the database.");
                }
            }

            string errlist = errors.ToString();
            var logchn = Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Several filters have been deleted",
                    Color = DiscordColor.DarkGreen
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Tried deleting filters with IDs", string.Join("\n", ids.Select(id => id.ToString())));
                if (!string.IsNullOrWhiteSpace(errlist))
                    emb.AddField("With errors", errlist);
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }

            await ctx.InformSuccessAsync($"Done!\n\n{errlist}")
                .ConfigureAwait(false);
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Filters to remove.")] params string[] filters)
        {
            if (!Shared.Filters.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no filters registered.");

            var errors = new StringBuilder();
            foreach (var filter in filters) {
                var rstr = $@"\b{filter}\b";
                if (Shared.Filters[ctx.Guild.Id].RemoveWhere(f => f.Trigger.ToString() == rstr) == 0) {
                    errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} does not exist.");
                    continue;
                }

                try {
                    await Database.RemoveFilterAsync(ctx.Guild.Id, filter)
                        .ConfigureAwait(false);
                } catch (Exception e) {
                    Shared.LogProvider.LogException(LogLevel.Warning, e);
                    errors.AppendLine($"Warning: Failed to remove filter {Formatter.Bold(filter)} from the database.");
                }
            }

            string errlist = errors.ToString();
            var logchn = Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Several filters have been deleted",
                    Color = DiscordColor.DarkGreen
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Tried deleting filters", string.Join("\n", filters));
                if (!string.IsNullOrWhiteSpace(errlist))
                    emb.AddField("With errors", errlist);
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }

            await ctx.InformSuccessAsync($"Done!\n\n{errlist}")
                .ConfigureAwait(false);
        }

        #endregion

        #region COMMAND_FILTER_LIST
        [Command("list"), Module(ModuleType.Administration)]
        [Description("Show all filters for this guild.")]
        [Aliases("ls", "l")]
        [UsageExamples("!filter list")]
        public async Task ListAsync(CommandContext ctx)
        {
            if (!Shared.Filters.ContainsKey(ctx.Guild.Id) || !Shared.Filters[ctx.Guild.Id].Any())
                throw new CommandFailedException("No filters registered for this guild.");

            await ctx.SendCollectionInPagesAsync(
                "Filters registered for this guild",
                Shared.Filters[ctx.Guild.Id],
                f => $"{f.Id} | {f.Trigger.ToString().Replace(@"\b", "")}",
                DiscordColor.DarkGreen
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
