using Discord.Commands;
using TexasHoldem.Logic.Cards;
using TheGodfather.Attributes;
using TheGodfather.Modules.Games.Services;

namespace TheGodfather.Modules.Games;

[Group("deck")]
[Alias("cards")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class CardsModule : TheGodfatherModule
{
    public CardsModule(LocalizationService localization)
        : base(localization, new Color(0x9B59B6)) { }


    [Command("")]
    public Task ExecuteGroupAsync()
        => this.ResetDeckAsync();

    [Command("draw")]
    [Alias("take")]
    public async Task DrawAsync(int amount = 1)
    {
        if (amount is < 1 or > 10)
            throw new LocalizedCommandException(TranslationKey.cmd_err_deck_amount(1, 10));

        Deck deck = CardDecksService.GetDeckForChannel(this.Context.Channel.Id);
        IReadOnlyList<Card> drawn = deck.DrawCards(amount);
        if (!drawn.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_deck_empty);

        await this.ImpInfoAsync(this.ModuleColor, Emojis.Dice, TranslationKey.fmt_deck_draw(this.Context.User.Mention, drawn.JoinWith(" ")));
    }

    [Command("reset")]
    [Alias("new", "opennew", "open")]
    public Task ResetDeckAsync()
    {
        CardDecksService.ResetDeckForChannel(this.Context.Channel.Id);
        return this.InfoAsync(this.ModuleColor);
    }
}
