using System;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Exceptions
{
    public class DatabaseOperationException : LocalizedException
    {
        // TODO remove
        public DatabaseOperationException(string message)
            : base("")
        {
            throw new InvalidOperationException();
        }
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

        public DatabaseOperationException(CommandContext ctx, string key, Exception inner, params object[]? args)
            : base(ctx, key, inner, args)
        {

        }
    }
}
