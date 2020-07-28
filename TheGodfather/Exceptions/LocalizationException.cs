using System;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Exceptions
{
    public class LocalizationException : LocalizedException
    {
        // TODO remove
        public LocalizationException(string message)
            : base(null)
        {
            throw new InvalidOperationException();
        }
        public LocalizationException(string message, Exception inner)
            : base(null)
        {
            throw new InvalidOperationException();
        }
        // END remove


        public LocalizationException(CommandContext ctx, params object[]? args)
            : base(ctx, "cmd-err-loc", args)
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
