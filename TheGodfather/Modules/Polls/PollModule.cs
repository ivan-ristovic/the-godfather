using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using TheGodfather.Modules.Polls.Common;
using TheGodfather.Modules.Polls.Extensions;

namespace TheGodfather.Modules.Polls;

[Group("poll")][Module(ModuleType.Polls)][NotBlocked][UsesInteractivity]
[Aliases("polls")]
[RequireGuild][Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class PollModule : TheGodfatherServiceModule<ChannelEventService>
{
    #region poll
    [GroupCommand][Priority(2)]
    public async Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_poll_t)] TimeSpan timeout,
        [RemainingText][Description(TranslationKey.desc_poll_q)] string question)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_poll_q_none);

        if (this.Service.IsEventRunningInChannel(ctx.Channel.Id, out _))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_poll_dup);

        if (timeout < TimeSpan.FromSeconds(Poll.MinTimeSeconds) || timeout >= TimeSpan.FromDays(Poll.MaxTimeDays))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_poll_time(Poll.MinTimeSeconds, Poll.MaxTimeDays));

        var poll = new Poll(ctx.Client.GetInteractivity(), ctx.Channel, ctx.Member, question, timeout);
        this.Service.RegisterEventInChannel(poll, ctx.Channel.Id);
        try {
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Question, TranslationKey.q_poll_ans);
            List<string>? options = await ctx.WaitAndParsePollOptionsAsync();
            if (options is null || options.Count < 2 || options.Count > Poll.MaxPollOptions)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_poll_opt(Poll.MaxPollOptions));
            poll.Options = options;

            await poll.RunAsync(this.Localization);
        } catch (TaskCanceledException) {
            await ctx.FailAsync(TranslationKey.cmd_err_poll_cancel);
        } finally {
            this.Service.UnregisterEventInChannel(ctx.Channel.Id);
        }
    }

    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_poll_q)] string question,
        [Description(TranslationKey.desc_poll_t)] TimeSpan timeout)
        => this.ExecuteGroupAsync(ctx, timeout, question);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_poll_q)] string question)
        => this.ExecuteGroupAsync(ctx, TimeSpan.FromMinutes(1), question);
    #endregion

    #region poll stop
    [Command("stop")]
    [Aliases("end", "cancel")]
    public Task StopAsync(CommandContext ctx)
    {
        Poll? poll = this.Service.GetEventInChannel<Poll>(ctx.Channel.Id);
        if (poll is null or ReactionsPoll)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_poll_none);

        if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.Administrator) && ctx.User != poll.Initiator)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_poll_cancel_perms);

        poll.Stop();
        this.Service.UnregisterEventInChannel(ctx.Channel.Id);

        return ctx.InfoAsync(this.ModuleColor);
    }
    #endregion
}