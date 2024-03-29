﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace TheGodfather.Modules.Misc;

[Group("help")][Module(ModuleType.Misc)][NotBlocked]
[Aliases("h", "?", "??", "???", "man")]
[Cooldown(3, 5, CooldownBucketType.User)]
public sealed class HelpModule : TheGodfatherServiceModule<CommandService>
{
    #region help
    [GroupCommand][Priority(2)]
    public Task ExecuteGroupAsync(CommandContext ctx)
    {
        return ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithLocalizedTitle(TranslationKey.h_title);
            emb.WithLocalizedDescription(TranslationKey.fmt_modules(Enum.GetNames<ModuleType>().Select(s => $"• {s}").JoinWith()));
            emb.WithLocalizedFooter(TranslationKey.h_footer, ctx.Client.CurrentUser.AvatarUrl);
        });
    }

    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_module)] ModuleType module)
    {
        Command? cmd = ctx.CommandsNext.FindCommand(module.ToString(), out string _);
        if (cmd is CommandGroup group && group.IsExecutableWithoutSubcommands)
            return this.ExecuteGroupAsync(ctx, module.ToString());

        IEnumerable<string> cmds = this.Service.GetCommandsInModule(module).OrderBy(cmd => cmd);
        return ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(module.ToDiscordColor());
            emb.WithLocalizedTitle(TranslationKey.h_title_m(module));
            emb.WithLocalizedDescription(module.ToLocalizedDescription(cmds.Select(s => Formatter.InlineCode(s)).JoinWith(", ")));
            emb.WithLocalizedFooter(TranslationKey.h_footer, ctx.Client.CurrentUser.AvatarUrl);
        });
    }

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_cmd)] params string[] cmd)
        => new CommandsNextExtension.DefaultHelpModule().DefaultHelpAsync(ctx, cmd);
    #endregion
}