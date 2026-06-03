namespace TheGodfather.Exceptions;

public sealed class LocalizedCommandException : Exception
{
    public TranslationKey Key { get; }


    public LocalizedCommandException(TranslationKey key)
        : base(key.Key)
    {
        this.Key = key;
    }
}
