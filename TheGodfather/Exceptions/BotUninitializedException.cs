namespace TheGodfather.Exceptions;

public sealed class BotUninitializedException : Exception
{
    public BotUninitializedException()
        : base("The bot is not initialized.")
    {
    }
}
