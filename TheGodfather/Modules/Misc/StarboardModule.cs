using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Attributes;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc
{
//    [Group("starboard"), Module(ModuleType.Misc), NotBlocked]
//    [Aliases("star", "sb")]
    [RequireGuild, RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class StarboardModule : TheGodfatherServiceModule<StarboardService>
    {
        #region starboard
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
        {
            return Task.CompletedTask;
        }
        #endregion
    }
}
