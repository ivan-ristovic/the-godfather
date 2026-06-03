using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Currency.Services;

namespace TheGodfather.Modules.Currency;

[Group("work")]
[Alias("job")]
[RequireContext(ContextType.Guild)]
[Cooldown(1, 60, CooldownBucketType.User)]
public sealed class WorkModule : TheGodfatherModule
{
    private readonly WorkService work;
    private readonly GuildConfigService guildConfig;


    public WorkModule(LocalizationService localization, WorkService work, GuildConfigService guildConfig)
        : base(localization, new Color(0x2ECC71))
    {
        this.work = work;
        this.guildConfig = guildConfig;
    }


    [Command("")]
    public async Task ExecuteGroupAsync()
    {
        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        string workString = await this.work.WorkAsync(this.Context.Guild.Id, this.Context.User.Id, currency);
        await this.RespondWithWorkString(workString, TranslationKey.str_work_footer);
    }

    [Command("streets")]
    [Alias("prostitute")]
    [Cooldown(1, 120, CooldownBucketType.User)]
    public async Task StreetsAsync()
    {
        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        string workString = await this.work.StreetsAsync(this.Context.Guild.Id, this.Context.User.Id, currency);
        await this.RespondWithWorkString(workString, TranslationKey.str_work_streets_footer);
    }

    [Command("crime")]
    [Cooldown(1, 300, CooldownBucketType.User)]
    public async Task CrimeAsync()
    {
        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        string workString = await this.work.CrimeAsync(this.Context.Guild.Id, this.Context.User.Id, currency);
        await this.RespondWithWorkString(workString, TranslationKey.str_work_crime_footer);
    }

    private Task RespondWithWorkString(string text, TranslationKey footer)
    {
        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithDescription($"{this.Context.User.Mention} {text}");
            emb.WithLocalizedFooter(footer, this.Context.User.GetAvatarUrl());
        });
    }
}
