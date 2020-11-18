using System;
using DSharpPlus.CommandsNext;
using TheGodfather.Services;

namespace TheGodfather.Exceptions
{
    public class DatabaseOperationException : LocalizedException
    {
        public DatabaseOperationException(string rawMessage)
            : base(rawMessage)
        {

        }

        public DatabaseOperationException(CommandContext ctx, params object[]? args)
            : base(ctx, "err-db", args)
        {

        }

        public DatabaseOperationException(CommandContext ctx, string key, params object[]? args)
            : base(ctx, key, args)
        {

        }

        public DatabaseOperationException(CommandContext ctx, Exception inner, string key, params object[]? args)
            : base(ctx, key, inner, args)
        {

        }

        public DatabaseOperationException(LocalizationService lcs, ulong gid, params object?[]? args)
            : base(lcs, gid, args)
        {

        }

        public DatabaseOperationException(LocalizationService lcs, ulong gid, string key, params object?[]? args)
            : base(lcs, gid, key, args)
        {

        }

        public DatabaseOperationException(LocalizationService lcs, ulong gid, Exception inner, string key, params object?[]? args)
            : base(lcs, gid, key, inner, args)
        {

        }
    }
}
