#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Helpers.Collections;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
#endregion

namespace TheGodfather.Commands.Games
{
    public class Duel
    {
        #region PUBLIC_FIELDS
        public DiscordUser Winner { get; private set; }
        public string FinishingMove { get; private set; }
        #endregion

        #region STATIC_FIELDS
        public static bool GameExistsInChannel(ulong cid) => _channels.Contains(cid);
        private static ConcurrentHashSet<ulong> _channels = new ConcurrentHashSet<ulong>();
        private static string[] weapons = { ":hammer:", ":dagger:", ":pick:", ":bomb:", ":guitar:", ":fire:" };
        #endregion

        #region PRIVATE_FIELDS
        private DiscordClient _client;
        private ulong _cid;
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
        private string _events = "";
        #endregion


        public Duel(DiscordClient client, ulong cid, DiscordUser p1, DiscordUser p2)
        {
            _channels.Add(_cid);
            _client = client;
            _cid = cid;
            _p1 = p1;
            _p2 = p2;
        }


        public async Task PlayAsync()
        {
            UpdateHpBars();

            var chn = await _client.GetChannelAsync(_cid)
                .ConfigureAwait(false);

            var e = DiscordEmoji.FromName(_client, ":crossed_swords:");
            _msg = await chn.SendMessageAsync($"{_p1.Mention} {_hp1bar} {e} {_hp2bar} {_p2.Mention}")
                .ConfigureAwait(false);

            while (_hp1 > 0 && _hp2 > 0) {
                await AdvanceAsync()
                    .ConfigureAwait(false);
            }

            if (_hp1 > 0)
                Winner = _p1;
            else
                Winner = _p2;

            await chn.SendMessageAsync(e + " FINISH HIM! ")
                .ConfigureAwait(false);
            FinishingMove = await CheckForFinishingMoveAsync()
                .ConfigureAwait(false);

            _channels.TryRemove(_cid);
        }

        private async Task AdvanceAsync()
        {
            DealDamage();

            await CheckForPotionUseAsync()
                .ConfigureAwait(false);

            UpdateHpBars();

            _msg = await _msg.ModifyAsync($"{_p1.Mention} {_hp1bar} :crossed_swords: {_hp2bar} {_p2.Mention}" + _events)
                .ConfigureAwait(false);
        }

        private void UpdateHpBars()
        {
            _hp1bar = string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(_client, ":white_large_square:"), _hp1)) + string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(_client, ":black_large_square:"), 5 - _hp1));
            _hp2bar = string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(_client, ":black_large_square:"), 5 - _hp2)) + string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(_client, ":white_large_square:"), _hp2));
        }

        private void DealDamage()
        {
            int damage = 1;
            if (_rand.Next() % 2 == 0) {
                _events += $"\n{_p1.Username} {weapons[_rand.Next(weapons.Length)]} {_p2.Username}";
                _hp2 -= damage;
            } else {
                _events += $"\n{_p2.Username} {weapons[_rand.Next(weapons.Length)]} {_p1.Username}";
                _hp1 -= damage;
            }
        }

        private async Task CheckForPotionUseAsync()
        {
            var interactivity = _client.GetInteractivityModule();
            var reply = await interactivity.WaitForMessageAsync(
                msg =>
                    msg.ChannelId == _cid && msg.Content.ToLower() == "hp" &&
                    ((!_pot1used && msg.Author.Id == _p1.Id) || (!_pot2used && msg.Author.Id == _p2.Id))
                , TimeSpan.FromSeconds(2)
            ).ConfigureAwait(false);
            if (reply != null) {
                if (reply.User.Id == _p1.Id && !_pot1used) {
                    _hp1 = (_hp1 + 1 > 5) ? 5 : _hp1 + 1;
                    _pot1used = true;
                    _events += $"\n{_p1.Username} {DiscordEmoji.FromName(_client, ":syringe:")}";
                } else if (reply.User.Id == _p2.Id && !_pot2used) {
                    _hp2 = (_hp2 + 1 > 5) ? 5 : _hp2 + 1;
                    _pot2used = true;
                    _events += $"\n{_p2.Username} {DiscordEmoji.FromName(_client, ":syringe:")}";
                }
            }
        }

        private async Task<string> CheckForFinishingMoveAsync()
        {
            var interactivity = _client.GetInteractivityModule();
            var reply = await interactivity.WaitForMessageAsync(
                msg =>
                    msg.ChannelId == _cid && msg.Author.Id == Winner.Id
                , TimeSpan.FromSeconds(20)
            ).ConfigureAwait(false);

            if (reply != null && !string.IsNullOrWhiteSpace(reply.Message?.Content))
                return reply.Message.Content;
            else
                return null;
        }
    }
}


