using DSharpPlus.Entities;

namespace TheGodfather.Modules.Games.Common;

public interface IQuiz : IChannelEvent
{
    int NumberOfQuestions { get; }
    bool IsTimeoutReached { get; }
    IReadOnlyDictionary<DiscordUser, int> Results { get; }
}