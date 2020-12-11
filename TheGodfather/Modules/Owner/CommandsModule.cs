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
        public CommandsModule(CommandService service)
            : base(service) { }


        #region commands
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);
        #endregion

        #region commands add
        [Command("add")]
        [Aliases("register", "reg", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public Task AddAsync(CommandContext ctx,
                            [RemainingText, Description("desc-code")] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidCommandUsageException(ctx, "cmd-err-cmd-add-cb");

            try {
                Type? t = CSharpCompilationService.CompileCommand(code);
                if (t is null)
                    throw new InvalidCommandUsageException(ctx, "cmd-err-cmd-add-cb");

                ctx.CommandsNext.RegisterCommands(t);
                return ctx.InfoAsync(this.ModuleColor);
            } catch (Exception ex) {
                return ctx.FailAsync("fmt-compile-fail", ex.GetType(), ex.Message);
            }
        }
        #endregion

        #region commands delete
        [Command("delete")]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public Task DeleteAsync(CommandContext ctx,
                               [RemainingText, Description("desc-cmd")] string command)
        {
            Command cmd = ctx.CommandsNext.FindCommand(command, out _);
            if (cmd is null)
                throw new CommandFailedException(ctx, "cmd-name-404");
            ctx.CommandsNext.UnregisterCommands(cmd);
            bool success = this.Service.RemoveCommand(command);
            return success ? ctx.InfoAsync(this.ModuleColor) : ctx.FailAsync("cmd-err-cmd-del");
        }
        #endregion

        #region commands list
        [Command("list")]
        [Aliases("print", "show", "ls", "l", "p")]
        public Task ListAsync(CommandContext ctx)
        {
            return ctx.PaginateAsync(
                "str-cmds",
                ctx.CommandsNext.GetRegisteredCommands().OrderBy(cmd => cmd.QualifiedName),
                cmd => Formatter.InlineCode(cmd.QualifiedName),
                this.ModuleColor,
                10
            );
        }
        #endregion
    }
}
