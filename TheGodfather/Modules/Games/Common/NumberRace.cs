#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Extensions;
using TheGodfather.Common.Collections;
using TheGodfather.Modules.Games.Common;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class NumberRace : Game
    {
        private ConcurrentHashSet<DiscordUser> _participants = new ConcurrentHashSet<DiscordUser>();
        public int ParticipantCount => _participants.Count();
        public bool Started { get; private set; }


        public NumberRace(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            Started = false;
        }


        public override async Task RunAsync()
        {
            Started = true;

            int num = new Random().Next(1000);
            await _channel.SendIconEmbedAsync(num.ToString(), DiscordEmoji.FromUnicode("\U0001f199"))
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
                    await _channel.SendIconEmbedAsync($"{mctx.User.Mention} lost!", DiscordEmoji.FromUnicode("\u2757"))
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
