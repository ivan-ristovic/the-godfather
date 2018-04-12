#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Gambling.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Gambling
{
    [Group("cards"), Module(ModuleType.Gambling)]
    [Description("Manipulate a deck of cards.")]
    [Aliases("deck")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class CardsModule : TheGodfatherBaseModule
    {

        public CardsModule(SharedData shared) : base(shared: shared) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => ResetDeckAsync(ctx);


        #region COMMAND_DECK_DRAW
        [Command("draw"), Module(ModuleType.Gambling)]
        [Description("Draw cards from the top of the deck. If amount of cards is not specified, draws one card.")]
        [Aliases("take")]
        [UsageExample("!deck draw 5")]
        public async Task DrawAsync(CommandContext ctx,
                                   [Description("Amount (in range [1-10]).")] int amount = 1)
        {
            if (!Shared.CardDecks.ContainsKey(ctx.Channel.Id) || Shared.CardDecks[ctx.Channel.Id] == null)
                throw new CommandFailedException($"No deck to deal from. Use command {Formatter.InlineCode("deck")} to open a deck.");

            var deck = Shared.CardDecks[ctx.Channel.Id];

            if (deck.CardCount == 0)
                throw new CommandFailedException($"Current deck has no more cards. Use command {Formatter.InlineCode("deck reset")} to reset the deck.");

            if (amount <= 0 || amount >= 10)
                throw new InvalidCommandUsageException("Cannot draw less than 1 or more than 10 cards...");

            if (deck.CardCount < amount)
                throw new InvalidCommandUsageException($"The deck has only {deck.CardCount} cards...");
            
            await ctx.RespondWithIconEmbedAsync($"{ctx.User.Mention} drew {string.Join(" ", deck.Draw(amount))}", ":ticket:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DECK_RESET
        [Command("reset"), Module(ModuleType.Gambling)]
        [Description("Opens a brand new card deck.")]
        [Aliases("new", "opennew", "open")]
        [UsageExample("!deck reset")]
        public async Task ResetDeckAsync(CommandContext ctx)
        {
            if (Shared.CardDecks.ContainsKey(ctx.Channel.Id))
                throw new CommandFailedException($"A deck is already opened in this channel! If you want to reset it, use {Formatter.InlineCode("!deck new")}");

            Shared.CardDecks[ctx.Channel.Id] = new Deck();
            Shared.CardDecks[ctx.Channel.Id].Shuffle();

            await ctx.RespondWithIconEmbedAsync("A new shuffled deck is opened in this channel!", ":spades:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DECK_SHUFFLE
        [Command("shuffle"), Module(ModuleType.Gambling)]
        [Description("Shuffles current deck.")]
        [Aliases("s", "sh", "mix")]
        [UsageExample("!deck shuffle")]
        public async Task ShuffleDeckAsync(CommandContext ctx)
        {
            if (!Shared.CardDecks.ContainsKey(ctx.Channel.Id) || Shared.CardDecks[ctx.Channel.Id] == null)
                throw new CommandFailedException($"No decks to shuffle. Use command {Formatter.InlineCode("deck")} to open a new shuffled deck.");

            Shared.CardDecks[ctx.Channel.Id].Shuffle();
            await ctx.RespondWithIconEmbedAsync(icon_emoji: ":ticket:")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
