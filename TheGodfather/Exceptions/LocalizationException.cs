using System;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Exceptions
{
    public class LocalizationException : LocalizedException
    {
        // TODO remove
        [Obsolete]
        public LocalizationException(string message, Exception inner)
            : base(null, message, inner)
        {
            throw new InvalidOperationException();
        }
        // END remove

        public LocalizationException(string message)
            : base(message)
        {

        }

        public LocalizationException(CommandContext ctx, params object[]? args)
            : base(ctx, args)
        {

        }

        public LocalizationException(CommandContext ctx, string key, params object[]? args)
            : base(ctx, key, args)
        {

        }

        public LocalizationException(CommandContext ctx, string key, Exception inner, params object[]? args)
            : base(ctx, key, inner, args)
        {

        }
    }
}
