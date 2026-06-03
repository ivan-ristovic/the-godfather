using System.Collections.Concurrent;
using Discord.Commands;

namespace TheGodfather.Attributes;

public enum CooldownBucketType
{
    Global,
    Guild,
    Channel,
    User
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class CooldownAttribute : PreconditionAttribute
{
    private static readonly ConcurrentDictionary<CooldownKey, Queue<DateTimeOffset>> _uses = new();

    private readonly int amount;
    private readonly int seconds;
    private readonly CooldownBucketType bucket;


    public CooldownAttribute(int amount, int seconds, CooldownBucketType bucket = CooldownBucketType.User)
    {
        this.amount = amount;
        this.seconds = seconds;
        this.bucket = bucket;
    }


    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset cutoff = now - TimeSpan.FromSeconds(this.seconds);
        var key = new CooldownKey(command.Aliases.FirstOrDefault() ?? command.Name, this.ResolveBucketId(context));
        Queue<DateTimeOffset> queue = _uses.GetOrAdd(key, _ => new Queue<DateTimeOffset>());

        lock (queue) {
            while (queue.Count > 0 && queue.Peek() <= cutoff)
                queue.Dequeue();

            if (queue.Count >= this.amount) {
                TimeSpan remaining = queue.Peek().AddSeconds(this.seconds) - now;
                return Task.FromResult(PreconditionResult.FromError($"Command is on cooldown for {remaining.TotalSeconds:0}s."));
            }

            queue.Enqueue(now);
        }

        return Task.FromResult(PreconditionResult.FromSuccess());
    }

    private ulong ResolveBucketId(ICommandContext context)
        => this.bucket switch {
            CooldownBucketType.Global => 0,
            CooldownBucketType.Guild => context.Guild?.Id ?? context.Channel.Id,
            CooldownBucketType.Channel => context.Channel.Id,
            CooldownBucketType.User => context.User.Id,
            _ => context.User.Id
        };


    private readonly record struct CooldownKey(string Command, ulong BucketId);
}
