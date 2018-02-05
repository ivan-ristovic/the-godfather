#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Gambling.Cards;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Gambling
{
    [Group("cards")]
    [Description("Manipulate a deck of cards.")]
    [Aliases("deck")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class CardsModule : GodfatherBaseModule
    {

        public CardsModule(SharedData shared) : base(shared: shared) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            if (SharedData.CardDecks.ContainsKey(ctx.Channel.Id))
                throw new CommandFailedException($"A deck is already opened in this channel! If you want to reset it, use {Formatter.InlineCode("!deck new")}");

            SharedData.CardDecks[ctx.Channel.Id] = new Deck();
            SharedData.CardDecks[ctx.Channel.Id].Shuffle();

            await ReplySuccessAsync(ctx, "A new shuffled deck is opened in this channel!", ":spades:")
                .ConfigureAwait(false);
        }


        #region COMMAND_DECK_DRAW
        [Command("draw")]
        [Description("Draw cards from the top of the deck.")]
        [Aliases("take")]
        public async Task DrawAsync(CommandContext ctx,
                                   [Description("Amount (in range [1-10]).")] int amount = 1)
        {
            if (!SharedData.CardDecks.ContainsKey(ctx.Channel.Id))
                throw new CommandFailedException($"No deck to deal from. Use {Formatter.InlineCode("!deck new")}");

            var deck = SharedData.CardDecks[ctx.Channel.Id];

            if (deck == null || deck.CardCount == 0)
                throw new CommandFailedException($"No deck to deal from. Use {Formatter.InlineCode("!deck new")}");

            if (amount <= 0 || amount >= 10)
                throw new InvalidCommandUsageException("Cannot draw less than 1 or more than 10 cards...");

            if (deck.CardCount < amount)
                throw new InvalidCommandUsageException($"The deck has only {deck.CardCount} cards...");

            await ctx.RespondAsync(string.Join(" ", deck.Draw(amount)))
                .ConfigureAwait(false);
        }
        #endregion
        /*
        #region COMMAND_DECK_RESET
        [Command("reset")]
        [Description("Opens a brand new card deck.")]
        [Aliases("new", "opennew", "open")]
        public async Task ResetDeckAsync(CommandContext ctx)
        {
            _deck = new List<string>();
            char[] suit = { '♠', '♥', '♦', '♣' };
            foreach (char s in suit) {
                _deck.Add("A" + s);
                for (int i = 2; i < 10; i++) {
                    _deck.Add(i.ToString() + s);
                }
                _deck.Add("T" + s);
                _deck.Add("J" + s);
                _deck.Add("Q" + s);
                _deck.Add("K" + s);
            }

            await ctx.RespondAsync("New deck opened!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DECK_SHUFFLE
        [Command("shuffle")]
        [Description("Shuffle current deck.")]
        [Aliases("s", "sh", "mix")]
        public async Task ShuffleDeckAsync(CommandContext ctx)
        {
            if (_deck == null || _deck.Count == 0)
                throw new CommandFailedException("No deck to shuffle.");

            _deck = _deck.OrderBy(a => Guid.NewGuid()).ToList();
            await ctx.RespondAsync("Deck shuffled.")
                .ConfigureAwait(false);
        }
        #endregion
    */
    }
}
