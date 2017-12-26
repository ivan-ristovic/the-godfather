#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Commands.Games
{
    //public partial class CommandsGamble
    //{
        [Group("cards", CanInvokeWithoutSubcommand = false)]
        [Description("Deck manipulation commands.")]
        [Aliases("deck")]
        [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
        [PreExecutionCheck]
        public class CommandsCards
        {
            #region PRIVATE_FIELDS
            private List<string> _deck = null;
            #endregion


            #region COMMAND_DECK_DRAW
            [Command("draw")]
            [Description("Draw cards from the top of the deck.")]
            [Aliases("take")]
            public async Task DrawAsync(CommandContext ctx,
                                       [Description("Amount.")] int amount = 1)
            {
                if (_deck == null || _deck.Count == 0)
                    throw new CommandFailedException($"No deck to deal from. Use {Formatter.InlineCode("!deck new")}");

                if (amount <= 0 || amount >= 10 || _deck.Count < amount)
                    throw new InvalidCommandUsageException("Cannot draw that amount of cards...", new ArgumentException());

                string hand = string.Join(" ", _deck.Take(amount));
                _deck.RemoveRange(0, amount);

                await ctx.RespondAsync(hand)
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_DECK_RESET
            [Command("reset")]
            [Description("Opens a brand new card deck.")]
            [Aliases("new", "opennew")]
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
        }

    //}
}
