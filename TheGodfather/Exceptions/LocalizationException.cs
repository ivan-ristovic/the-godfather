using System;
using DSharpPlus.CommandsNext;
using TheGodfather.Services;

namespace TheGodfather.Exceptions
{
    public class LocalizationException : LocalizedException
    {
        // TODO remove
        [Obsolete]
        public LocalizationException(string message, Exception inner)
            : base(null, message, inner)
        {
            throw new InvalidOperationException();
        }
        // END remove

        public LocalizationException(string rawMessage)
            : base(rawMessage)
        {

        }

        public LocalizationException(CommandContext ctx, params object?[]? args)
            : base(ctx, args)
        {

        }

        public LocalizationException(CommandContext ctx, string key, params object?[]? args)
            : base(ctx, key, args)
        {

        }

        public LocalizationException(CommandContext ctx, Exception inner, string key, params object?[]? args)
            : base(ctx, key, inner, args)
        {

        }

        public LocalizationException(LocalizationService lcs, ulong gid, params object?[]? args)
            : base(lcs, gid, args)
        {

        }

        public LocalizationException(LocalizationService lcs, ulong gid, string key, params object?[]? args)
            : base(lcs, gid, key, args)
        {

        }

        public LocalizationException(LocalizationService lcs, ulong gid, Exception inner, string key, params object?[]? args)
            : base(lcs, gid, key, inner, args)
        {

        }
    }
}
