using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Reactions.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration
{
    [Group("filter"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("f", "filters", "autodel")]
    [RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class FilterModule : TheGodfatherServiceModule<FilteringService>
    {
        public FilterModule(FilteringService service)
            : base(service) { }


        #region filter
        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-filters")] params string[] filters)
            => this.AddAsync(ctx, filters);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);
        #endregion

        #region filter add
        [Command("add")]
        [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public async Task AddAsync(CommandContext ctx,
                                  [RemainingText, Description("desc-filters")] params string[] filters)
        {
            if (filters is null || !filters.Any())
                throw new InvalidCommandUsageException(ctx, "cmd-err-f-missing");

            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            var eb = new StringBuilder();
            foreach (string regexString in filters) {
                if (regexString.Contains('%')) {
                    eb.AppendLine(lcs.GetString(ctx.Guild.Id, "cmd-err-f-%", Formatter.InlineCode(regexString)));
                    continue;
                }

                if (regexString.Length < 3 || regexString.Length > 120) {
                    eb.AppendLine(lcs.GetString(ctx.Guild.Id, "cmd-err-f-size", Formatter.InlineCode(regexString)));
                    continue;
                }

                if (ctx.Services.GetService<ReactionsService>().GuildHasTextReaction(ctx.Guild.Id, regexString)) {
                    eb.AppendLine(lcs.GetString(ctx.Guild.Id, "cmd-err-f-tr", Formatter.InlineCode(regexString)));
                    continue;
                }

                if (!regexString.TryParseRegex(out Regex? regex) || regex is null) {
                    eb.AppendLine(lcs.GetString(ctx.Guild.Id, "cmd-err-f-invalid", Formatter.InlineCode(regexString)));
                    continue;
                }

                if (ctx.CommandsNext.RegisteredCommands.Any(kvp => regex.IsMatch(kvp.Key))) {
                    eb.AppendLine(lcs.GetString(ctx.Guild.Id, "cmd-err-f-err", Formatter.InlineCode(regexString)));
                    continue;
                }

                if (this.Service.GetGuildFilters(ctx.Guild.Id).Any(f => f.TriggerString == regex.ToString())) {
                    eb.AppendLine(lcs.GetString(ctx.Guild.Id, "cmd-err-f-dup", Formatter.InlineCode(regexString)));
                    continue;
                }

                if (!await this.Service.AddFilterAsync(ctx.Guild.Id, regexString))
                    eb.AppendLine(lcs.GetString(ctx.Guild.Id, "cmd-err-f-err", Formatter.InlineCode(regexString)));
            }

            LoggingService ls = ctx.Services.GetRequiredService<LoggingService>();
            if (ls.IsLogEnabledFor(ctx.Guild.Id, out LocalizedEmbedBuilder emb)) {
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedTitle("evt-f-add");
                emb.AddLocalizedTitleField("evt-usr-responsible", ctx.User.Mention, inline: true);
                emb.AddLocalizedTitleField("evt-invoke-loc", ctx.Channel.Mention, inline: true);
                emb.AddLocalizedTitleField("fmt-f-add", filters.Select(rgx => Formatter.InlineCode(rgx)).Separate());
                if (eb.Length > 0)
                    emb.AddLocalizedTitleField("str-errs", eb.ToString());
                await ls.LogAsync(ctx.Guild, emb);
            }

            if (eb.Length > 0)
                await ctx.InfoAsync(this.ModuleColor, "evt-action-err", eb.ToString());
            else
                await ctx.InfoAsync(this.ModuleColor, "str-f-add");
        }
        #endregion

        #region filter delete
        [Group("delete")]
        [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
        public class FilterDeleteModule : TheGodfatherServiceModule<FilteringService>
        {
            public FilterDeleteModule(FilteringService service)
                : base(service) { }


            #region filter delete
            [GroupCommand, Priority(1)]
            public Task DeleteAsync(CommandContext ctx,
                                   [RemainingText, Description("desc-filters-del-ids")] params int[] ids)
                => this.DeleteIdAsync(ctx, ids);

            [GroupCommand, Priority(0)]
            public Task DeleteAsync(CommandContext ctx,
                                   [RemainingText, Description("desc-filters-del")] params string[] regexStrings)
                => this.DeletePatternAsync(ctx, regexStrings);
            #endregion

            #region filter delete id
            public async Task DeleteIdAsync(CommandContext ctx,
                                           [RemainingText, Description("desc-filters-del-ids")] params int[] ids)
            {
                if (ids is null || !ids.Any())
                    throw new CommandFailedException(ctx, "cmd-err-f-ids-none");

                IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(ctx.Guild.Id);
                if (!fs.Any())
                    throw new InvalidCommandUsageException(ctx, "cmd-err-f-none");

                int removed = await this.Service.RemoveFiltersAsync(ctx.Guild.Id, ids);

                LoggingService ls = ctx.Services.GetRequiredService<LoggingService>();
                if (ls.IsLogEnabledFor(ctx.Guild.Id, out LocalizedEmbedBuilder emb)) {
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedTitle("evt-f-del");
                    emb.AddLocalizedTitleField("evt-usr-responsible", ctx.User.Mention, inline: true);
                    emb.AddLocalizedTitleField("evt-invoke-loc", ctx.Channel.Mention, inline: true);
                    emb.AddLocalizedTitleField("fmt-f-del", ids.Separate());
                    await ls.LogAsync(ctx.Guild, emb);
                }

                await ctx.InfoAsync(this.ModuleColor, "str-f-del", removed);
            }
            #endregion

            #region filter delete matching
            public async Task DeleteMatchingAsync(CommandContext ctx,
                                                 [Description("desc-filters-del")] string match)
            {
                if (string.IsNullOrWhiteSpace(match))
                    throw new CommandFailedException(ctx, "cmd-err-f-pat-none");

                IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(ctx.Guild.Id);
                if (!fs.Any())
                    throw new InvalidCommandUsageException(ctx, "cmd-err-f-none");

                int removed = await this.Service.RemoveFiltersMatchingAsync(ctx.Guild.Id, match);

                LoggingService ls = ctx.Services.GetRequiredService<LoggingService>();
                if (ls.IsLogEnabledFor(ctx.Guild.Id, out LocalizedEmbedBuilder emb)) {
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedTitle("evt-f-del");
                    emb.AddLocalizedTitleField("evt-usr-responsible", ctx.User.Mention, inline: true);
                    emb.AddLocalizedTitleField("evt-invoke-loc", ctx.Channel.Mention, inline: true);
                    emb.AddLocalizedTitleField("fmt-f-del-matching", match);
                    await ls.LogAsync(ctx.Guild, emb);
                }

                await ctx.InfoAsync(this.ModuleColor, "str-f-del", removed);
            }
            #endregion

            #region filter delete pattern
            public async Task DeletePatternAsync(CommandContext ctx,
                                                [RemainingText, Description("desc-filters-del")] params string[] regexStrings)
            {
                if (regexStrings is null || !regexStrings.Any())
                    throw new CommandFailedException(ctx, "cmd-err-f-pat-none");

                IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(ctx.Guild.Id);
                if (!fs.Any())
                    throw new InvalidCommandUsageException(ctx, "cmd-err-f-none");

                int removed = await this.Service.RemoveFiltersAsync(ctx.Guild.Id, regexStrings);

                LoggingService ls = ctx.Services.GetRequiredService<LoggingService>();
                if (ls.IsLogEnabledFor(ctx.Guild.Id, out LocalizedEmbedBuilder emb)) {
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedTitle("evt-f-del");
                    emb.AddLocalizedTitleField("evt-usr-responsible", ctx.User.Mention, inline: true);
                    emb.AddLocalizedTitleField("evt-invoke-loc", ctx.Channel.Mention, inline: true);
                    emb.AddLocalizedTitleField("fmt-f-del", regexStrings.Separate());
                    await ls.LogAsync(ctx.Guild, emb);
                }
                
                await ctx.InfoAsync(this.ModuleColor, "str-f-del", removed);
            }
            #endregion
        }
        #endregion

        #region filter deleteall
        [Command("deleteall"), UsesInteractivity]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("q-f-rem-all"))
                return;

            int removed = await this.Service.RemoveFiltersAsync(ctx.Guild.Id);

            LoggingService ls = ctx.Services.GetRequiredService<LoggingService>();
            if (ls.IsLogEnabledFor(ctx.Guild.Id, out LocalizedEmbedBuilder emb)) {
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedTitle("evt-f-del-all");
                emb.AddLocalizedTitleField("evt-usr-responsible", ctx.User.Mention, inline: true);
                emb.AddLocalizedTitleField("evt-invoke-loc", ctx.Channel.Mention, inline: true);
                emb.AddLocalizedTitleField("str-count", removed, inline: true);
                await ls.LogAsync(ctx.Guild, emb);
            }
            
            await ctx.InfoAsync(this.ModuleColor, "str-f-del-all", removed);
        }
        #endregion

        #region filter list
        [Command("list")]
        [Aliases("print", "show", "ls", "l", "p")]
        public Task ListAsync(CommandContext ctx)
        {
            IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(ctx.Guild.Id);
            return fs.Any()
                ? ctx.PaginateAsync(
                    "str-f",
                    fs.OrderBy(f => f.Id),
                    f => $"{Formatter.InlineCode($"{f.Id:D3}")} | {Formatter.InlineCode(f.TriggerString)}",
                    this.ModuleColor
                )
                : throw new CommandFailedException(ctx, "cmd-err-f-none");
        }
        #endregion
    }
}
