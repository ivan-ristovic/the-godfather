namespace TheGodfather.Exceptions;

public sealed class LocalizationException : Exception
{
    public LocalizationException(string message)
        : base(message)
    {
    }
}
