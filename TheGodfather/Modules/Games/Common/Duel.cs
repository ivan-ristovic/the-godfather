#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games
{
    public class Duel : Game
    {
        #region PUBLIC_FIELDS
        public string FinishingMove { get; private set; }
        #endregion

        #region PRIVATE_FIELDS
        private DiscordUser _p1;
        private DiscordUser _p2;
        private DiscordMessage _msg;
        private string _hp1bar = null;
        private string _hp2bar = null;
        private bool _pot1used = false;
        private bool _pot2used = false;
        private int _hp1 = 5;
        private int _hp2 = 5;
        private Random _rand = new Random();
        private StringBuilder _events = new StringBuilder();
        #endregion


        public Duel(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser p1, DiscordUser p2)
            : base(interactivity, channel)
        {
            _p1 = p1;
            _p2 = p2;
        }
        

        public async Task StartAsync()
        {
            UpdateHpBars();
            
            _msg = await _channel.SendMessageAsync($"{_p1.Mention} {_hp1bar} {EmojiUtil.DuelSwords} {_hp2bar} {_p2.Mention}")
                .ConfigureAwait(false);

            while (_hp1 > 0 && _hp2 > 0) {
                await AdvanceAsync()
                    .ConfigureAwait(false);
            }

            Winner = _hp1 > 0 ? _p1 : _p2;

            await _channel.SendMessageAsync(EmojiUtil.DuelSwords + " FINISH HIM! " + EmojiUtil.DuelSwords)
                .ConfigureAwait(false);
            FinishingMove = await WaitForFinishingMoveAsync()
                .ConfigureAwait(false);
        }

        private async Task AdvanceAsync()
        {
            DealDamage();

            await WaitForPotionUseAsync()
                .ConfigureAwait(false);

            UpdateHpBars();

            _msg = await _msg.ModifyAsync($"{_p1.Mention} {_hp1bar} {EmojiUtil.DuelSwords} {_hp2bar} {_p2.Mention}", embed: new DiscordEmbedBuilder() {
                Title = "CNN LIVE DUEL COVERAGE",
                Description = _events.ToString(),
                Color = DiscordColor.Chartreuse
            }.Build()).ConfigureAwait(false);
        }

        private void UpdateHpBars()
        {
            _hp1bar = string.Join("", Enumerable.Repeat(EmojiUtil.WhiteSquare, _hp1)) + string.Join("", Enumerable.Repeat(EmojiUtil.BlackSquare, 5 - _hp1));
            _hp2bar = string.Join("", Enumerable.Repeat(EmojiUtil.BlackSquare, 5 - _hp2)) + string.Join("", Enumerable.Repeat(EmojiUtil.WhiteSquare, _hp2));
        }

        private void DealDamage()
        {
            int damage = 1;
            if (_rand.Next() % 2 == 0) {
                _events.AppendLine($"{_p1.Username} {EmojiUtil.GetRandomDuelWeapon(_rand)} {_p2.Username}");
                _hp2 -= damage;
            } else {
                _events.AppendLine($"{_p2.Username} {EmojiUtil.GetRandomDuelWeapon(_rand)} {_p1.Username}");
                _hp1 -= damage;
            }
        }

        private async Task WaitForPotionUseAsync()
        {
            var mctx = await _interactivity.WaitForMessageAsync(
                msg =>
                    msg.ChannelId == _channel.Id && msg.Content.ToLower() == "hp" &&
                    ((!_pot1used && msg.Author.Id == _p1.Id) || (!_pot2used && msg.Author.Id == _p2.Id))
                , TimeSpan.FromSeconds(2)
            ).ConfigureAwait(false);
            if (mctx != null) {
                if (mctx.User.Id == _p1.Id && !_pot1used) {
                    _hp1 = (_hp1 + 1 > 5) ? 5 : _hp1 + 1;
                    _pot1used = true;
                    _events.AppendLine($"{_p1.Username} {EmojiUtil.Syringe}");
                } else if (mctx.User.Id == _p2.Id && !_pot2used) {
                    _hp2 = (_hp2 + 1 > 5) ? 5 : _hp2 + 1;
                    _pot2used = true;
                    _events.AppendLine($"{_p2.Username} {EmojiUtil.Syringe}");
                }
            }
        }

        private async Task<string> WaitForFinishingMoveAsync()
        {
            var mctx = await _interactivity.WaitForMessageAsync(
                m => m.ChannelId == _channel.Id && m.Author.Id == Winner.Id
            ).ConfigureAwait(false);

            if (mctx != null && !string.IsNullOrWhiteSpace(mctx.Message?.Content))
                return mctx.Message.Content.Trim();
            else
                return null;
        }
    }
}


