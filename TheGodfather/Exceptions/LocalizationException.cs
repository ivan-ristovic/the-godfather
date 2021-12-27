namespace TheGodfather.Exceptions
{
    public class LocalizationException : LocalizedException
    {
        public LocalizationException(string rawMessage)
            : base(rawMessage) {}
    }
}
