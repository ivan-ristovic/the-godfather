using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Owner.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Owner
{
    [Group("commands"), Module(ModuleType.Owner), Hidden]
    [Aliases("cmds", "cmd")]
    [RequireOwner]
    public sealed class CommandsModule : TheGodfatherServiceModule<CommandService>
    {
        #region commands
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);
        #endregion

        #region commands add
        [Command("add")]
        [Aliases("register", "reg", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public Task AddAsync(CommandContext ctx,
                            [RemainingText, Description(TranslationKey.desc_code)] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_cmd_add_cb);

            try {
                Type? t = CSharpCompilationService.CompileCommand(code);
                if (t is null)
                    throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_cmd_add_cb);

                ctx.CommandsNext.RegisterCommands(t);
                return ctx.InfoAsync(this.ModuleColor);
            } catch (Exception ex) {
                return ctx.FailAsync(TranslationKey.fmt_compile_fail(ex.GetType(), ex.Message));
            }
        }
        #endregion

        #region commands delete
        [Command("delete")]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public Task DeleteAsync(CommandContext ctx,
                               [RemainingText, Description(TranslationKey.desc_cmd)] string command)
        {
            Command cmd = ctx.CommandsNext.FindCommand(command, out _);
            if (cmd is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_name_404(command));
            ctx.CommandsNext.UnregisterCommands(cmd);
            bool success = this.Service.RemoveCommand(cmd.QualifiedName);
            return success ? ctx.InfoAsync(this.ModuleColor) : ctx.FailAsync(TranslationKey.cmd_err_cmd_del(cmd.QualifiedName));
        }
        #endregion

        #region commands list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public Task ListAsync(CommandContext ctx)
        {
            return ctx.PaginateAsync(
                TranslationKey.str_cmds,
                ctx.CommandsNext.GetRegisteredCommands().OrderBy(cmd => cmd.QualifiedName),
                cmd => Formatter.InlineCode(cmd.QualifiedName),
                this.ModuleColor,
                10
            );
        }
        #endregion
    }
}
