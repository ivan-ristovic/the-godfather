using System;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Exceptions
{
    public class CommandFailedException : LocalizedException
    {
        public CommandFailedException(CommandContext ctx, params object[]? args)
            : base(ctx, "cmd-fail", args) { }

        public CommandFailedException(CommandContext ctx, string key, params object[]? args)
            : base(ctx, key, args) { }

        public CommandFailedException(CommandContext ctx, Exception inner, string key, params object[]? args)
            : base(ctx, key, inner, args) { }
    }
}
