using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using TheGodfather.Modules.Polls.Extensions;

namespace TheGodfather.Modules.Polls.Common;

public class ReactionsPoll : Poll
{
    public new IReadOnlyCollection<PollEmoji>? Results { get; private set; }


    public ReactionsPoll(InteractivityExtension interactivity, DiscordChannel channel, DiscordMember sender, string question, TimeSpan timeout)
        : base(interactivity, channel, sender, question, timeout) { }


    public override async Task RunAsync(LocalizationService lcs)
    {
        this.EndTime = DateTimeOffset.UtcNow + this.TimeoutAfter;
        this.IsRunning = true;

        DiscordMessage msgHandle = await this.Channel.SendMessageAsync(this.ToEmbed(lcs));

        this.Results = await this.Interactivity.DoPollAsync(
            msgHandle,
            Emojis.Numbers.All.Skip(1).Take(this.Options.Count).ToArray(),
            PollBehaviour.DeleteEmojis,
            this.TimeUntilEnd
        );

        await this.Channel.SendMessageAsync(this.ResultsToDiscordEmbed(lcs));

        this.IsRunning = false;
    }
}