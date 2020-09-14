using System;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Exceptions
{
    public class DatabaseOperationException : LocalizedException
    {
        // TODO remove
        [Obsolete]
        public DatabaseOperationException(string message)
            : base("")
        {
            throw new InvalidOperationException();
        }
        [Obsolete]
        public DatabaseOperationException(string message, Exception inner)
            : base("")
        {
            throw new InvalidOperationException();
        }
        // END remove


        public DatabaseOperationException(CommandContext ctx, params object[]? args)
            : base(ctx, "err-db", args)
        {

        }

        public DatabaseOperationException(CommandContext ctx, string key, params object[]? args)
            : base(ctx, key, args)
        {

        }

        public DatabaseOperationException(CommandContext ctx, Exception inner, string key, params object[]? args)
            : base(ctx, key, inner, args)
        {

        }
    }
}
