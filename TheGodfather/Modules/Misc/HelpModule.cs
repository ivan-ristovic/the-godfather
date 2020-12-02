using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Attributes;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Misc
{
    public class HelpModuleImpl : TheGodfatherServiceModule<CommandService>
    {
        public HelpModuleImpl(CommandService cs)
            : base(cs) { }


        #region help
        [Command("help"), Priority(2)]
        [Aliases("h", "?", "??", "???")]
        public Task HelpAsync(CommandContext ctx)
        {
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedTitle("h-title");
                emb.WithLocalizedDescription("fmt-modules", Enum.GetNames<ModuleType>().Select(s => $"• {s}").Separate());
                emb.WithLocalizedFooter("h-footer", ctx.Client.CurrentUser.AvatarUrl);
            });
        }

        [Command("help"), Priority(1)]
        public Task HelpAsync(CommandContext ctx,
                             [Description("desc-module")] ModuleType module)
        {
            IReadOnlyList<string> cmds = this.Service.GetCommandsInModule(module);
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(module.ToDiscordColor());
                emb.WithLocalizedTitle("h-title-m", module);
                emb.WithLocalizedDescription(module.ToLocalizedDescriptionKey(), cmds.Select(s => Formatter.InlineCode(s)).Separate(", "));
                emb.WithLocalizedFooter("h-footer", ctx.Client.CurrentUser.AvatarUrl);
            });
        }

        [Command("help"), Priority(0)]
        public Task HelpAsync(CommandContext ctx, 
                             [RemainingText, Description("desc-cmd")] params string[] cmd)
            => new CommandsNextExtension.DefaultHelpModule().DefaultHelpAsync(ctx, cmd);
        #endregion
    }
}
