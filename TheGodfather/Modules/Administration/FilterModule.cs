using System.Text;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.EventListeners.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Reactions.Services;

namespace TheGodfather.Modules.Administration;

[Group("filter")][Module(ModuleType.Administration)][NotBlocked]
[Aliases("f", "filters", "autodel")]
[RequireGuild][RequireUserPermissions(Permissions.ManageGuild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class FilterModule : TheGodfatherServiceModule<FilteringService>
{
    #region filter
    [GroupCommand][Priority(2)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_punish_action)] Filter.Action action,
        [RemainingText][Description(TranslationKey.desc_filters)] params string[] filters)
        => this.InternalAddAsync(ctx, filters, action);

    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_filters)] params string[] filters)
        => this.InternalAddAsync(ctx, filters);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx)
        => this.ListAsync(ctx);
    #endregion

    #region filter add
    [Command("add")][Priority(1)]
    [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_punish_action)] Filter.Action action,
        [RemainingText][Description(TranslationKey.desc_filters)] params string[] filters)
        => this.InternalAddAsync(ctx, filters, action);

    [Command("add")][Priority(0)]
    public Task AddAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_filters)] params string[] filters)
        => this.InternalAddAsync(ctx, filters);
    #endregion

    #region filter delete
    [Group("delete")]
    [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
    public class FilterDeleteModule : TheGodfatherServiceModule<FilteringService>
    {
        #region filter delete
        [GroupCommand][Priority(1)]
        public Task DeleteAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_filters_del_ids)] params int[] ids)
            => this.DeleteIdAsync(ctx, ids);

        [GroupCommand][Priority(0)]
        public Task DeleteAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_filters_del)] params string[] regexStrings)
            => this.DeletePatternAsync(ctx, regexStrings);
        #endregion

        #region filter delete id
        public async Task DeleteIdAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_filters_del_ids)] params int[] ids)
        {
            if (ids is null || !ids.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_f_ids_none);

            IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(ctx.Guild.Id);
            if (!fs.Any())
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_f_none);

            int removed = await this.Service.RemoveFiltersAsync(ctx.Guild.Id, ids);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_f_del);
                emb.WithDescription(ids.JoinWith());
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_f_del(removed));
        }
        #endregion

        #region filter delete matching
        public async Task DeleteMatchingAsync(CommandContext ctx,
            [Description(TranslationKey.desc_filters_del)] string match)
        {
            if (string.IsNullOrWhiteSpace(match))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_f_pat_none);

            IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(ctx.Guild.Id);
            if (!fs.Any())
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_f_none);

            int removed = await this.Service.RemoveFiltersMatchingAsync(ctx.Guild.Id, match);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_f_del_match);
                emb.WithDescription(match);
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_f_del(removed));
        }
        #endregion

        #region filter delete pattern
        public async Task DeletePatternAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_filters_del)] params string[] regexStrings)
        {
            if (regexStrings is null || !regexStrings.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_f_pat_none);

            IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(ctx.Guild.Id);
            if (!fs.Any())
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_f_none);

            int removed = await this.Service.RemoveFiltersAsync(ctx.Guild.Id, regexStrings);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_f_del);
                emb.WithDescription(regexStrings.JoinWith());
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_f_del(removed));
        }
        #endregion
    }
    #endregion

    #region filter deleteall
    [Command("deleteall")][UsesInteractivity]
    [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task DeleteAllAsync(CommandContext ctx)
    {
        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_f_rem_all))
            return;

        int removed = await this.Service.RemoveFiltersAsync(ctx.Guild.Id);

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_f_del_all);
            emb.AddLocalizedField(TranslationKey.str_count, removed, true);
        });

        await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_f_del_all(removed));
    }
    #endregion

    #region filter list
    [Command("list")]
    [Aliases("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync(CommandContext ctx)
    {
        IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(ctx.Guild.Id);
        if (!fs.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_f_none);

        return ctx.PaginateAsync(
            TranslationKey.str_f,
            fs.OrderBy(f => f.Id),
            f => $"{Formatter.InlineCode($"{f.Id:D3}")} | {Formatter.InlineCode(f.RegexString)} | {f.OnHitAction.Humanize()}",
            this.ModuleColor
        );
    }
    #endregion


    #region internals
    private async Task InternalAddAsync(CommandContext ctx, IEnumerable<string> filters, Filter.Action action = Filter.Action.Delete)
    {
        if (filters is null || !filters.Any()) {
            await this.ListAsync(ctx);
            return;
        }

        var eb = new StringBuilder();
        foreach (string regexString in filters) {
            if (regexString.Contains('%')) {
                eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_f_percent(Formatter.InlineCode(regexString))));
                continue;
            }

            if (regexString.Length is < 3 or > Filter.FilterLimit) {
                eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_f_size(Formatter.InlineCode(regexString), Filter.FilterLimit)));
                continue;
            }

            if (ctx.Services.GetRequiredService<ReactionsService>().GuildHasTextReaction(ctx.Guild.Id, regexString)) {
                eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_f_tr(Formatter.InlineCode(regexString))));
                continue;
            }

            if (!regexString.TryParseRegex(out Regex? regex) || regex is null) {
                eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_f_invalid(Formatter.InlineCode(regexString))));
                continue;
            }

            if (ctx.CommandsNext.RegisteredCommands.Any(kvp => regex.IsMatch(kvp.Key))) {
                eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_f_err(Formatter.InlineCode(regexString))));
                continue;
            }

            if (!await this.Service.AddFilterAsync(ctx.Guild.Id, regex, action))
                eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_f_dup(Formatter.InlineCode(regexString))));
        }

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_f_add);
            emb.WithDescription(filters.Select(rgx => Formatter.InlineCode(rgx)).JoinWith());
            emb.AddLocalizedField(TranslationKey.evt_f_action, action.Humanize(), true);
            if (eb.Length > 0)
                emb.AddLocalizedField(TranslationKey.str_errs, eb.ToString());
        });

        if (eb.Length > 0)
            await ctx.FailAsync(TranslationKey.fmt_action_err(eb.ToString()));
        else
            await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_f_add);
    }
    #endregion
}