using System;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Exceptions
{
    public class InvalidCommandUsageException : LocalizedException
    {
        // TODO remove
        public InvalidCommandUsageException(string? message = null)
            : base("")
        {
            throw new InvalidOperationException();
        }
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

        public InvalidCommandUsageException(CommandContext ctx, string key, Exception inner, params object[]? args)
            : base(ctx, key, inner, args)
        {

        }
    }
}
