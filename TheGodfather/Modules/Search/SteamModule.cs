#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("steam"), Module(ModuleType.Searches), NotBlocked]
    [Description("Steam commands. Group call searches steam profiles for a given ID.")]
    [Aliases("s", "st")]
    [UsageExampleArgs("123456123")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class SteamModule : TheGodfatherServiceModule<SteamService>
    {

        public SteamModule(SteamService steam, SharedData shared, DatabaseContextBuilder db)
            : base(steam, shared, db)
        {
            this.ModuleColor = DiscordColor.Blue;
        }


        #region COMMAND_STEAM_PROFILE
        [Command("profile")]
        [Description("Get Steam user information for user based on his ID.")]
        [Aliases("id", "user")]
        [UsageExampleArgs("123456123")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("ID.")] ulong id)
        {
            if (this.Service.IsDisabled)
                throw new ServiceDisabledException();

            DiscordEmbed em = await this.Service.GetEmbeddedInfoAsync(id);
            if (em is null) {
                await this.InformFailureAsync(ctx, "User with such ID does not exist!");
                return;
            }

            await ctx.RespondAsync(embed: em);
        }
        #endregion
    }
}