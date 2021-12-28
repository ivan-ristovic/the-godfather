using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TexasHoldem.Logic.Cards;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Services;

namespace TheGodfather.Modules.Games
{
    [Group("deck"), Module(ModuleType.Games), NotBlocked]
    [Aliases("cards")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class CardsModule : TheGodfatherModule
    {
        #region deck
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ResetDeckAsync(ctx);
        #endregion

        #region deck draw
        [Command("draw")]
        [Aliases("take")]
        public Task DrawAsync(CommandContext ctx,
                             [Description(TranslationKey.desc_draw_amount)] int amount = 1)
        {
            Deck deck = CardDecksService.GetDeckForChannel(ctx.Channel.Id);

            if (amount is < 1 or > 10)
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_deck_amount(1, 10));

            IReadOnlyList<Card> drawn = deck.DrawCards(amount);
            if (!drawn.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_deck_empty);

            return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Dice, TranslationKey.fmt_deck_draw(ctx.User.Mention, drawn.JoinWith(" ")));
        }
        #endregion

        #region deck reset
        [Command("reset")]
        [Aliases("new", "opennew", "open")]
        public Task ResetDeckAsync(CommandContext ctx)
        {
            CardDecksService.ResetDeckForChannel(ctx.Channel.Id);
            return ctx.InfoAsync(this.ModuleColor);
        }
        #endregion
    }
}
