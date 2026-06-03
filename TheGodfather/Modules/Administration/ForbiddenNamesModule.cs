using System.Text;
using System.Text.RegularExpressions;
using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Administration;

[Group("forbiddennames")]
[Alias("forbiddenname", "forbiddennicknames", "disallowednames", "fnames", "fname", "fn")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild | GuildPermission.ManageNicknames)]
[RequireBotPermission(GuildPermission.ManageNicknames)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class ForbiddenNamesModule : TheGodfatherModule
{
    private readonly ForbiddenNamesService forbiddenNames;


    public ForbiddenNamesModule(LocalizationService localization, ForbiddenNamesService forbiddenNames)
        : base(localization, new Color(0x3498DB))
    {
        this.forbiddenNames = forbiddenNames;
    }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync([Remainder] string names)
        => this.InternalAddAsync(null, SplitArguments(names));

    [Command("add")]
    [Priority(2)]
    [Alias("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(Punishment.Action action, [Remainder] string names)
        => this.InternalAddAsync(action, SplitArguments(names));

    [Command("add")]
    [Priority(1)]
    public Task AddAsync(string name, Punishment.Action? action = null)
        => this.InternalAddAsync(action, name);

    [Command("add")]
    [Priority(0)]
    public Task AddAsync([Remainder] string names)
        => this.InternalAddAsync(null, SplitArguments(names));

    [Command("delete")]
    [Priority(1)]
    [Alias("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
    public async Task DeleteAsync(params int[] ids)
    {
        if (ids is null || !ids.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_f_ids_none);

        int removed = await this.forbiddenNames.RemoveForbiddenNamesAsync(this.Context.Guild.Id, ids);
        await this.InfoAsync(this.ModuleColor, TranslationKey.str_fn_del(removed));
    }

    [Command("delete")]
    [Priority(0)]
    public Task DeletePatternAsync([Remainder] string regex)
        => this.DeletePatternsAsync(SplitArguments(regex));

    [Command("delete matching")]
    [Alias("delete match", "delete m")]
    public async Task DeleteMatchingAsync([Remainder] string match)
    {
        if (string.IsNullOrWhiteSpace(match))
            throw new LocalizedCommandException(TranslationKey.cmd_err_fn_pat_none);

        int removed = await this.forbiddenNames.RemoveForbiddenNamesMatchingAsync(this.Context.Guild.Id, match);
        await this.InfoAsync(this.ModuleColor, TranslationKey.str_fn_del(removed));
    }

    [Command("deleteall")]
    [Alias("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task DeleteAllAsync()
    {
        if (!await this.ConfirmAsync(TranslationKey.q_fn_rem_all))
            return;

        int removed = await this.forbiddenNames.RemoveForbiddenNamesAsync(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor, TranslationKey.str_fn_del_all(removed));
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync()
    {
        IReadOnlyCollection<ForbiddenName> fs = this.forbiddenNames.GetGuildForbiddenNames(this.Context.Guild.Id);
        if (!fs.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_fn_none);

        return this.RespondPagedAsync(
            TranslationKey.str_fn,
            fs.OrderBy(f => f.Id),
            f => $"{InlineCode($"{f.Id:D3}")} | {InlineCode(f.RegexString)} | {f.ActionOverride?.Humanize() ?? "-"}"
        );
    }


    private async Task DeletePatternsAsync(params string[] regexStrings)
    {
        if (regexStrings is null || !regexStrings.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_fn_pat_none);

        int removed = await this.forbiddenNames.RemoveForbiddenNamesAsync(this.Context.Guild.Id, regexStrings);
        await this.InfoAsync(this.ModuleColor, TranslationKey.str_fn_del(removed));
    }

    private async Task InternalAddAsync(Punishment.Action? action, params string[] names)
    {
        if (names is null || !names.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_fn_pat_none);

        var errors = new StringBuilder();
        var added = new List<Regex>();
        foreach (string regexString in names.Where(n => !string.IsNullOrWhiteSpace(n))) {
            if (regexString.Length is < 3 or > ForbiddenName.NameLimit) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_fn_size(InlineCode(regexString), ForbiddenName.NameLimit)));
                continue;
            }

            if (!regexString.TryParseRegex(out Regex? regex) || regex is null) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_fn_invalid(InlineCode(regexString))));
                continue;
            }

            if (!this.forbiddenNames.IsSafePattern(regex)) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_fn_unsafe(InlineCode(regexString))));
                continue;
            }

            if (!await this.forbiddenNames.AddForbiddenNameAsync(this.Context.Guild.Id, regex, action)) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_fn_dup(InlineCode(regexString))));
                continue;
            }

            added.Add(regex);
        }

        await EnforceAsync(added, action);

        if (errors.Length > 0)
            await this.FailAsync(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.fmt_err(errors.ToString())));
        else
            await this.InfoAsync(this.ModuleColor, TranslationKey.str_fn_add);
    }

    private async Task EnforceAsync(IEnumerable<Regex> patterns, Punishment.Action? action)
    {
        Regex[] addedPatterns = patterns.ToArray();
        if (!addedPatterns.Any())
            return;

        SocketGuildUser bot = this.Context.Guild.CurrentUser;
        foreach (SocketGuildUser member in this.Context.Guild.Users.Where(u => !u.IsBot && u.Hierarchy < bot.Hierarchy)) {
            Regex? match = addedPatterns.FirstOrDefault(r => r.IsMatch(member.DisplayName));
            if (match is null)
                continue;

            if (action is null)
                await member.ModifyAsync(
                    props => props.Nickname = member.Id.ToString(),
                    new RequestOptions {
                        AuditLogReason = this.Localization.GetString(this.Context.Guild.Id, TranslationKey.rsn_fname_match(match))
                    }
                );
        }
    }

    private static string[] SplitArguments(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
