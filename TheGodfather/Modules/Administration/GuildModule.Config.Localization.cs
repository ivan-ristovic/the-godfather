#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class GuildModule
    {
        public partial class GuildConfigModule
        {
            [Group("localization")]
            [Description("Change the bot locale (language and date formats) for this guild. Group call shows current guild locale.")]
            [Aliases("locale", "language", "lang", "region")]

            public class LocalizationModule : TheGodfatherServiceModule<LocalizationService>
            {

                public LocalizationModule(LocalizationService service, DbContextBuilder db)
                    : base(service, db)
                {

                }


                [GroupCommand, Priority(1)]
                public Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Locale")] string locale)
                    => this.SetLocaleAsync(ctx, locale);

                [GroupCommand, Priority(0)]
                public Task ExecuteGroupAsync(CommandContext ctx)
                    => this.InformAsync(ctx, $"This guild locale: {this.Service.GetGuildLocale(ctx.Guild.Id)}");


                #region COMMAND_LOCALE_SET
                [Command("set")]
                [Description("Change the locale for the guild.")]

                public async Task SetLocaleAsync(CommandContext ctx,
                                                [Description("Members to exempt.")] string locale)
                {
                    if (!await this.Service.SetGuildLocaleAsync(ctx.Guild.Id, locale))
                        throw new CommandFailedException("Given locale does not exist");

                    await this.InformAsync(ctx, "Successfully changed guild locale.", important: false);
                }
                #endregion

                #region COMMAND_LOCALE_LIST
                [Command("list")]
                [Description("List all available locales.")]
                [Aliases("l", "ls")]
                public Task ListLocalesAsync(CommandContext ctx)
                {
                    IReadOnlyList<string> locales = this.Service.AvailableLocales;
                    return ctx.PaginateAsync("Available locales", locales, s => s, this.ModuleColor);
                }
                #endregion
            }
        }
    }
}
