using System;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Services;
using TheGodfather.Translations;

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

        protected LocalizedException(CommandContext ctx, TranslationKey key)
            : base()
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild?.Id, key);
        }

        protected LocalizedException(CommandContext ctx, Exception inner, TranslationKey key)
            : base(null, inner)
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild?.Id, key);
        }

        protected LocalizedException(LocalizationService lcs, ulong? gid, TranslationKey key)
            : base()
        {
            this.LocalizedMessage = lcs.GetString(gid, key);
        }

        protected LocalizedException(LocalizationService lcs, ulong? gid, Exception inner, TranslationKey key)
            : base(null, inner)
        {
            this.LocalizedMessage = lcs.GetString(gid, key);
        }
    }
}
