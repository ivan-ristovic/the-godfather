using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.Extensions;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc
{
    [Group("starboard"), Module(ModuleType.Misc), NotBlocked]
    [Aliases("star", "sb")]
    [RequireGuild, RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class StarboardModule : TheGodfatherServiceModule<StarboardService>
    {
        #region starboard
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            DiscordChannel? starChn = null;
            DiscordEmoji? starEmoji = null;
            if (this.Service.IsStarboardEnabled(ctx.Guild.Id, out string? emoji)) {
                ulong cid = await this.Service.GetStarboardChannelAsync(ctx.Guild.Id);
                starChn = ctx.Guild.GetChannel(cid);
                try {
                    starEmoji = DiscordEmoji.FromName(ctx.Client, emoji);
                } catch {

                }
            }

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedTitle("str-starboard");
                emb.AddLocalizedField("str-status", emoji is null ? "str-disabled" : "str-enabled", inline: true);
                emb.AddLocalizedTitleField("str-star", starEmoji, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-chn", starChn?.Mention, inline: true, unknown: false);
            });
        }
        #endregion

        #region starboard enable
         
        #endregion

        #region starboard disable
        #endregion
    }
}
