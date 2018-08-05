#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("steam"), Module(ModuleType.Searches), NotBlocked]
    [Description("Steam commands. Group call searches steam profiles for a given ID.")]
    [Aliases("s", "st")]
    [UsageExamples("!steam profile 123456123")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class SteamModule : TheGodfatherServiceModule<SteamService>
    {

        public SteamModule(SteamService steam, SharedData shared, DBService db)
            : base(steam, shared, db)
        {
            this.ModuleColor = DiscordColor.Blue;
        }


        #region COMMAND_STEAM_PROFILE
        [Command("profile")]
        [Description("Get Steam user information for user based on his ID.")]
        [Aliases("id", "user")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("ID.")] ulong id)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            DiscordEmbed em = await this.Service.GetEmbeddedInfoAsync(id);
            if (em == null) {
                await InformFailureAsync(ctx, "User with such ID does not exist!");
                return;
            }

            await ctx.RespondAsync(embed: em);
        }
        #endregion
    }
}