#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfatherBot.Modules.Games
{
    [Group("cards", CanInvokeWithoutSubcommand = false)]
    [Description("Deck manipulation commands")]
    [Aliases("deck")]
    public class CommandsCards
    {
        #region PRIVATE_FIELDS
        private List<string> _deck = null;
        #endregion


        #region COMMAND_DECK_DEAL
        [Command("deal"), Description("Deal hand from the top of the deck.")]
        public async Task DealHand(CommandContext ctx, [Description("Ammount")] int ammount = 5)
        {
            if (_deck == null || _deck.Count == 0)
                throw new Exception("No deck to deal from. Use ``!deck new``");

            if (ammount <= 0 || ammount >= 10 || _deck.Count < ammount)
                throw new ArgumentException("Cannot draw that ammount of cards...");

            string hand = "";
            for (int i = 0; i < ammount; i++) {
                hand += _deck[0] + " ";
                _deck.RemoveAt(0);
            }

            await ctx.RespondAsync(hand);
        }
        #endregion

        #region COMMAND_DECK_DRAW
        [Command("draw"), Description("Draw a card from the current deck.")]
        public async Task Draw(CommandContext ctx)
        {
            if (_deck == null || _deck.Count == 0)
                throw new Exception("No deck to draw from.");

            await ctx.RespondAsync(_deck[0]);
            _deck.RemoveAt(0);
        }
        #endregion

        #region COMMAND_DECK_RESET
        [Command("reset"), Description("Opens a brand new card deck.")]
        [Aliases("new")]
        public async Task Reset(CommandContext ctx)
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

            await ctx.RespondAsync("New deck opened!");
        }
        #endregion

        #region COMMAND_DECK_SHUFFLE
        [Command("shuffle"), Description("Shuffle current deck.")]
        public async Task Shuffle(CommandContext ctx)
        {
            var shuffled = _deck.OrderBy(a => Guid.NewGuid()).ToList();
            _deck.Clear();
            _deck.AddRange(shuffled);
            await ctx.RespondAsync("Deck shuffled.");
        }
        #endregion

    }
}
