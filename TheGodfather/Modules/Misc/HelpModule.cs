using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Misc
{
    public class HelpModuleImpl : TheGodfatherServiceModule<CommandService>
    {
        public HelpModuleImpl(CommandService cs)
            : base(cs) { }


        [Command("help")]
        [Aliases("?", "??", "???")]
        public Task HelpAsync(CommandContext ctx, [RemainingText] params string[] command)
            => new CommandsNextExtension.DefaultHelpModule().DefaultHelpAsync(ctx, command);
    }
}
