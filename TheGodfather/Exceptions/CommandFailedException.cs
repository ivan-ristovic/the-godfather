using System;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Exceptions
{
    public class CommandFailedException : LocalizedException
    {
        // TODO remove
        [Obsolete]
        public CommandFailedException(string message)
            : base("")
        {
            throw new InvalidOperationException();
        }
        [Obsolete]
        public CommandFailedException(string message, Exception inner)
            : base("")
        {
            throw new InvalidOperationException();
        }
        // END remove


        public CommandFailedException(CommandContext ctx, params object[]? args) 
            : base(ctx, "cmd-fail", args)
        {

        }

        public CommandFailedException(CommandContext ctx, string key, params object[]? args)
            : base(ctx, key, args)
        {

        }

        public CommandFailedException(CommandContext ctx, Exception inner, string key, params object[]? args)
            : base(ctx, key, inner, args)
        {

        }
    }
}
