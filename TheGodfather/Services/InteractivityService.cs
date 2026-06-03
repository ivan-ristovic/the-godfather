using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using TheGodfather.Common.Collections;

namespace TheGodfather.Services;

public sealed class InteractivityService : ITheGodfatherService, IDisposable
{
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    public bool IsDisabled => false;

    private static readonly Regex _trueRegex = new(
        @"^(y+e*(s*|(a*|u*)(h*|p*))|d+a+(v+a+j+)?|1+|e+n+a+b+l+e|o+n+|t+(r+u+e+)?)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );
    private static readonly Regex _falseRegex = new(
        @"^(n+(o*(p+e+)?|a*h*|e*i*|y*e*t*)|0+|d+i+s+a+b+l+e|o+f+|f+(a+l+s+e+)?)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    private readonly ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>> pendingResponses;
    private readonly ConcurrentDictionary<Guid, MessageWaiter> messageWaiters;
    private readonly ConcurrentDictionary<Guid, ReactionWaiter> reactionWaiters;
    private readonly DiscordShardedClient? client;
    private bool disposed;


    public InteractivityService()
    {
        this.pendingResponses = new ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>>();
        this.messageWaiters = new ConcurrentDictionary<Guid, MessageWaiter>();
        this.reactionWaiters = new ConcurrentDictionary<Guid, ReactionWaiter>();
    }

    public InteractivityService(DiscordShardedClient client)
        : this()
    {
        this.client = client;
        this.client.MessageReceived += this.HandleMessageReceivedAsync;
        this.client.ReactionAdded += this.HandleReactionAddedAsync;
    }


    public void Dispose()
    {
        if (this.disposed)
            return;

        if (this.client is not null) {
            this.client.MessageReceived -= this.HandleMessageReceivedAsync;
            this.client.ReactionAdded -= this.HandleReactionAddedAsync;
        }

        foreach (MessageWaiter waiter in this.messageWaiters.Values)
            waiter.TrySetCanceled();
        foreach (ReactionWaiter waiter in this.reactionWaiters.Values)
            waiter.TrySetCanceled();

        this.messageWaiters.Clear();
        this.reactionWaiters.Clear();
        this.disposed = true;
    }


    public void AddPendingResponse(ulong cid, ulong uid)
    {
        this.pendingResponses.AddOrUpdate(
            cid,
            new ConcurrentHashSet<ulong> { uid },
            (_, v) => { v.Add(uid); return v; }
        );
    }

    public bool IsResponsePending(ulong cid, ulong uid)
        => this.pendingResponses.TryGetValue(cid, out ConcurrentHashSet<ulong>? pending) && pending.Contains(uid);

    public bool RemovePendingResponse(ulong cid, ulong uid)
    {
        if (!this.pendingResponses.TryGetValue(cid, out ConcurrentHashSet<ulong>? pending))
            return false;

        if (!pending.Contains(uid))
            return false;

        bool success = pending.TryRemove(uid);
        if (!this.pendingResponses[cid].Any())
            this.pendingResponses.TryRemove(cid, out _);
        return success;
    }

    public IDisposable TrackPendingResponse(ulong cid, ulong uid)
    {
        this.AddPendingResponse(cid, uid);
        return new PendingResponseScope(this, cid, uid);
    }

    public Task<InteractivityResult<SocketUserMessage>> WaitForMessageAsync(
        Func<SocketUserMessage, bool> predicate,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
        => this.WaitForMessageAsync(predicate, timeout ?? DefaultTimeout, cancellationToken);

    public async Task<InteractivityResult<SocketUserMessage>> WaitForMessageAsync(
        IMessageChannel channel,
        IUser user,
        Func<SocketUserMessage, bool>? predicate = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        using IDisposable _ = this.TrackPendingResponse(channel.Id, user.Id);
        return await this.WaitForMessageAsync(
            msg => !msg.Author.IsBot
                   && msg.Channel.Id == channel.Id
                   && msg.Author.Id == user.Id
                   && (predicate?.Invoke(msg) ?? true),
            timeout,
            cancellationToken
        );
    }

    public async Task<bool> WaitForBooleanReplyAsync(
        IMessageChannel channel,
        IUser user,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        bool response = false;
        InteractivityResult<SocketUserMessage> result = await this.WaitForMessageAsync(
            channel,
            user,
            msg => TryParseBooleanReply(msg.Content, out response),
            timeout,
            cancellationToken
        );

        return !result.TimedOut && response;
    }

    public async Task<int?> WaitForOptionReplyAsync(
        IMessageChannel channel,
        IUser user,
        int max,
        int min = 0,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        int index = 0;
        InteractivityResult<SocketUserMessage> result = await this.WaitForMessageAsync(
            channel,
            user,
            msg => int.TryParse(msg.Content, NumberStyles.Integer, CultureInfo.InvariantCulture, out index)
                   && index >= min
                   && index < max,
            timeout,
            cancellationToken
        );

        return result.TimedOut ? null : index;
    }

    public Task<InteractivityResult<SocketReaction>> WaitForReactionAsync(
        Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, bool> predicate,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
        => this.WaitForReactionAsync(predicate, timeout ?? DefaultTimeout, cancellationToken);

    public async Task SendPaginatedMessageAsync(
        IMessageChannel channel,
        IUser user,
        IReadOnlyList<Embed> pages,
        TimeSpan? timeout = null,
        string? content = null)
    {
        if (!pages.Any())
            return;

        if (pages.Count == 1) {
            await channel.SendMessageAsync(content, embed: pages[0]);
            return;
        }

        var controls = new[] {
            new Emoji("\u25c0\ufe0f"),
            new Emoji("\u25b6\ufe0f"),
            new Emoji("\u23f9\ufe0f")
        };

        int page = 0;
        IUserMessage message = await channel.SendMessageAsync(content, embed: pages[page]);
        foreach (Emoji control in controls)
            await message.AddReactionAsync(control);

        DateTimeOffset end = DateTimeOffset.UtcNow + (timeout ?? TimeSpan.FromMinutes(3));
        while (DateTimeOffset.UtcNow < end) {
            TimeSpan remaining = end - DateTimeOffset.UtcNow;
            InteractivityResult<SocketReaction> reaction = await this.WaitForReactionAsync(
                (messageRef, channelRef, socketReaction) =>
                    messageRef.Id == message.Id
                    && channelRef.Id == channel.Id
                    && socketReaction.UserId == user.Id
                    && controls.Any(control => string.Equals(control.Name, socketReaction.Emote.Name, StringComparison.Ordinal)),
                remaining
            );

            if (reaction.TimedOut || reaction.Result is null)
                break;

            string name = reaction.Result.Emote.Name;
            if (name == controls[2].Name)
                break;
            if (name == controls[0].Name)
                page = page == 0 ? pages.Count - 1 : page - 1;
            else if (name == controls[1].Name)
                page = (page + 1) % pages.Count;

            await message.ModifyAsync(props => props.Embed = pages[page]);
            await TryRemoveReactionAsync(message, reaction.Result.Emote, user);
        }

        await TryClearReactionsAsync(message);
    }

    public static bool TryParseBooleanReply(string value, out bool result)
    {
        value = value.Trim();
        if (_trueRegex.IsMatch(value)) {
            result = true;
            return true;
        }

        if (_falseRegex.IsMatch(value)) {
            result = false;
            return true;
        }

        return bool.TryParse(value, out result);
    }

    private async Task<InteractivityResult<SocketUserMessage>> WaitForMessageAsync(
        Func<SocketUserMessage, bool> predicate,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();
        var waiter = new MessageWaiter(predicate);
        this.messageWaiters[waiter.Id] = waiter;

        try {
            Task delay = Task.Delay(timeout, cancellationToken);
            Task winner = await Task.WhenAny(waiter.Task, delay);
            return winner == waiter.Task
                ? new InteractivityResult<SocketUserMessage>(await waiter.Task, timedOut: false)
                : InteractivityResult<SocketUserMessage>.Timeout;
        } finally {
            this.messageWaiters.TryRemove(waiter.Id, out _);
        }
    }

    private async Task<InteractivityResult<SocketReaction>> WaitForReactionAsync(
        Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, bool> predicate,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();
        var waiter = new ReactionWaiter(predicate);
        this.reactionWaiters[waiter.Id] = waiter;

        try {
            Task delay = Task.Delay(timeout, cancellationToken);
            Task winner = await Task.WhenAny(waiter.Task, delay);
            return winner == waiter.Task
                ? new InteractivityResult<SocketReaction>(await waiter.Task, timedOut: false)
                : InteractivityResult<SocketReaction>.Timeout;
        } finally {
            this.reactionWaiters.TryRemove(waiter.Id, out _);
        }
    }

    private Task HandleMessageReceivedAsync(SocketMessage rawMessage)
    {
        if (rawMessage is not SocketUserMessage message)
            return Task.CompletedTask;

        foreach (MessageWaiter waiter in this.messageWaiters.Values)
            waiter.TrySetResult(message);

        return Task.CompletedTask;
    }

    private Task HandleReactionAddedAsync(
        Cacheable<IUserMessage, ulong> messageRef,
        Cacheable<IMessageChannel, ulong> channelRef,
        SocketReaction reaction)
    {
        foreach (ReactionWaiter waiter in this.reactionWaiters.Values)
            waiter.TrySetResult(messageRef, channelRef, reaction);

        return Task.CompletedTask;
    }

    private static async Task TryRemoveReactionAsync(IUserMessage message, IEmote emote, IUser user)
    {
        try {
            await message.RemoveReactionAsync(emote, user);
        } catch (Exception e) when (e is Discord.Net.HttpException or UnauthorizedAccessException) {
            Log.Debug(e, "Failed to remove pagination reaction from message {MessageId}", message.Id);
        }
    }

    private static async Task TryClearReactionsAsync(IUserMessage message)
    {
        try {
            await message.RemoveAllReactionsAsync();
        } catch (Exception e) when (e is Discord.Net.HttpException or UnauthorizedAccessException) {
            Log.Debug(e, "Failed to clear pagination reactions from message {MessageId}", message.Id);
        }
    }

    private void ThrowIfDisposed()
    {
        if (this.disposed)
            throw new ObjectDisposedException(nameof(InteractivityService));
    }


    private sealed class PendingResponseScope : IDisposable
    {
        private readonly InteractivityService service;
        private readonly ulong cid;
        private readonly ulong uid;
        private bool disposed;


        public PendingResponseScope(InteractivityService service, ulong cid, ulong uid)
        {
            this.service = service;
            this.cid = cid;
            this.uid = uid;
        }


        public void Dispose()
        {
            if (this.disposed)
                return;

            this.service.RemovePendingResponse(this.cid, this.uid);
            this.disposed = true;
        }
    }

    private sealed class MessageWaiter
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Task<SocketUserMessage> Task => this.tcs.Task;

        private readonly Func<SocketUserMessage, bool> predicate;
        private readonly TaskCompletionSource<SocketUserMessage> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);


        public MessageWaiter(Func<SocketUserMessage, bool> predicate)
        {
            this.predicate = predicate;
        }


        public void TrySetResult(SocketUserMessage message)
        {
            try {
                if (this.predicate(message))
                    this.tcs.TrySetResult(message);
            } catch (Exception e) {
                this.tcs.TrySetException(e);
            }
        }

        public void TrySetCanceled()
            => this.tcs.TrySetCanceled();
    }

    private sealed class ReactionWaiter
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Task<SocketReaction> Task => this.tcs.Task;

        private readonly Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, bool> predicate;
        private readonly TaskCompletionSource<SocketReaction> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);


        public ReactionWaiter(Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, bool> predicate)
        {
            this.predicate = predicate;
        }


        public void TrySetResult(Cacheable<IUserMessage, ulong> messageRef, Cacheable<IMessageChannel, ulong> channelRef, SocketReaction reaction)
        {
            try {
                if (this.predicate(messageRef, channelRef, reaction))
                    this.tcs.TrySetResult(reaction);
            } catch (Exception e) {
                this.tcs.TrySetException(e);
            }
        }

        public void TrySetCanceled()
            => this.tcs.TrySetCanceled();
    }
}

public sealed class InteractivityResult<T>
{
    public static InteractivityResult<T> Timeout { get; } = new(default, timedOut: true);

    public T? Result { get; }
    public bool TimedOut { get; }
    public bool Succeeded => !this.TimedOut;


    public InteractivityResult(T? result, bool timedOut)
    {
        this.Result = result;
        this.TimedOut = timedOut;
    }
}
