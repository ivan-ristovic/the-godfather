#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Extensions.Collections;
using TheGodfather.Modules.Games.Common;

using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Games
{
    public class NumberRace : Game
    {
        private ConcurrentHashSet<DiscordUser> _participants = new ConcurrentHashSet<DiscordUser>();
        public int ParticipantCount => _participants.Count();
        public bool GameStarted { get; private set; }


        public NumberRace(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            GameStarted = false;
        }


        public override async Task RunAsync()
        {
            GameStarted = true;

            int num = new Random().Next(1000);
            await _channel.SendMessageAsync(num.ToString())
                .ConfigureAwait(false);
            
            while (ParticipantCount > 1) {
                int guess = 0;
                var mctx = await _interactivity.WaitForMessageAsync(
                    xm => {
                        if (xm.Channel.Id != _channel.Id || xm.Author.IsBot) return false;
                        if (!_participants.Contains(xm.Author)) return false;
                        return int.TryParse(xm.Content, out guess);
                    },
                    TimeSpan.FromSeconds(20)
                ).ConfigureAwait(false);

                if (mctx == null) {
                    NoReply = true;
                    return;
                } else if (guess == num + 1) {
                    num++;
                    Winner = mctx.User;
                } else {
                    await _channel.SendMessageAsync(mctx.User.Mention + " lost!")
                        .ConfigureAwait(false);
                    if (Winner != null && Winner.Id == mctx.User.Id)
                        Winner = null;
                    _participants.RemoveWhere(u => mctx.User.Id == u.Id);
                }
            }

            Winner = _participants.First();
        }

        public bool AddParticipant(DiscordUser user)
        {
            if (_participants.Any(u => user.Id == u.Id))
                return false;
            return _participants.Add(user);
        }
    }
}
