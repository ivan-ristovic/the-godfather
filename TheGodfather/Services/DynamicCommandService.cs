using System.Collections.Concurrent;
using Discord.Commands;

namespace TheGodfather.Services;

public sealed class DynamicCommandService : ITheGodfatherService
{
    public bool IsDisabled => false;

    private readonly ConcurrentDictionary<string, string> commands = new(StringComparer.OrdinalIgnoreCase);


    public bool Add(string name, string response)
        => this.commands.TryAdd(Normalize(name), response);

    public bool Remove(string name)
        => this.commands.TryRemove(Normalize(name), out _);

    public IReadOnlyDictionary<string, string> GetAll()
        => this.commands.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public async Task<bool> TryExecuteAsync(ShardedCommandContext context, string input)
    {
        (string? name, string? response) = this.Resolve(input);
        if (name is null || response is null)
            return false;

        string args = input.Length > name.Length ? input[name.Length..].Trim() : string.Empty;
        string output = response
            .Replace("%user%", context.User.Mention, StringComparison.OrdinalIgnoreCase)
            .Replace("%args%", args, StringComparison.OrdinalIgnoreCase);
        await context.Channel.SendMessageAsync(output);
        return true;
    }

    private (string? Name, string? Response) Resolve(string input)
    {
        string normalizedInput = input.Trim();
        foreach ((string name, string response) in this.commands.OrderByDescending(kvp => kvp.Key.Length))
            if (normalizedInput.Equals(name, StringComparison.OrdinalIgnoreCase)
                || normalizedInput.StartsWith(name + " ", StringComparison.OrdinalIgnoreCase))
                return (name, response);
        return (null, null);
    }

    private static string Normalize(string name)
        => name.Trim().ToLowerInvariant();
}
