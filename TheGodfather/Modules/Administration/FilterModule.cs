#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common.Attributes;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Reactions.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("filter"), Module(ModuleType.Administration), NotBlocked]
    [Description("Message filtering commands. If invoked without subcommand, either lists all filters or " +
                 "adds a new filter for the given word list. Filters are regular expressions.")]
    [Aliases("f", "filters")]
    [UsageExampleArgs("fuck fk f+u+c+k+")]
    [RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class FilterModule : TheGodfatherServiceModule<FilteringService>
    {

        public FilterModule(FilteringService service, SharedData shared, DatabaseContextBuilder db)
            : base(service, shared, db)
        {
            
        }


        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Filter list. Filter is a regular expression (case insensitive).")] params string[] filters)
            => this.AddAsync(ctx, filters);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);


        #region COMMAND_FILTER_ADD
        [Command("add")]
        [Description("Add filter to guild filter list.")]
        [Aliases("addnew", "create", "a", "+", "+=", "<", "<<")]
        [UsageExampleArgs("fuck f+u+c+k+")]
        public async Task AddAsync(CommandContext ctx,
                                  [RemainingText, Description("Filter list. Filter is a regular expression (case insensitive).")] params string[] filters)
        {
            if (filters is null || !filters.Any())
                throw new InvalidCommandUsageException("Filter regexes missing.");

            var eb = new StringBuilder();
            foreach (string regexString in filters) {
                if (regexString.Contains('%')) {
                    eb.AppendLine($"Error: Filter {Formatter.InlineCode(regexString)} cannot contain '%' character.");
                    continue;
                }

                if (regexString.Length < 3 || regexString.Length > 120) {
                    eb.AppendLine($"Error: Filter {Formatter.InlineCode(regexString)} doesn't fit the size requirement. Filters cannot be shorter than 3 and longer than 120 characters.");
                    continue;
                }

                if (ctx.Services.GetService<ReactionsService>().GuildHasTextReaction(ctx.Guild.Id, regexString)) {
                    eb.AppendLine($"Error: Filter {Formatter.InlineCode(regexString)} cannot be added because of a conflict with an existing text reaction trigger.");
                    continue;
                }

                if (!regexString.TryParseRegex(out Regex regex)) {
                    eb.AppendLine($"Error: Filter {Formatter.InlineCode(regexString)} is not a valid regular expression.");
                    continue;
                }

                if (ctx.CommandsNext.RegisteredCommands.Any(kvp => regex.IsMatch(kvp.Key))) {
                    eb.AppendLine($"Error: Filter {Formatter.InlineCode(regexString)} collides with an existing bot command.");
                    continue;
                }

                if (this.Service.GetGuildFilters(ctx.Guild.Id).Any(f => f.TriggerString == regex.ToString())) {
                    eb.AppendLine($"Error: Filter {Formatter.InlineCode(regexString)} already exists.");
                    continue;
                }

                if (!await this.Service.AddFilterAsync(ctx.Guild.Id, regexString))
                    eb.AppendLine($"Error: Failed to add filter {Formatter.InlineCode(regexString)}.");
            }

            DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
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
        [Description("Removes filter either by ID or plain text match.")]
        [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
        [UsageExampleArgs("fuck f+u+c+k+", "3 4")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Filters IDs to remove.")] params int[] ids)
        {
            if (ids is null || !ids.Any())
                throw new CommandFailedException("No IDs given.");

            IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(ctx.Guild.Id);
            if (!fs.Any())
                throw new CommandFailedException("This guild has no filters registered.");

            await this.Service.RemoveFiltersAsync(ctx.Guild.Id, ids);

            DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = "Filter deletion occured",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Tried deleting filters with IDs", string.Join("\n", ids.Select(id => id.ToString())));
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, important: false);
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Filters to remove.")] params string[] regexStrings)
        {
            if (regexStrings is null || !regexStrings.Any())
                throw new CommandFailedException("No filters given.");

            IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(ctx.Guild.Id);
            if (!fs.Any())
                throw new CommandFailedException("This guild has no filters registered.");

            await this.Service.RemoveFiltersAsync(ctx.Guild.Id, regexStrings);

            DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = "Filter deletion occured",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Tried deleting filters", string.Join("\n", regexStrings));
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, important: false);
        }

        #endregion

        #region COMMAND_FILTERS_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all filters for the current guild.")]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all filters for this guild?"))
                return;

            int removed = await this.Service.RemoveFiltersAsync(ctx.Guild.Id);

            DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = $"All guild filters have been deleted ({removed} total)",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, important: false);
        }
        #endregion

        #region COMMAND_FILTER_LIST
        [Command("list")]
        [Description("Show all filters for this guild.")]
        [Aliases("ls", "l")]
        public Task ListAsync(CommandContext ctx)
        {
            IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(ctx.Guild.Id);
            if (!fs.Any())
                throw new CommandFailedException("No filters registered for this guild.");

            return ctx.SendCollectionInPagesAsync(
                $"Filters registered for {ctx.Guild.Name}",
                fs.OrderBy(f => f.Id),
                f => $"{Formatter.InlineCode($"{f.Id:D3}")} | {Formatter.InlineCode(f.TriggerString)}",
                this.ModuleColor
            );
        }
        #endregion
    }
}
