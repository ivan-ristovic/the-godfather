using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc;

[Group("insult")]
[Alias("burn", "ins", "roast")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class InsultModule : TheGodfatherModule
{
    private readonly InsultService insults;


    public InsultModule(LocalizationService localization, InsultService insults)
        : base(localization, new Color(0x007FFF))
    {
        this.insults = insults;
    }


    [Command("")]
    [Priority(1)]
    public async Task ExecuteGroupAsync(IUser? user = null)
    {
        user ??= this.Context.User;
        if (user.Id == this.Context.Client.CurrentUser.Id)
            user = this.Context.User;

        string insult = await this.insults.FetchInsultAsync(user.Username);
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithDescription(Format.Italics(insult));
            emb.WithLocalizedFooter(TranslationKey.fmt_powered_by(InsultService.Provider), user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
        });
    }

    [Command("")]
    [Priority(0)]
    public async Task ExecuteGroupAsync([Remainder] string target)
    {
        string insult = await this.insults.FetchInsultAsync(target);
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithDescription(Format.Italics(insult));
            emb.WithLocalizedFooter(TranslationKey.fmt_powered_by(InsultService.Provider), null);
        });
    }
}
