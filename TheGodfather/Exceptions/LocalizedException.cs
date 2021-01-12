using System;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Services;

namespace TheGodfather.Exceptions
{
    public abstract class LocalizedException : Exception
    {
        public string LocalizedMessage { get; }


        public LocalizedException(string rawMessage)
            : base(rawMessage)
        {
            this.LocalizedMessage = rawMessage;
        }

        public LocalizedException(CommandContext ctx, params object?[]? args)
            : base("err-loc")
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild?.Id, "err-loc", args);
        }

        public LocalizedException(CommandContext ctx, string key, params object?[]? args)
            : base(key)
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild?.Id, key, args);
        }

        public LocalizedException(CommandContext ctx, Exception inner, string key, params object?[]? args)
            : base(key, inner)
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild?.Id, key, args);
        }

        public LocalizedException(LocalizationService lcs, ulong? gid, params object?[]? args)
            : base("err-loc")
        {
            this.LocalizedMessage = lcs.GetString(gid, "err-loc", args);
        }

        public LocalizedException(LocalizationService lcs, ulong? gid, string key, params object?[]? args)
            : base(key)
        {
            this.LocalizedMessage = lcs.GetString(gid, key, args);
        }

        public LocalizedException(LocalizationService lcs, ulong? gid, Exception inner, string key, params object?[]? args)
            : base(key, inner)
        {
            this.LocalizedMessage = lcs.GetString(gid, key, args);
        }
    }
}
