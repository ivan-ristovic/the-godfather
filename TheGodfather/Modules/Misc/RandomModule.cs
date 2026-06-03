using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc;

[Group("random")]
[Alias("rnd", "rand")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class RandomModule : TheGodfatherModule
{
    private readonly RandomService random;


    public RandomModule(LocalizationService localization, RandomService random)
        : base(localization, new Color(0x007FFF))
    {
        this.random = random;
    }


    [Command("choice")]
    [Alias("select", "choose")]
    public Task ChooseAsync([Remainder] string list)
    {
        if (string.IsNullOrWhiteSpace(list))
            throw new LocalizedCommandException(TranslationKey.cmd_err_choice);

        string choice = this.random.Choice(list);
        return this.ReplyAsync(embed: new EmbedBuilder {
            Description = $"{Emojis.Dice} {Format.Sanitize(choice)}",
            Color = this.ModuleColor
        }.Build());
    }

    [Command("raffle")]
    [Alias("chooseuser")]
    [RequireContext(ContextType.Guild)]
    public Task RaffleAsync(IRole? role = null)
    {
        IEnumerable<SocketGuildUser> members = this.Context.Guild.Users;
        if (role is not null)
            members = members.Where(member => member.Roles.Any(r => r.Id == role.Id));

        SocketGuildUser[] candidates = members.Where(member => !member.IsBot).ToArray();
        if (!candidates.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_choice_none);

        SocketGuildUser raffled = new SecureRandom().ChooseRandomElement(candidates);
        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithLocalizedDescription(TranslationKey.fmt_raffle(raffled.Mention));
        });
    }
}
