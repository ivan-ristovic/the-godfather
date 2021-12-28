using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc;

[Group("insult")][Module(ModuleType.Misc)][NotBlocked]
[Aliases("burn", "ins", "roast")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class InsultModule : TheGodfatherServiceModule<InsultService>
{
    #region insult
    [GroupCommand][Priority(1)]
    public async Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordUser? user = null)
    {
        user ??= ctx.User;
        if (user == ctx.Client.CurrentUser)
            user = ctx.User;

        string insult = await this.Service.FetchInsultAsync(user.Username);
        await ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithDescription(Formatter.Italic(insult));
            emb.WithLocalizedFooter(TranslationKey.fmt_powered_by(InsultService.Provider), user.AvatarUrl);
        });
    }

    [GroupCommand][Priority(0)]
    public async Task ExecuteGroupAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_insult_target)] string target)
    {
        string insult = await this.Service.FetchInsultAsync(target);
        await ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithDescription(Formatter.Italic(insult));
            emb.WithLocalizedFooter(TranslationKey.fmt_powered_by(InsultService.Provider), null);
        });
    }
    #endregion
}