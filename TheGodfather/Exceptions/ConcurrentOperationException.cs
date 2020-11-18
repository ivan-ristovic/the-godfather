using System;
using DSharpPlus.CommandsNext;
using TheGodfather.Services;

namespace TheGodfather.Exceptions
{
    public class ConcurrentOperationException : LocalizedException
    {
        // TODO remove
        [Obsolete]
        public ConcurrentOperationException(string message, Exception inner)
            : base("")
        {
            throw new InvalidOperationException();
        }
        // END remove


        public ConcurrentOperationException(string rawMessage)
            : base(rawMessage)
        {

        }

        public ConcurrentOperationException(CommandContext ctx, params object[]? args)
            : base(ctx, "err-concurrent", args)
        {

        }

        public ConcurrentOperationException(CommandContext ctx, string key, params object[]? args)
            : base(ctx, key, args)
        {

        }

        public ConcurrentOperationException(CommandContext ctx, Exception inner, string key, params object[]? args)
            : base(ctx, key, inner, args)
        {

        }

        public ConcurrentOperationException(LocalizationService lcs, ulong gid, params object?[]? args)
            : base(lcs, gid, args)
        {

        }

        public ConcurrentOperationException(LocalizationService lcs, ulong gid, string key, params object?[]? args)
            : base(lcs, gid, key, args)
        {

        }

        public ConcurrentOperationException(LocalizationService lcs, ulong gid, Exception inner, string key, params object?[]? args)
            : base(lcs, gid, key, inner, args)
        {

        }
    }
}
