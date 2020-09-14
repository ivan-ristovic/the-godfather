using System;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Exceptions
{
    public class ConcurrentOperationException : LocalizedException
    {
        // TODO remove
        [Obsolete]
        public ConcurrentOperationException(string message)
            : base("")
        {
            throw new InvalidOperationException();
        }
        [Obsolete]
        public ConcurrentOperationException(string message, Exception inner)
            : base("")
        {
            throw new InvalidOperationException();
        }
        // END remove


        public ConcurrentOperationException(CommandContext ctx, params object[]? args)
            : base(ctx, "err-concurrent", args)
        {

        }

        public ConcurrentOperationException(CommandContext ctx, string key, params object[]? args)
            : base(ctx, key, args)
        {

        }

        public ConcurrentOperationException(CommandContext ctx, Exception inner, string key, params object[]? args)
            : base(ctx, key, inner, args)
        {

        }
    }
}
