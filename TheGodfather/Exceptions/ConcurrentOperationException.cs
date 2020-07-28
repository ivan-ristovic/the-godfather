using System;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Exceptions
{
    public class ConcurrentOperationException : LocalizedException
    {
        // TODO remove
        public ConcurrentOperationException(string message)
            : base(null)
        {
            throw new InvalidOperationException();
        }
        public ConcurrentOperationException(string message, Exception inner)
            : base(null)
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

        public ConcurrentOperationException(CommandContext ctx, string key, Exception inner, params object[]? args)
            : base(ctx, key, inner, args)
        {

        }
    }
}
