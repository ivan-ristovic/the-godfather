using System;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Services;

namespace TheGodfather.Exceptions
{
    public abstract class LocalizedException : Exception
    {
        public string LocalizedMessage { get; }


        protected LocalizedException(string rawMessage)
            : base(rawMessage)
        {
            this.LocalizedMessage = rawMessage;
        }

        protected LocalizedException(CommandContext ctx, params object?[]? args)
            : base("err-loc")
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild?.Id, "err-loc", args);
        }

        protected LocalizedException(CommandContext ctx, string key, params object?[]? args)
            : base(key)
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild?.Id, key, args);
        }

        protected LocalizedException(CommandContext ctx, Exception inner, string key, params object?[]? args)
            : base(key, inner)
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild?.Id, key, args);
        }

        protected LocalizedException(LocalizationService lcs, ulong? gid, params object?[]? args)
            : base("err-loc")
        {
            this.LocalizedMessage = lcs.GetString(gid, "err-loc", args);
        }

        protected LocalizedException(LocalizationService lcs, ulong? gid, string key, params object?[]? args)
            : base(key)
        {
            this.LocalizedMessage = lcs.GetString(gid, key, args);
        }

        protected LocalizedException(LocalizationService lcs, ulong? gid, Exception inner, string key, params object?[]? args)
            : base(key, inner)
        {
            this.LocalizedMessage = lcs.GetString(gid, key, args);
        }
    }
}
