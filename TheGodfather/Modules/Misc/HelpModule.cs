using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Misc;

[Group("help")]
[Alias("h", "?", "??", "???", "man")]
[Cooldown(3, 5, CooldownBucketType.User)]
public sealed class HelpModule : TheGodfatherModule
{
    private readonly CommandService commands;


    public HelpModule(LocalizationService localization, CommandService commands)
        : base(localization, new Color(0x007FFF))
    {
        this.commands = commands;
    }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync()
    {
        IEnumerable<string> moduleNames = this.commands.Modules
            .Select(module => module.Name)
            .Distinct()
            .OrderBy(name => name)
            .Select(name => $"- {name}");

        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithLocalizedTitle(TranslationKey.h_title);
            emb.WithDescription(moduleNames.JoinWith());
            emb.WithLocalizedFooter(TranslationKey.h_footer, this.Context.Client.CurrentUser.GetAvatarUrl());
        });
    }

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync([Remainder] string command)
    {
        SearchResult search = this.commands.Search(this.Context, command);
        if (!search.IsSuccess || !search.Commands.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_404(command));

        CommandInfo info = search.Commands.First().Command;
        string aliases = info.Aliases.Select(InlineCode).JoinWith(", ");
        string parameters = info.Parameters.Select(param => $"{param.Name}: {param.Type.Name}").JoinWith("\n");
        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithTitle(info.Name);
            emb.AddLocalizedField(TranslationKey.str_aliases, aliases, false, false);
            emb.AddLocalizedField(TranslationKey.str_args, parameters, false, false);
            emb.WithLocalizedFooter(TranslationKey.h_footer, this.Context.Client.CurrentUser.GetAvatarUrl());
        });
    }
}
