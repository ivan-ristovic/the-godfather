using System;
using DSharpPlus.CommandsNext;
using TheGodfather.Translations;

namespace TheGodfather.Exceptions
{
    public class InvalidCommandUsageException : LocalizedException
    {
        public InvalidCommandUsageException(CommandContext ctx)
            : base(ctx, TranslationKey.cmd_err_inv_usage) { }

        public InvalidCommandUsageException(CommandContext ctx, TranslationKey key)
            : base(ctx, key) { }

        public InvalidCommandUsageException(CommandContext ctx, Exception inner, TranslationKey key)
            : base(ctx, inner, key) { }
    }
}
