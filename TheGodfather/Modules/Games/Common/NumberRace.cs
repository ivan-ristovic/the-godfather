using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Games.Common
{
    public sealed class NumberRace : BaseChannelGame
    {
        public const int MaxParticipants = 10;

        public int ParticipantCount => this.participants.Count;
        public bool Started { get; private set; }

        private readonly ConcurrentHashSet<DiscordUser> participants;


        public NumberRace(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            this.Started = false;
            this.participants = new ConcurrentHashSet<DiscordUser>();
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            this.Started = true;

            int num = new SecureRandom().Next(1000);
            await this.Channel.EmbedAsync(num.ToString(), Emojis.ArrowUp);

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
                    await this.Channel.LocalizedEmbedAsync(lcs, Emojis.Dead, null, "fmt-game-nr-lost", mctx.Result.Author.Mention);
                    if (this.Winner is { } && this.Winner.Id == mctx.Result.Author.Id)
                        this.Winner = null;
                    this.participants.TryRemove(mctx.Result.Author);
                }
            }

            this.Winner = this.participants.First();
        }

        public bool AddParticipant(DiscordUser user) 
            => !this.participants.Contains(user) && this.participants.Add(user);
    }
}
