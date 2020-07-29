using System;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Exceptions
{
    public class CommandFailedException : LocalizedException
    {
        // TODO remove
        public CommandFailedException(string message)
            : base("")
        {
            throw new InvalidOperationException();
        }
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

        public CommandFailedException(CommandContext ctx, string key, Exception inner, params object[]? args)
            : base(ctx, key, inner, args)
        {

        }
    }
}
