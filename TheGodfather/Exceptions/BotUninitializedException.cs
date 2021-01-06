using System;

namespace TheGodfather.Exceptions
{
    public sealed class BotUninitializedException : Exception
    {
        public BotUninitializedException()
            : base("Cannot retrieve this property since bot has not been started") { }
    }
}
