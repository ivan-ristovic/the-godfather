#region USING_DIRECTIVES
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("steam")]
    [Description("Steam commands.")]
    [Aliases("s", "st")]
    [UsageExample("!steam profile 123456123")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class SteamModule : TheGodfatherServiceModule<SteamService>
    {

        public SteamModule(SteamService steam) : base(steam) { }


        #region COMMAND_STEAM_PROFILE
        [Command("profile")]
        [Description("Get Steam user information for user based on his ID.")]
        [Aliases("id", "user")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("ID.")] ulong id)
        {
            var em = await Service.GetEmbeddedResultAsync(id)
                .ConfigureAwait(false);

            if (em == null) {
                await ReplyWithEmbedAsync(ctx, "User with such ID does not exist!", ":negative_squared_cross_mark:")
                    .ConfigureAwait(false);
                return;
            }

            await ctx.RespondAsync(embed: em)
                .ConfigureAwait(false);
        }
        #endregion
    }
}