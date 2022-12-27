using DSharpPlus.CommandsNext;

namespace TheGodfather.Services.Extensions;

public static class CommandServiceExtensions
{
    public static bool TranslationsPresentForRegisteredCommands(this CommandService cs, LocalizationService lcs, IEnumerable<Command> cmds)
    {
        bool succ = true;
            
        foreach (Command cmd in cmds)
            try {
                _ = lcs.GetCommandDescription(0, cmd.QualifiedName);
                if (cmd is not CommandGroup group || @group.IsExecutableWithoutSubcommands) {
                    _ = cs.GetCommandDescription(0, cmd.QualifiedName);
                    _ = cs.GetCommandUsageExamples(0, cmd.QualifiedName);
                    IEnumerable<CommandArgument> args = cmd.Overloads.SelectMany(o => o.Arguments).Distinct();
                    foreach (CommandArgument arg in args)
                        _ = lcs.GetStringUnsafe(null, arg.Description ?? TranslationKey.str_404.Key);
                }
            } catch (LocalizationException e) {
                Log.Warning(e, "Translation not found");
                succ = false;
            }

        return succ;
    }

}