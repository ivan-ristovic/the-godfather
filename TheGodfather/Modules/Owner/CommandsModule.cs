using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Owner;

[Group("commands")]
[Alias("cmds", "cmd")]
[RequireOwner]
public sealed class CommandsModule : TheGodfatherModule
{
    private readonly CommandService commands;
    private readonly DynamicCommandService dynamicCommands;

    public CommandsModule(LocalizationService localization, CommandService commands, DynamicCommandService dynamicCommands)
        : base(localization, new Color(0xE74C3C))
    {
        this.commands = commands;
        this.dynamicCommands = dynamicCommands;
    }

    [Command("")]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("add")]
    [Alias("register", "reg", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public async Task AddAsync([Remainder] string definition)
    {
        (string? name, string? response) = ParseDefinition(definition);
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(response))
            throw new LocalizedCommandException(TranslationKey.cmd_err_cmd_add_cb);

        if (!this.dynamicCommands.Add(name, response))
            await this.FailAsync(TranslationKey.cmd_err_trig_exists(name));
        else
            await this.InfoAsync(this.ModuleColor);
    }

    [Command("delete")]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public Task DeleteAsync([Remainder] string command)
        => this.dynamicCommands.Remove(command)
            ? this.InfoAsync(this.ModuleColor)
            : this.FailAsync(TranslationKey.cmd_name_404(command));

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync()
    {
        IEnumerable<string> builtIn = this.commands.Commands
            .OrderBy(cmd => cmd.Name)
            .Select(cmd => InlineCode(string.Join(" ", cmd.Aliases.FirstOrDefault() ?? cmd.Name)));
        IEnumerable<string> dynamic = this.dynamicCommands.GetAll().Keys.Select(cmd => $"{InlineCode(cmd)} *");
        return this.RespondPagedAsync(TranslationKey.str_cmds, builtIn.Concat(dynamic), line => line);
    }

    private static (string? Name, string? Response) ParseDefinition(string definition)
    {
        definition = StripCodeFence(definition);
        string[] split = definition.Split("=>", 2, StringSplitOptions.TrimEntries);
        return split.Length == 2 ? (split[0], split[1]) : (null, null);
    }

    private static string StripCodeFence(string value)
    {
        value = value.Trim();
        if (!value.StartsWith("```", StringComparison.Ordinal) || !value.EndsWith("```", StringComparison.Ordinal))
            return value;

        value = value[3..^3].Trim();
        return value.StartsWith("cmd", StringComparison.OrdinalIgnoreCase)
            ? value[3..].Trim()
            : value;
    }
}
