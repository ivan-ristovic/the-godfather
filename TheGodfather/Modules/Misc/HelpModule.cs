using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;
using TheGodfather.Attributes;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Misc
{
    [Group("help"), Module(ModuleType.Misc), NotBlocked]
    [Aliases("h", "?", "??", "???")]
    [Cooldown(3, 5, CooldownBucketType.User)]
    public sealed class HelpModule : TheGodfatherServiceModule<CommandService>
    {
        #region help
        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx)
        {
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedTitle("h-title");
                emb.WithLocalizedDescription("fmt-modules", Enum.GetNames<ModuleType>().Select(s => $"• {s}").JoinWith());
                emb.WithLocalizedFooter("h-footer", ctx.Client.CurrentUser.AvatarUrl);
            });
        }

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-module")] ModuleType module)
        {
            Command? cmd = ctx.CommandsNext.FindCommand(module.ToString(), out var _);
            if (cmd is CommandGroup group && group.IsExecutableWithoutSubcommands)
                return this.ExecuteGroupAsync(ctx, module.ToString());

            IReadOnlyList<string> cmds = this.Service.GetCommandsInModule(module);
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(module.ToDiscordColor());
                emb.WithLocalizedTitle("h-title-m", module);
                emb.WithLocalizedDescription(module.ToLocalizedDescriptionKey(), cmds.Select(s => Formatter.InlineCode(s)).JoinWith(", "));
                emb.WithLocalizedFooter("h-footer", ctx.Client.CurrentUser.AvatarUrl);
            });
        }

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-cmd")] params string[] cmd)
            => new CommandsNextExtension.DefaultHelpModule().DefaultHelpAsync(ctx, cmd);
        #endregion
    }
}
