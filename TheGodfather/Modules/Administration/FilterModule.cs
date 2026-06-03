using System.Text;
using System.Text.RegularExpressions;
using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Reactions.Services;

namespace TheGodfather.Modules.Administration;

[Group("filter")]
[Alias("f", "filters", "autodel")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class FilterModule : TheGodfatherModule
{
    private readonly FilteringService filters;
    private readonly ReactionsService reactions;

    public FilterModule(LocalizationService localization, FilteringService filters, ReactionsService reactions)
        : base(localization, new Color(0x3498DB))
    {
        this.filters = filters;
        this.reactions = reactions;
    }

    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(Filter.Action action, [Remainder] string filter)
        => this.InternalAddAsync(action, filter);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync([Remainder] string? filter = null)
        => string.IsNullOrWhiteSpace(filter) ? this.ListAsync() : this.InternalAddAsync(Filter.Action.Delete, filter);

    [Command("add")]
    [Priority(1)]
    [Alias("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(Filter.Action action, [Remainder] string filter)
        => this.InternalAddAsync(action, filter);

    [Command("add")]
    [Priority(0)]
    public Task AddAsync([Remainder] string filter)
        => this.InternalAddAsync(Filter.Action.Delete, filter);

    [Command("delete")]
    [Priority(1)]
    [Alias("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
    public async Task DeleteAsync(params int[] ids)
    {
        if (ids is null || !ids.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_f_ids_none);

        int removed = await this.filters.RemoveFiltersAsync(this.Context.Guild.Id, ids);
        await this.InfoAsync(this.ModuleColor, TranslationKey.str_f_del(removed));
    }

    [Command("delete")]
    [Priority(0)]
    public Task DeletePatternAsync([Remainder] string regex)
        => this.DeletePatternsAsync(regex);

    [Command("delete matching")]
    [Alias("delete match", "delete m")]
    public async Task DeleteMatchingAsync([Remainder] string match)
    {
        if (string.IsNullOrWhiteSpace(match))
            throw new LocalizedCommandException(TranslationKey.cmd_err_f_pat_none);

        int removed = await this.filters.RemoveFiltersMatchingAsync(this.Context.Guild.Id, match);
        await this.InfoAsync(this.ModuleColor, TranslationKey.str_f_del(removed));
    }

    [Command("delete pattern")]
    [Alias("delete regex", "delete regexp", "delete p", "delete r")]
    public Task DeletePatternsAsync([Remainder] string regex)
        => this.DeletePatternsAsync(SplitArguments(regex));

    [Command("deleteall")]
    [Alias("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task DeleteAllAsync()
    {
        if (!await this.ConfirmAsync(TranslationKey.q_f_rem_all))
            return;

        int removed = await this.filters.RemoveFiltersAsync(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor, TranslationKey.str_f_del_all(removed));
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync()
    {
        IReadOnlyCollection<Filter> fs = this.filters.GetGuildFilters(this.Context.Guild.Id);
        if (!fs.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_f_none);

        return this.RespondPagedAsync(
            TranslationKey.str_f,
            fs.OrderBy(f => f.Id),
            f => $"{InlineCode($"{f.Id:D3}")} | {InlineCode(f.RegexString)} | {f.OnHitAction.Humanize()}"
        );
    }


    private async Task DeletePatternsAsync(params string[] regexStrings)
    {
        if (regexStrings is null || !regexStrings.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_f_pat_none);

        int removed = await this.filters.RemoveFiltersAsync(this.Context.Guild.Id, regexStrings);
        await this.InfoAsync(this.ModuleColor, TranslationKey.str_f_del(removed));
    }

    private async Task InternalAddAsync(Filter.Action action, params string[] filterArgs)
    {
        string[] regexStrings = filterArgs.SelectMany(SplitArguments).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (!regexStrings.Any()) {
            await this.ListAsync();
            return;
        }

        var errors = new StringBuilder();
        foreach (string regexString in regexStrings) {
            if (regexString.Contains('%')) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_f_percent(InlineCode(regexString))));
                continue;
            }

            if (regexString.Length is < 3 or > Filter.FilterLimit) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_f_size(InlineCode(regexString), Filter.FilterLimit)));
                continue;
            }

            if (this.reactions.GuildHasTextReaction(this.Context.Guild.Id, regexString)) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_f_tr(InlineCode(regexString))));
                continue;
            }

            if (!regexString.TryParseRegex(out Regex? regex) || regex is null) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_f_invalid(InlineCode(regexString))));
                continue;
            }

            if (!await this.filters.AddFilterAsync(this.Context.Guild.Id, regex, action))
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_f_dup(InlineCode(regexString))));
        }

        if (errors.Length > 0)
            await this.FailAsync(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.fmt_action_err(errors.ToString())));
        else
            await this.InfoAsync(this.ModuleColor, TranslationKey.str_f_add);
    }

    private static string[] SplitArguments(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
