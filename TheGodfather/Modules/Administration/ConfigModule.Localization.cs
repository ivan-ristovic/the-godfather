using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration
{
    public sealed partial class ConfigModule
    {
        [Group("localization")]
        [Aliases("locale", "language", "lang", "region")]
        public sealed class LocalizationModule : TheGodfatherServiceModule<LocalizationService>
        {
            #region config localization
            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-locale")] string locale)
                => this.SetLocaleAsync(ctx, locale);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => ctx.InfoAsync(this.ModuleColor, "fmt-locale", this.Service.GetGuildLocale(ctx.Guild.Id));
            #endregion

            #region config localization set
            [Command("set")]
            [Aliases("change")]
            public async Task SetLocaleAsync(CommandContext ctx,
                                            [Description("desc-locale")] string locale)
            {
                if (!await this.Service.SetGuildLocaleAsync(ctx.Guild.Id, locale))
                    throw new CommandFailedException(ctx, "cmd-err-locale");

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-locale-change", locale);
                    emb.WithColor(this.ModuleColor);
                });
                await ctx.InfoAsync(this.ModuleColor, "evt-locale-change", locale);
            }
            #endregion

            #region config locale list
            [Command("list")]
            [Aliases("print", "show", "ls", "l", "p")]
            public Task ListLocalesAsync(CommandContext ctx)
            {
                IReadOnlyList<string> locales = this.Service.AvailableLocales;
                return ctx.PaginateAsync("str-locales-all", locales, s => s, this.ModuleColor);
            }
            #endregion
        }
    }
}
