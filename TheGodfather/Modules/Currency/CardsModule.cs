#region USING_DIRECTIVES
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using TexasHoldem.Logic.Cards;
#endregion

namespace TheGodfather.Modules.Currency
{
    [Group("cards"), Module(ModuleType.Currency)]
    [Description("Manipulate a deck of cards.")]
    [Aliases("deck")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class CardsModule : TheGodfatherModule
    {
        public static ConcurrentDictionary<ulong, Deck> Decks { get; internal set; } = new ConcurrentDictionary<ulong, Deck>();


        public CardsModule(SharedData shared) : base(shared: shared) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => ResetDeckAsync(ctx);


        #region COMMAND_DECK_DRAW
        [Command("draw"), Module(ModuleType.Currency)]
        [Description("Draw cards from the top of the deck. If amount of cards is not specified, draws one card.")]
        [Aliases("take")]
        [UsageExamples("!deck draw 5")]
        public async Task DrawAsync(CommandContext ctx,
                                   [Description("Amount (in range [1-10]).")] int amount = 1)
        {
            if (!Decks.ContainsKey(ctx.Channel.Id) || Decks[ctx.Channel.Id] == null)
                throw new CommandFailedException($"No deck to deal from. Use command {Formatter.InlineCode("deck")} to open a deck.");

            var deck = Decks[ctx.Channel.Id];
            
            if (amount <= 0 || amount >= 10)
                throw new InvalidCommandUsageException("Cannot draw less than 1 or more than 10 cards...");

            var drawn = deck.DrawCards(amount);
            if (!drawn.Any())
                throw new CommandFailedException($"Current deck doesn't have enough cards. Use command {Formatter.InlineCode("deck reset")} to reset the deck.");

            await ctx.InformSuccessAsync($"{ctx.User.Mention} drew {string.Join(" ", drawn)}", ":ticket:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DECK_RESET
        [Command("reset"), Module(ModuleType.Currency)]
        [Description("Opens a brand new card deck.")]
        [Aliases("new", "opennew", "open")]
        [UsageExamples("!deck reset")]
        public async Task ResetDeckAsync(CommandContext ctx)
        {
            if (Decks.ContainsKey(ctx.Channel.Id))
                throw new CommandFailedException($"A deck is already opened in this channel! If you want to reset it, use {Formatter.InlineCode("!deck new")}");

            Decks[ctx.Channel.Id] = new Deck();

            await ctx.InformSuccessAsync("A new shuffled deck is opened in this channel!", ":spades:")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
