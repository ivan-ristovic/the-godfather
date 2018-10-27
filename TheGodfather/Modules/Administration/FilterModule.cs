#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("filter"), Module(ModuleType.Administration), NotBlocked]
    [Description("Message filtering commands. If invoked without subcommand, either lists all filters or " +
                 "adds a new filter for the given word list. Filters are regular expressions.")]
    [Aliases("f", "filters")]
    [UsageExamples("!filter fuck fk f+u+c+k+")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class FilterModule : TheGodfatherModule
    {

        public FilterModule(SharedData shared, DBService db) 
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.DarkRed;
        }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(0)]
        [RequirePermissions(Permissions.ManageGuild)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Filter list. Filter is a regular expression (case insensitive).")] params string[] filters)
            => this.AddAsync(ctx, filters);


        #region COMMAND_FILTER_ADD
        [Command("add")]
        [Description("Add filter to guild filter list.")]
        [Aliases("addnew", "create", "a", "+", "+=", "<", "<<")]
        [UsageExamples("!filter add fuck f+u+c+k+")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [RemainingText, Description("Filter list. Filter is a regular expression (case insensitive).")] params string[] filters)
        {
            if (filters is null || !filters.Any())
                throw new InvalidCommandUsageException("Filter regexes missing.");

            var eb = new StringBuilder();

            using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                foreach (string regexString in filters) {
                    if (regexString.Contains('%')) {
                        eb.AppendLine($"Error: Filter {Formatter.InlineCode(regexString)} cannot contain '%' character.");
                        continue;
                    }

                    if (regexString.Length < 3 || regexString.Length > 60) {
                        eb.AppendLine($"Error: Filter {Formatter.InlineCode(regexString)} doesn't fit the size requirement. Filters cannot be shorter than 3 and longer than 60 characters.");
                        continue;
                    }

                    if (this.Shared.GuildHasTextReaction(ctx.Guild.Id, regexString)) {
                        eb.AppendLine($"Error: Filter {Formatter.InlineCode(regexString)} cannot be added because of a conflict with an existing text reaction trigger.");
                        continue;
                    }

                    if (!regexString.TryParseRegex(out var regex)) {
                        eb.AppendLine($"Error: Filter {Formatter.InlineCode(regexString)} is not a valid regular expression.");
                        continue;
                    }

                    if (ctx.CommandsNext.RegisteredCommands.Any(kvp => regex.IsMatch(kvp.Key))) {
                        eb.AppendLine($"Error: Filter {Formatter.InlineCode(regexString)} collides with an existing bot command.");
                        continue;
                    }

                    if (this.Shared.Filters.TryGetValue(ctx.Guild.Id, out var existingFilters)) {
                        if (existingFilters.Any(f => regexString == regex.ToString())) {
                            eb.AppendLine($"Error: Filter {Formatter.InlineCode(regexString)} already exists.");
                            continue;
                        }
                    } else {
                        if (!this.Shared.Filters.TryAdd(ctx.Guild.Id, new ConcurrentHashSet<Filter>()))
                            throw new ConcurrentOperationException("Failed to create filter data structure for this guild. This is bad!");
                    }

                    var filter = new DatabaseFilter() { GuildId = ctx.Guild.Id, Trigger = regexString };
                    db.Filters.Add(filter);

                    if (filter.Id == 0 || !this.Shared.Filters[ctx.Guild.Id].Add(new Filter(filter.Id, regex)))
                        eb.AppendLine($"Error: Failed to add filter {Formatter.InlineCode(regexString)}.");
                }

                await db.SaveChangesAsync();
            }
            
            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Filter addition occured",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Tried adding filters", string.Join("\n", filters.Select(rgx => Formatter.InlineCode(rgx))));
                if (eb.Length > 0)
                    emb.AddField("Errors", eb.ToString());
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            if (eb.Length > 0)
                await this.InformFailureAsync(ctx, $"Action finished with warnings/errors:\n\n{eb.ToString()}");
            else
                await this.InformAsync(ctx, "Successfully added all given filters!", important: false);
        }
        #endregion

        #region COMMAND_FILTER_DELETE
        [Command("delete"), Priority(1)]
        [Description("Removes filter either by ID or plain text.")]
        [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
        [UsageExamples("!filter delete fuck f+u+c+k+",
                       "!filter delete 3 4")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Filters IDs to remove.")] params int[] ids)
        {
            if (!this.Shared.Filters.TryGetValue(ctx.Guild.Id, out var filters))
                throw new CommandFailedException("This guild has no filters registered.");

            var eb = new StringBuilder();
            using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                foreach (int id in ids) {
                    if (filters.RemoveWhere(f => f.Id == id) == 0) {
                        eb.AppendLine($"Error: Filter with ID {Formatter.Bold(id.ToString())} does not exist.");
                        continue;
                    }
                    db.Filters.Remove(new DatabaseFilter() { GuildId = ctx.Guild.Id, Id = id });
                }

                await db.SaveChangesAsync();
            }
            
            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Filter deletion occured",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Tried deleting filters with IDs", string.Join("\n", ids.Select(id => id.ToString())));
                if (eb.Length > 0)
                    emb.AddField("Errors", eb.ToString());
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            if (eb.Length > 0)
                await this.InformFailureAsync(ctx, $"Action finished with warnings/errors:\n\n{eb.ToString()}");
            else
                await this.InformAsync(ctx, "Successfully deleted all given filters!", important: false);
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Filters to remove.")] params string[] filters)
        {
            if (!this.Shared.Filters.TryGetValue(ctx.Guild.Id, out var existingFilters))
                throw new CommandFailedException("This guild has no filters registered.");

            var eb = new StringBuilder();
            using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                foreach (string regexString in filters) {
                    string filterString = Filter.Wrap(regexString);
                    if (existingFilters.RemoveWhere(f => f.Trigger.ToString() == filterString) == 0) {
                        eb.AppendLine($"Error: Filter {Formatter.InlineCode(regexString)} does not exist.");
                        continue;
                    }

                    db.Filters.Remove(new DatabaseFilter() { GuildId = ctx.Guild.Id, Trigger = regexString });
                }

                await db.SaveChangesAsync();
            }
            
            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Filter deletion occured",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Tried deleting filters", string.Join("\n", filters));
                if (eb.Length > 0)
                    emb.AddField("Errors", eb.ToString());
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }

            if (eb.Length > 0)
                await this.InformFailureAsync(ctx, $"Action finished with warnings/errors:\n\n{eb.ToString()}");
            else
                await this.InformAsync(ctx);
        }

        #endregion

        #region COMMAND_FILTERS_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all filters for the current guild.")]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da")]
        [UsageExamples("!filter clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all filters for this guild?"))
                return;

            if (this.Shared.Filters.ContainsKey(ctx.Guild.Id) && !this.Shared.Filters.TryRemove(ctx.Guild.Id, out _))
                throw new ConcurrentOperationException("Failed to remove filter data structure for this guild. This is bad!");

            using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                db.RemoveRange(db.Filters.Where(f => f.GuildId == ctx.Guild.Id));
                await db.SaveChangesAsync();
            }

            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "All filters have been deleted",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, "Successfully deleted all guild filters!", important: false);
        }
        #endregion

        #region COMMAND_FILTER_LIST
        [Command("list")]
        [Description("Show all filters for this guild.")]
        [Aliases("ls", "l")]
        [UsageExamples("!filter list")]
        public Task ListAsync(CommandContext ctx)
        {
            if (!this.Shared.Filters.TryGetValue(ctx.Guild.Id, out var filters) || !filters.Any())
                throw new CommandFailedException("No filters registered for this guild.");

            return ctx.SendCollectionInPagesAsync(
                $"Filters registered for {ctx.Guild.Name}",
                this.Shared.Filters[ctx.Guild.Id].OrderBy(f => f.Id),
                f => $"{Formatter.InlineCode($"{f.Id:D3}")} | {Formatter.InlineCode(f.GetBaseRegexString())}",
                this.ModuleColor
            );
        }
        #endregion
    }
}
