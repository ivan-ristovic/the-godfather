#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class NumberRace : ChannelEvent
    {
        public int ParticipantCount => this.participants.Count();
        public bool Started { get; private set; }

        private readonly ConcurrentHashSet<DiscordUser> participants;


        public NumberRace(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            this.Started = false;
            this.participants = new ConcurrentHashSet<DiscordUser>();
        }


        public override async Task RunAsync()
        {
            this.Started = true;

            int num = GFRandom.Generator.Next(1000);
            await this.Channel.EmbedAsync(num.ToString(), StaticDiscordEmoji.ArrowUp);
            
            while (this.participants.Any()) {
                int guess = 0;
                InteractivityResult<DiscordMessage> mctx = await this.Interactivity.WaitForMessageAsync(
                    xm => {
                        if (xm.Channel.Id != this.Channel.Id || xm.Author.IsBot) return false;
                        if (!this.participants.Contains(xm.Author)) return false;
                        return int.TryParse(xm.Content, out guess);
                    },
                    TimeSpan.FromSeconds(20)
                );

                if (mctx.TimedOut) {
                    this.IsTimeoutReached = true;
                    return;
                } else if (guess == num + 1) {
                    num++;
                    this.Winner = mctx.Result.Author;
                } else {
                    await this.Channel.EmbedAsync($"{mctx.Result.Author.Mention} lost!", StaticDiscordEmoji.Dead);
                    if (!(this.Winner is null) && this.Winner.Id == mctx.Result.Author.Id)
                        this.Winner = null;
                    this.participants.RemoveWhere(u => mctx.Result.Author.Id == u.Id);
                }
            }

            this.Winner = this.participants.First();
        }

        public bool AddParticipant(DiscordUser user)
        {
            if (this.participants.Any(u => user.Id == u.Id))
                return false;
            return this.participants.Add(user);
        }
    }
}
