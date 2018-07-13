#region USING_DIRECTIVES
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("steam"), Module(ModuleType.Searches)]
    [Description("Steam commands.")]
    [Aliases("s", "st")]
    [UsageExamples("!steam profile 123456123")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class SteamModule : TheGodfatherServiceModule<SteamService>
    {

        public SteamModule(SteamService steam) : base(steam) { }


        #region COMMAND_STEAM_PROFILE
        [Command("profile"), Module(ModuleType.Searches)]
        [Description("Get Steam user information for user based on his ID.")]
        [Aliases("id", "user")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("ID.")] ulong id)
        {
            var em = await _Service.GetEmbeddedInfoAsync(id)
                .ConfigureAwait(false);

            if (em == null) {
                await ctx.InformFailureAsync("User with such ID does not exist!")
                    .ConfigureAwait(false);
                return;
            }

            await ctx.RespondAsync(embed: em)
                .ConfigureAwait(false);
        }
        #endregion
    }
}