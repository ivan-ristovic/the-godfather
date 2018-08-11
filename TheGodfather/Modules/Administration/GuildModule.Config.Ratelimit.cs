#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class GuildModule
    {
        public partial class GuildConfigModule
        {
            [Group("ratelimit")]
            [Description("Prevents users from posting more than specified messages in short period of time.")]
            [Aliases("rl", "rate")]
            [UsageExamples("!guild cfg ratelimit",
                           "!guild cfg ratelimit on",
                           "!guild cfg ratelimit off")]
            public class RatelimitModule : TheGodfatherServiceModule<RatelimitService>
            {

                public RatelimitModule(RatelimitService service, SharedData shared, DBService db)
                    : base(service, shared, db)
                {
                    this.ModuleColor = DiscordColor.Rose;
                }


                [GroupCommand, Priority(1)]
                public async Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Enable?")] bool enable)
                {

                }

                [GroupCommand, Priority(0)]
                public async Task ExecuteGroupAsync(CommandContext ctx)
                {

                }


                // TODO command sensitivity
            }
        }
    }
}
