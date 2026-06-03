using System.Collections.Concurrent;
using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Polls;

[Group("poll")]
[Alias("polls")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public class PollModule : TheGodfatherModule
{
    private const int MaxPollOptions = 10;
    private static readonly ConcurrentDictionary<ulong, ActivePoll> _activePolls = new();

    protected virtual bool UsesReactionVoting => false;


    public PollModule(LocalizationService localization)
        : base(localization, new Color(0xF1C40F)) { }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(string timeout, [Remainder] string payload)
        => this.CreateAsync(timeout, payload);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync([Remainder] string payload)
        => this.CreateAsync("1m", payload);

    [Command("stop")]
    [Alias("end", "cancel")]
    public async Task StopAsync()
    {
        if (!_activePolls.TryGetValue(this.Context.Channel.Id, out ActivePoll? poll) || poll.UsesReactionVoting != this.UsesReactionVoting)
            throw new LocalizedCommandException(TranslationKey.cmd_err_poll_none);

        bool isAdmin = this.Context.User is IGuildUser member && member.GuildPermissions.Administrator;
        if (!isAdmin && poll.InitiatorId != this.Context.User.Id)
            throw new LocalizedCommandException(TranslationKey.cmd_err_poll_cancel_perms);

        poll.Cancel();
        await this.InfoAsync(this.ModuleColor);
    }

    protected async Task CreateAsync(string timeout, string payload)
    {
        if (_activePolls.ContainsKey(this.Context.Channel.Id))
            throw new LocalizedCommandException(TranslationKey.cmd_err_poll_dup);

        if (!TryParseDuration(timeout, out TimeSpan span))
            span = TimeSpan.FromMinutes(1);
        if (span < TimeSpan.FromSeconds(10) || span > TimeSpan.FromDays(1)) {
            await this.FailAsync("Poll timeout must be between 10 seconds and 1 day.");
            return;
        }

        string[] parts = payload.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        string question = parts.FirstOrDefault() ?? payload;
        string[] options = parts.Skip(1).Distinct().Take(MaxPollOptions).ToArray();
        if (string.IsNullOrWhiteSpace(question))
            throw new LocalizedCommandException(TranslationKey.cmd_err_poll_q_none);
        if (options.Length is < 2 or > MaxPollOptions)
            throw new LocalizedCommandException(TranslationKey.cmd_err_poll_opt(MaxPollOptions));

        var poll = new ActivePoll(
            this.Localization,
            this.Context.Channel,
            this.Context.User.Id,
            UserDisplayName(this.Context.User),
            question,
            options,
            DateTimeOffset.UtcNow + span,
            this.UsesReactionVoting
        );

        IUserMessage msg = await this.Context.Channel.SendMessageAsync(embed: poll.BuildEmbed(this.ModuleColor));
        poll.MessageId = msg.Id;
        if (!_activePolls.TryAdd(this.Context.Channel.Id, poll))
            throw new LocalizedCommandException(TranslationKey.cmd_err_poll_dup);

        if (this.UsesReactionVoting) {
            foreach (string reaction in poll.ReactionOptions)
                await msg.AddReactionAsync(new Emoji(reaction));
        }

        _ = Task.Run(() => CompletePollAsync(poll, this.ModuleColor));
    }

    internal static bool TryGetTextPoll(ulong channelId, out ActivePoll poll)
        => _activePolls.TryGetValue(channelId, out poll!) && !poll.UsesReactionVoting;

    internal static async Task UpdatePollMessageAsync(ActivePoll poll, Color color)
    {
        if (poll.Channel is not IMessageChannel channel)
            return;

        IUserMessage? message = await channel.GetMessageAsync(poll.MessageId) as IUserMessage;
        if (message is not null)
            await message.ModifyAsync(props => props.Embed = poll.BuildEmbed(color));
    }

    private static async Task CompletePollAsync(ActivePoll poll, Color color)
    {
        try {
            TimeSpan delay = poll.EndTime - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, poll.CancellationToken);
        } catch (TaskCanceledException) {
            // Manual stop.
        }

        _activePolls.TryRemove(poll.Channel.Id, out _);

        try {
            if (poll.Channel is not IMessageChannel channel)
                return;

            if (poll.UsesReactionVoting) {
                IUserMessage? message = await channel.GetMessageAsync(poll.MessageId) as IUserMessage;
                if (message is not null)
                    poll.ApplyReactionResults(message);
            }

            await channel.SendMessageAsync(embed: poll.BuildResultsEmbed(color));
        } finally {
            poll.Dispose();
        }
    }

    private static bool TryParseDuration(string value, out TimeSpan span)
    {
        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out span))
            return span > TimeSpan.Zero;
        if (value.Length < 2 || !double.TryParse(value[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out double amount)) {
            span = TimeSpan.Zero;
            return false;
        }
        span = value[^1] switch {
            's' or 'S' => TimeSpan.FromSeconds(amount),
            'm' or 'M' => TimeSpan.FromMinutes(amount),
            'h' or 'H' => TimeSpan.FromHours(amount),
            'd' or 'D' => TimeSpan.FromDays(amount),
            _ => TimeSpan.Zero
        };
        return span > TimeSpan.Zero;
    }
}

[Group("reactionpoll")]
[Alias("rpoll", "reactpoll")]
public sealed class ReactionsPollModule : PollModule
{
    protected override bool UsesReactionVoting => true;

    public ReactionsPollModule(LocalizationService localization)
        : base(localization) { }
}

[Group("vote")]
[Alias("voting")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class VotingModule : TheGodfatherModule
{
    public VotingModule(LocalizationService localization)
        : base(localization, new Color(0xF1C40F)) { }

    [Command("")]
    public async Task ExecuteGroupAsync(int option)
    {
        if (!PollModule.TryGetTextPoll(this.Context.Channel.Id, out ActivePoll poll))
            throw new LocalizedCommandException(TranslationKey.cmd_err_poll_none);

        int index = option - 1;
        if (!poll.IsValidVote(index))
            throw new LocalizedCommandException(TranslationKey.cmd_err_poll_opt_inv(poll.Options.Count));

        if (!poll.VoteFor(this.Context.User.Id, index))
            throw new LocalizedCommandException(TranslationKey.cmd_err_poll_vote);

        await PollModule.UpdatePollMessageAsync(poll, this.ModuleColor);
        await this.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_vote(this.Context.User.Mention, poll.Options[index], poll.Question));
    }

    [Command("cancel")]
    [Alias("c", "reset")]
    public async Task CancelAsync()
    {
        if (!PollModule.TryGetTextPoll(this.Context.Channel.Id, out ActivePoll poll))
            throw new LocalizedCommandException(TranslationKey.cmd_err_poll_none);

        if (!poll.UserVoted(this.Context.User.Id))
            throw new LocalizedCommandException(TranslationKey.cmd_err_poll_vote_cancel);

        if (!poll.CancelVote(this.Context.User.Id))
            throw new LocalizedCommandException(TranslationKey.cmd_err_poll_vote_cancel_fail);

        await PollModule.UpdatePollMessageAsync(poll, this.ModuleColor);
        await this.InfoAsync(this.ModuleColor);
    }
}

public sealed class ActivePoll : IDisposable
{
    public IReadOnlyList<string> Options { get; }
    public string Question { get; }
    public IMessageChannel Channel { get; }
    public ulong InitiatorId { get; }
    public string InitiatorName { get; }
    public DateTimeOffset EndTime { get; }
    public bool UsesReactionVoting { get; }
    public ulong MessageId { get; set; }
    public IReadOnlyList<string> ReactionOptions => Emojis.Numbers.All.Skip(1).Take(this.Options.Count).ToArray();
    public CancellationToken CancellationToken => this.cts.Token;

    private readonly LocalizationService localization;
    private readonly CancellationTokenSource cts;
    private readonly ConcurrentDictionary<ulong, int> votes;
    private readonly ConcurrentDictionary<int, int> reactionVotes;


    public ActivePoll(
        LocalizationService localization,
        IMessageChannel channel,
        ulong initiatorId,
        string initiatorName,
        string question,
        IReadOnlyList<string> options,
        DateTimeOffset endTime,
        bool usesReactionVoting)
    {
        this.localization = localization;
        this.Channel = channel;
        this.InitiatorId = initiatorId;
        this.InitiatorName = initiatorName;
        this.Question = question;
        this.Options = options;
        this.EndTime = endTime;
        this.UsesReactionVoting = usesReactionVoting;
        this.cts = new CancellationTokenSource();
        this.votes = new ConcurrentDictionary<ulong, int>();
        this.reactionVotes = new ConcurrentDictionary<int, int>();
    }


    public void Dispose()
        => this.cts.Dispose();

    public void Cancel()
        => this.cts.Cancel();

    public bool IsValidVote(int vote)
        => vote >= 0 && vote < this.Options.Count;

    public bool UserVoted(ulong userId)
        => this.votes.ContainsKey(userId);

    public bool VoteFor(ulong userId, int vote)
        => this.IsValidVote(vote) && this.votes.TryAdd(userId, vote);

    public bool CancelVote(ulong userId)
        => this.votes.TryRemove(userId, out _);

    public void ApplyReactionResults(IUserMessage message)
    {
        this.reactionVotes.Clear();
        for (int i = 0; i < this.ReactionOptions.Count; i++) {
            var emoji = new Emoji(this.ReactionOptions[i]);
            int count = message.Reactions.TryGetValue(emoji, out ReactionMetadata metadata)
                ? Math.Max(0, metadata.ReactionCount - 1)
                : 0;
            this.reactionVotes[i] = count;
        }
    }

    public Embed BuildEmbed(Color color)
    {
        ulong? gid = (this.Channel as IGuildChannel)?.GuildId;
        var emb = new LocalizedEmbedBuilder(this.localization, gid);
        emb.WithTitle(this.Question);
        emb.WithColor(color);
        emb.WithDescription(this.UsesReactionVoting
            ? this.localization.GetString(gid, TranslationKey.str_vote_react)
            : this.localization.GetString(gid, TranslationKey.str_vote_text));

        for (int i = 0; i < this.Options.Count; i++) {
            string title = this.UsesReactionVoting
                ? $"{this.ReactionOptions[i]} {i + 1}"
                : $"{i + 1} : {this.Options[i]}";
            string value = this.UsesReactionVoting
                ? this.Options[i]
                : this.votes.Count(kvp => kvp.Value == i).ToString(CultureInfo.InvariantCulture);
            emb.AddField(title, value, true);
        }

        string localizedTime = this.localization.GetLocalizedTimeString(gid, this.EndTime);
        TimeSpan remaining = this.EndTime - DateTimeOffset.UtcNow;
        emb.WithLocalizedFooter(
            remaining.TotalSeconds > 1
                ? TranslationKey.fmt_poll_end(localizedTime, $"{remaining:hh\\:mm\\:ss}")
                : TranslationKey.fmt_poll_ended,
            null
        );
        return emb.Build();
    }

    public Embed BuildResultsEmbed(Color color)
    {
        ulong? gid = (this.Channel as IGuildChannel)?.GuildId;
        var emb = new LocalizedEmbedBuilder(this.localization, gid);
        emb.WithLocalizedTitle(TranslationKey.fmt_poll_res(this.Question));
        emb.WithColor(color);
        emb.WithLocalizedFooter(TranslationKey.fmt_poll_by(this.InitiatorName), null);

        bool any = false;
        for (int i = 0; i < this.Options.Count; i++) {
            int count = this.UsesReactionVoting
                ? this.reactionVotes.GetValueOrDefault(i)
                : this.votes.Count(kvp => kvp.Value == i);
            any |= count > 0;
            emb.AddField(this.Options[i], count.ToString(CultureInfo.InvariantCulture), true);
        }

        if (!any)
            emb.WithLocalizedDescription(TranslationKey.str_poll_none);

        return emb.Build();
    }
}
