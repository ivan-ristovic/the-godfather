using System;
using DSharpPlus.CommandsNext;
using TheGodfather.Translations;

namespace TheGodfather.Exceptions
{
    public class CommandFailedException : LocalizedException
    {
        public CommandFailedException(CommandContext ctx)
            : base(ctx, TranslationKey.cmd_fail) { }

        public CommandFailedException(CommandContext ctx, TranslationKey key)
            : base(ctx, key) { }

        public CommandFailedException(CommandContext ctx, Exception inner, TranslationKey key)
            : base(ctx, inner, key) { }
    }
}
