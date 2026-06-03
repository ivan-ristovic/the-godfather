using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Administration;

[Group("commandrules")]
[Alias("cmdrules", "crules", "cr")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.Administrator)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class CommandRulesModule : TheGodfatherModule
{
    private readonly CommandRulesService rules;


    public CommandRulesModule(LocalizationService localization, CommandRulesService rules)
        : base(localization, new Color(0x3498DB))
    {
        this.rules = rules;
    }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(ITextChannel? channel = null)
        => this.PrintRulesAsync(channel);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync([Remainder] string command)
        => this.PrintRulesAsync(cmd: command);

    [Command("allow")]
    [Priority(1)]
    [Alias("only", "register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AllowAsync(ITextChannel channel, [Remainder] string command)
        => this.AddRuleAsync(command, true, channel);

    [Command("allow")]
    [Priority(0)]
    public Task AllowAsync(string command, params ITextChannel[] channels)
        => this.AddRuleAsync(command, true, channels);

    [Command("forbid")]
    [Priority(1)]
    [Alias("f", "deny", "unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public Task ForbidAsync(ITextChannel channel, [Remainder] string command)
        => this.AddRuleAsync(command, false, channel);

    [Command("forbid")]
    [Priority(0)]
    public Task ForbidAsync(string command, params ITextChannel[] channels)
        => this.AddRuleAsync(command, false, channels);

    [Command("deleteall")]
    [Alias("reset", "removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task RemoveAllAsync()
    {
        if (!await this.ConfirmAsync(TranslationKey.q_cr_rem_all))
            return;

        await this.rules.ClearAsync(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor, TranslationKey.str_cr_clear);
    }

    [Command("list")]
    [Priority(1)]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync([Remainder] string command)
        => this.PrintRulesAsync(cmd: command);

    [Command("list")]
    [Priority(0)]
    public Task ListAsync(ITextChannel? channel = null)
        => this.PrintRulesAsync(channel);


    private async Task AddRuleAsync(string command, bool allow, params ITextChannel[] channels)
    {
        if (string.IsNullOrWhiteSpace(command)) {
            await this.FailAsync("Command name is required.");
            return;
        }

        string normalized = command.Trim().ToLowerInvariant();
        await this.rules.AddRuleAsync(this.Context.Guild.Id, normalized, allow, channels.Select(c => c.Id));

        if (channels.Any()) {
            string target = channels.Select(c => c.Mention).JoinWith();
            await this.InfoAsync(this.ModuleColor, allow
                ? TranslationKey.fmt_cr_allowed(InlineCode(normalized), target)
                : TranslationKey.fmt_cr_denied(InlineCode(normalized), target)
            );
        } else {
            await this.InfoAsync(this.ModuleColor, allow
                ? TranslationKey.fmt_cr_allowed_global(InlineCode(normalized))
                : TranslationKey.fmt_cr_denied_global(InlineCode(normalized))
            );
        }
    }

    private Task PrintRulesAsync(ITextChannel? channel = null, string? cmd = null, bool includeGlobalRules = false)
    {
        IEnumerable<CommandRule> crs = this.rules.GetRules(this.Context.Guild.Id, cmd);
        if (channel is not null)
            crs = crs.Where(cr => cr.ChannelId == channel.Id);
        else if (!includeGlobalRules)
            crs = crs.Where(cr => cr.ChannelId != 0);

        CommandRule[] items = crs.OrderBy(cr => cr.ChannelId).ThenBy(cr => cr.Command).ToArray();
        if (!items.Any())
            return this.InfoAsync(this.ModuleColor, TranslationKey.cmd_err_cr_none);

        return this.RespondPagedAsync(
            TranslationKey.fmt_cr_list(this.Context.Guild.Name),
            items,
            cr => {
                string mark = cr.Allowed ? Emojis.CheckMarkSuccess : Emojis.X;
                string location = cr.ChannelId == 0
                    ? this.Localization.GetString(this.Context.Guild.Id, TranslationKey.str_global)
                    : (this.Context.Guild.GetTextChannel(cr.ChannelId)?.Mention ?? cr.ChannelId.ToString());
                return this.Localization.GetString(this.Context.Guild.Id, TranslationKey.fmt_cr_list_item(mark, location, InlineCode(cr.Command)));
            }
        );
    }
}
