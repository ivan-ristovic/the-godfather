#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
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
            [UsageExampleArgs("locale", "locale sr-RS")]
            public class LocalizationModule : TheGodfatherServiceModule<LocalizationService>
            {

                public LocalizationModule(LocalizationService service, SharedData shared, DatabaseContextBuilder db)
                    : base(service, shared, db)
                {

                }


                [GroupCommand, Priority(1)]
                public Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Locale")] string locale)
                    => this.SetLocaleAsync(ctx, locale);

                [GroupCommand, Priority(0)]
                public Task ExecuteGroupAsync(CommandContext ctx)
                {
                    string locale = this.Service.GetGuildLocale(ctx.Guild.Id) ?? this.Shared.BotConfiguration.Locale;
                    return this.InformAsync(ctx, $"This guild locale: {locale}");
                }


                #region COMMAND_LOCALE_SET
                [Command("set")]
                [Description("Change the locale for the guild.")]
                [UsageExampleArgs("locale", "locale sr-RS")]
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
                    return ctx.SendCollectionInPagesAsync("Available locales", locales, s => s, this.ModuleColor);
                }
                #endregion
            }
        }
    }
}
