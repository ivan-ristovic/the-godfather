#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Search
{
    [Cooldown(5, 10, CooldownBucketType.Channel)]
    [NotBlocked]
    public class SearchesModule : TheGodfatherModule
    {
        public SearchesModule(SharedData shared, DBService db)
            : base(shared, db)
        {

        }

        [Command("ipstack"), Module(ModuleType.Searches)]
        [Description("Retrieve IP geolocation information.")]
        [Aliases("ip", "geolocation", "iplocation", "iptracker", "iptrack", "trackip", "iplocate")]
        [UsageExamples("!ipstack 123.123.123.123")]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("IP.")] string ip)
        {
            IpInfo info = await IpGeolocationService.GetInfoForIpAsync(ip);

            if (!info.Success)
                throw new CommandFailedException($"Retrieving IP geolocation info failed! Details: {info.ErrorMessage}");

            await ctx.RespondAsync(embed: info.ToDiscordEmbed())
                .ConfigureAwait(false);
        }
    }
}
