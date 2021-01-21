using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using TheGodfather.Common;
using TheGodfather.Modules.Polls.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Polls.Common
{
    public class ReactionsPoll : Poll
    {
        public new IReadOnlyCollection<PollEmoji>? Results { get; private set; }


        public ReactionsPoll(InteractivityExtension interactivity, DiscordChannel channel, DiscordMember sender, string question, TimeSpan runFor)
            : base(interactivity, channel, sender, question, runFor)
        {

        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            this.IsRunning = true;

            DiscordMessage msgHandle = await this.Channel.SendMessageAsync(embed: this.ToEmbed(lcs));

            this.Results = await this.Interactivity.DoPollAsync(
                msgHandle,
                Emojis.Numbers.All.Skip(1).Take(this.Options.Count).ToArray(),
                PollBehaviour.DeleteEmojis,
                this.TimeUntilEnd
            );

            await this.Channel.SendMessageAsync(embed: this.ResultsToDiscordEmbed(lcs));

            this.IsRunning = false;
        }
    }
}
