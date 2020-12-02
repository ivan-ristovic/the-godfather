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
    [Group("insult"), Module(ModuleType.Misc), NotBlocked]
    [Aliases("burn", "ins", "roast")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class InsultModule : TheGodfatherServiceModule<InsultService>
    {
        public InsultModule(InsultService service)
            : base(service) { }


        #region insult
        [GroupCommand, Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("desc-user")] DiscordUser? user = null)
        {
            user ??= ctx.User;
            if (user == ctx.Client.CurrentUser)
                user = ctx.User;

            string insult = await this.Service.FetchInsultAsync(user.Username);
            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithDescription(Formatter.Italic(insult));
                emb.WithLocalizedFooter("fmt-powered-by", user.AvatarUrl, InsultService.Provider);
            });
        }

        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("desc-insult-target")] string target)
        {
            string insult = await this.Service.FetchInsultAsync(target);
            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithDescription(Formatter.Italic(insult));
                emb.WithLocalizedFooter("fmt-powered-by", null, InsultService.Provider);
            });
        }
        #endregion
    }
}
