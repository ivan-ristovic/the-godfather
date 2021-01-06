using System;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Exceptions
{
    public class InvalidCommandUsageException : LocalizedException
    {
        public InvalidCommandUsageException(CommandContext ctx, params object[]? args)
            : base(ctx, "cmd-inv-usage", args) { }

        public InvalidCommandUsageException(CommandContext ctx, string key, params object[]? args)
            : base(ctx, key, args) { }

        public InvalidCommandUsageException(CommandContext ctx, Exception inner, string key, params object[]? args)
            : base(ctx, key, inner, args) { }
    }
}
