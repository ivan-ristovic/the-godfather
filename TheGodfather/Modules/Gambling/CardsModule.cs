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
            => await ResetDeckAsync(ctx).ConfigureAwait(false);


        #region COMMAND_DECK_DRAW
        [Command("draw")]
        [Description("Draw cards from the top of the deck. If amount of cards is not specified, draws one card.")]
        [Aliases("take")]
        [UsageExample("!deck draw 5")]
        public async Task DrawAsync(CommandContext ctx,
                                   [Description("Amount (in range [1-10]).")] int amount = 1)
        {
            if (!SharedData.CardDecks.ContainsKey(ctx.Channel.Id) || SharedData.CardDecks[ctx.Channel.Id] == null)
                throw new CommandFailedException($"No deck to deal from. Type {Formatter.InlineCode("!deck")} to open a deck.");

            var deck = SharedData.CardDecks[ctx.Channel.Id];

            if (deck.CardCount == 0)
                throw new CommandFailedException($"Current deck has no more cards. Type {Formatter.InlineCode("!deck reset")} to reset the deck.");

            if (amount <= 0 || amount >= 10)
                throw new InvalidCommandUsageException("Cannot draw less than 1 or more than 10 cards...");

            if (deck.CardCount < amount)
                throw new InvalidCommandUsageException($"The deck has only {deck.CardCount} cards...");
            
            await ReplyWithEmbedAsync(ctx, $"{ctx.User.Mention} drew {string.Join(" ", deck.Draw(amount))}", ":ticket:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DECK_RESET
        [Command("reset")]
        [Description("Opens a brand new card deck.")]
        [Aliases("new", "opennew", "open")]
        [UsageExample("!deck draw 5")]
        public async Task ResetDeckAsync(CommandContext ctx)
        {
            if (SharedData.CardDecks.ContainsKey(ctx.Channel.Id))
                throw new CommandFailedException($"A deck is already opened in this channel! If you want to reset it, use {Formatter.InlineCode("!deck new")}");

            SharedData.CardDecks[ctx.Channel.Id] = new Deck();
            SharedData.CardDecks[ctx.Channel.Id].Shuffle();

            await ReplyWithEmbedAsync(ctx, "A new shuffled deck is opened in this channel!", ":spades:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DECK_SHUFFLE
        [Command("shuffle")]
        [Description("Shuffles current deck.")]
        [Aliases("s", "sh", "mix")]
        [UsageExample("!deck shuffle")]
        public async Task ShuffleDeckAsync(CommandContext ctx)
        {
            if (!SharedData.CardDecks.ContainsKey(ctx.Channel.Id) || SharedData.CardDecks[ctx.Channel.Id] == null)
                throw new CommandFailedException($"No decks to shuffle. Type {Formatter.InlineCode("!deck")} to open a new shuffled deck.");

            SharedData.CardDecks[ctx.Channel.Id].Shuffle();
            await ReplyWithEmbedAsync(ctx, emojistr: ":ticket:")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
