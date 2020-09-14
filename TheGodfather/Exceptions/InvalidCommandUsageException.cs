using System;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Exceptions
{
    public class InvalidCommandUsageException : LocalizedException
    {
        // TODO remove
        [Obsolete]
        public InvalidCommandUsageException(string? message = null)
            : base("")
        {
            throw new InvalidOperationException();
        }
        [Obsolete]
        public InvalidCommandUsageException(string message, Exception inner)
            : base("")
        {
            throw new InvalidOperationException();
        }
        // END remove


        public InvalidCommandUsageException(CommandContext ctx, params object[]? args)
            : base(ctx, "cmd-inv-usage", args)
        {

        }

        public InvalidCommandUsageException(CommandContext ctx, string key, params object[]? args)
            : base(ctx, key, args)
        {

        }

        public InvalidCommandUsageException(CommandContext ctx, Exception inner, string key, params object[]? args)
            : base(ctx, key, inner, args)
        {

        }
    }
}
